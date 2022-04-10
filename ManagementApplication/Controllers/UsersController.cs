#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManagementApplication.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using ManagementApplication.Common;
using Management_Api.Models;
using ErrorHandling.Api.Models;
using System.Net;
using Management_Api.Services;

namespace ManagementApplication.Controllers
{
    [Authorize]
    //[TypeFilter(typeof(CustomAuthorizeFilter))]
    [CustomAuthorizeFilter]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ManagementContext _context;
        private readonly IConfiguration _config;

        public UsersController(ManagementContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // GET: api/Users
        /// <summary>
        /// 전체회원 조회
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> Getusers()

        {
            // DTO 모델 적용  -> ItemToDTO method로 분리
            //var userList = await _context.users.Select(
            //    x => new UserDTO (){ 
            //        UserId = x.UserId, UserName = x.UserName, UserRole = x.UserRole, UserNo = x.UserNo 
            //    }).ToListAsync();

            var userList = await _context.Users.Select(x => ItemToDTO(x)).ToListAsync();

            // temp data
            //if (userList.Count == 0)
            //{
            //    _context.Users.Add(new User { UserId = "hong", UserName = "홍길동", Password = "1234", UserRole = "Admin" });
            //    _context.Users.Add(new User { UserId = "lee", UserName = "이순신", Password = "1234", UserRole = "User" });
            //    await _context.SaveChangesAsync();
            //    userList = await _context.Users.Select(x => ItemToDTO(x)).ToListAsync();
            //}
            return Ok(userList);
        }

        // GET: api/Users/All
        ///<summary>
        /// 전체회원 상세 조회(관리자만 조회)
        ///</summary>
        [HttpGet]
        [Route("All")]
        //[Authorize]
        [Authorize(Policy = Policies.Admin)]
        public async Task<ActionResult<IEnumerable<User>>> Getusers_Admin()
        {
            //if (HttpContext.Session.GetInt32("USER_LOGIN_KEY") == null)
            //{
            //    return Unauthorized();
            //}

            var userList = await _context.Users.ToListAsync();

            return Ok(userList);
        }

        // GET: api/Users/5
        //[Authorize]
        /// <summary>
        /// 회원 상세 조회(관리자 또는 해당 유저만)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUsers(int id)
        {

            UserService service = new UserService();
            var user = await _context.Users.FindAsync(id);
            var userID = service.GetUserID(HttpContext.User);
            
            if (service.CheckIsAdmin(HttpContext.User) || user.UserId == userID)
            {
                //return ItemToDTO(users);
                return Ok(user);
            }
            else
            {
                return Unauthorized(new { Message = "Access Denied"});
            }
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// 회원정보 수정(관리자 또는 해당 유저만)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        //[Authorize]
        public async Task<IActionResult> PutUsers(int id, User user)
        {
            if (id != user.UserNo || !UsersExists(id))
            {
                throw new HttpException(1001, "User Update Fail");
            }

            UserService service = new UserService();
            var userID = service.GetUserID(HttpContext.User);
            if (service.CheckIsAdmin(HttpContext.User) || userID == user.UserId)
            {
                _context.Entry(user).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetUsers), new { id = user.UserNo }, ItemToDTO(user));
            }
            else
            {
                return Unauthorized(new { Message = "Access Denied" });
            }

        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        ///<summary>
        /// 회원가입
        ///</summary>
        ///<returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (String.IsNullOrEmpty(user.UserRole)) user.UserRole = "User";
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            //CreateAtAction - 성공 시 http 상태코드 201 return
            return CreatedAtAction(nameof(GetUsers), new { id = user.UserNo }, ItemToDTO(user));

        }

        // POST: api/Users/Login
        /// <summary>
        /// 로그인
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("Login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] User user)
        {

            // 필수 입력값 - UserId, Password
            if (ModelState.IsValid)
            {
                IActionResult response = Unauthorized(new { message = "invalid user" });

                // 사용자 확인
                // 아래와 같은 쿼리는 새로운 객체를 선언하여 가져 온 모델이 되어 메모리 누수 발생 
                //var curUser = await _context.users.FirstOrDefaultAsync(u => u.UserId == user.UserId && u.Password == user.Password);
                var curUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId.Equals(user.UserId) && u.Password.Equals(user.Password));

                if (curUser != null)
                {
                    // jwt 토큰 넘겨주기
                    var tokenString = GenerateJWTToken(curUser);
                    response = Ok(new { token = tokenString });

                    //if(Global.Session.ContainsKey(this.HttpContext.Session.Id))
                    //{

                    //} else
                    //{
                    //    Global.Session.AddOrUpdate(this.HttpContext.Session.Id, curUser, (key, existContext) => 
                    //    { 
                    //        existContext = curUser; 
                    //        return existContext; 
                    //    });

                    HttpContext.Session.SetInt32("USER_LOGIN_KEY", curUser.UserNo);
                    //    HttpContext.Session.SetString("SID", HttpContext.Session.Id);
                    //}

                }

                return response;

            }

            // 모델이 유효하지 않으면
            return BadRequest();

        }

        [Route("Logout")]
        [HttpPost]
        //[Authorize]
        public IActionResult Logout([FromBody] User user)
        {
            if (ModelState.IsValid)
            {
                //User u;
                //Global.Session.TryRemove(this.HttpContext.Session.Id, out u);
                //this.HttpContext.Session.Clear();
                HttpContext.Session.Remove("USER_LOGIN_KEY");
                return Ok(new { message = "logout", user = user.UserName });
            }
            return BadRequest();

        }

        /// <summary>
        /// 회원 삭제(관리자 또는 해당 유저만)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUsers(int id)
        {
            UserService service = new UserService();
            var userID = service.GetUserID(HttpContext.User);

            var users = await _context.Users.FindAsync(id);
            if (users == null)
            {
                throw new HttpException(1001, "User Delete Fail");
            }

            if (service.CheckIsAdmin(HttpContext.User) || users.UserId == userID)
            {
                _context.Users.Remove(users);
                await _context.SaveChangesAsync();

            }
            else
            {
                return Unauthorized();
            }

            return Ok(new { message = "User Delete Success", result = ItemToDTO(users) });
        }

        private bool UsersExists(int id)
        {
            return _context.Users.Any(e => e.UserNo == id);
        }

        /// <summary>
        /// jwt 생성
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        private string GenerateJWTToken(User userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); // .net core에 지원되는 algorithm으로 변경 (Sha512는 지원x)
            var claims = new[]
            {
               new Claim(JwtRegisteredClaimNames.Sub, userInfo.UserId),
               new Claim("role",userInfo.UserRole),
               new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static UserDTO ItemToDTO(User user) =>
            new UserDTO
            {
                UserIdx = user.UserNo,
                UserId = user.UserId,
                UserName = user.UserName,
                UserRole = user.UserRole,
            };





    }
}

#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ManagementApplication.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using ManagementApplication.Common;

namespace ManagementApplication.Controllers
{
    [Authorize]
    //[TypeFilter(typeof(CustomAuthorizeFilter))]
    [CustomAuthorizeFilter]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserContext _context;
        private readonly IConfiguration _config;

        public UsersController(UserContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // GET: api/Users
        /// <summary>
        /// 전체 사용자 조회
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> Getusers()
        {
            //var userList = await _context.users.ToListAsync();

            // DTO 모델 적용  -> ItemToDTO method로 분리
            //var userList = await _context.users.Select(
            //    x => new UserDTO (){ 
            //        UserId = x.UserId, UserName = x.UserName, UserRole = x.UserRole, UserIdx = x.UserIdx 
            //    }).ToListAsync();

            var userList = await _context.users.Select(x => ItemToDTO(x)).ToListAsync();
            
            // temp data
            if (userList.Count == 0)
            {
                _context.users.Add(new User { UserId = "hong", UserName = "홍길동", Password = "1234", UserRole = "Admin" });
                _context.users.Add(new User { UserId = "lee", UserName = "이순신", Password = "1234", UserRole = "User" });
                await _context.SaveChangesAsync();
                userList = await _context.users.Select(x => ItemToDTO(x)).ToListAsync();
            }
            return userList;
        }

        // GET: api/Users/All
        ///<summary>
        /// 전체사용자 조회(관리자)
        ///</summary>
        [HttpGet]
        [Route("All")]
        //[Authorize]
        [Authorize(Policy = Policies.Admin)]
        public async Task<ActionResult<IEnumerable<User>>> Getusers_Admin()
        {
            var userList = await _context.users.ToListAsync();

            return userList;
        }

        // GET: api/Users/5
        //[Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUsers(int id)
        {
            var users = await _context.users.FindAsync(id);

            if (users == null)
            {
                return NotFound();
            }

            return ItemToDTO(users);
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        //[Authorize]
        public async Task<IActionResult> PutUsers(int id, User user)
        {
            if (id != user.UserIdx)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsersExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
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
            _context.users.Add(user);
            await _context.SaveChangesAsync();
            //CreateAtAction - 성공 시 http 상태코드 201 return
            return CreatedAtAction(nameof(GetUsers), new { id = user.UserIdx }, ItemToDTO(user));

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
                var curUser = await _context.users.FirstOrDefaultAsync(u => u.UserId.Equals(user.UserId) && u.Password.Equals(user.Password));

                if (curUser != null)
                {
                    // jwt 토큰 넘겨주기
                    var tokenString = GenerateJWTToken(curUser);
                    response = Ok(new { token = tokenString });

                    if(Global.Session.ContainsKey(this.HttpContext.Session.Id))
                    {

                    } else
                    {
                        Global.Session.AddOrUpdate(this.HttpContext.Session.Id, curUser, (key, existContext) => 
                        { 
                            existContext = curUser; 
                            return existContext; 
                        });

                        HttpContext.Session.SetString("SID", HttpContext.Session.Id);
                    }

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
            if(ModelState.IsValid)
            {
                User u;
                Global.Session.TryRemove(this.HttpContext.Session.Id, out u);
                this.HttpContext.Session.Clear();
                return Ok(new { message = "logout", user = user.UserName});
            }
            return BadRequest();

        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        //[Authorize]
        public async Task<IActionResult> DeleteUsers(int id)
        {
            var users = await _context.users.FindAsync(id);
            if (users == null)
            {
                return NotFound();
            }

            _context.users.Remove(users);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsersExists(int id)
        {
            return _context.users.Any(e => e.UserIdx == id);
        }


        private string GenerateJWTToken(User userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); // .net core에 지원되는 algorithm으로 변경 (Sha512는 지원x)
            var claims = new[]
            {
               new Claim(JwtRegisteredClaimNames.Sub, userInfo.UserName),
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
                UserIdx = user.UserIdx,
                UserId = user.UserId,
                UserName = user.UserName,
                UserRole = user.UserRole,
            };

        private Object CheckIsAdmin()
        {
            // Token 발급 시 Claim 객체에 저장 한 사용자 정보 확인하기
            var currentUser = HttpContext.User;
            var IsAdmin = false;
            if (currentUser.HasClaim(c => c.Type == ClaimTypes.Role))
            {
                //return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status200OK, currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value);
                IsAdmin = currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value == "Admin" ? true : false;
                return IsAdmin;
            }
            else
            {
                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized, "My error message");
            }
        }

        private class APIResponse
        {
            public int StatusCode { get; set; }
            public string Message { get; set; }
            public Dictionary<string, List<string>> Error { get; set; }
        }
    }
}

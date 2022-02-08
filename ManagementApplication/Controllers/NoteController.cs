using ErrorHandling.Api.Models;
using Management_Api.Models;
using Management_Api.Services;
using ManagementApplication.Common;
using ManagementApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Management_Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [CustomAuthorizeFilter]
    public class NoteController : ControllerBase
    {
        private readonly ManagementContext _context;
        //private readonly IConfiguration _config;

        public NoteController(ManagementContext context)
        {
            _context = context;
        }
        // GET: api/<NoteController>
        /// <summary>
        /// 게시글조회
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Note>>> Get()
        {
            //if(HttpContext.Session.GetInt32("USER_LOGIN_KEY") == null)
            //{
            //    return Unauthorized();
            //}
            var list = await _context.Notes.ToListAsync();
            if (list.Count == 0)
            {
                _context.Notes.Add(new Note { NoteTitle = "첫번째 게시글", NoteContents = "안녕하세요?", UserID = "hong" });
                _context.Notes.Add(new Note { NoteTitle = "게시물 등록 테스트", NoteContents = "Hello!", UserID = "lee" });
                await _context.SaveChangesAsync();
                list = _context.Notes.ToList();

                _context.Users.Add(new User { UserId = "hong", UserName = "홍길동", Password = "1234", UserRole = "Admin" });
                _context.Users.Add(new User { UserId = "lee", UserName = "이순신", Password = "1234", UserRole = "User" });
                await _context.SaveChangesAsync();
            }
            return list;
        }

        // GET api/<NoteController>/5
        /// <summary>
        /// 게시판 상세 조회
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Note>> Get(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            return Ok(note);
        }

        // POST api/<NoteController>
        /// <summary>
        /// 게시글 작성
        /// </summary>
        /// <param name="value"></param>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Note model)
        {
            var result = Problem(detail: "Note Register Fail", statusCode: 1001);

            UserService _service = new UserService();
            var userID = _service.GetUserID(HttpContext.User);

            if (userID == model.UserID)
            {
                _context.Notes.Add(model);
                if (await _context.SaveChangesAsync() > 0)
                {
                    return CreatedAtAction(nameof(Get), new { id = model.NoteNo }, model);
                }
                else return result;
            }
            else return result;

            //var result = new { Message = "Register Fail" };
            //return new JsonResult(result)
            //{
            //    StatusCode = StatusCodes.Status501NotImplemented
            //};

        }

        // PUT api/<NoteController>/5
        /// <summary>
        /// 게시글 수정
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Note note)
        {
            UserService _service = new UserService();
            var userID = _service.GetUserID(HttpContext.User);
            if (userID == note.UserID)
            {
                _context.Entry(note).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), new { id = note.NoteNo }, note);
            }
            else
            {
                throw new HttpException(1001, "Note Register Fail");
            }
        }

        // DELETE api/<NoteController>/5
        /// <summary>
        /// 게시글 삭제
        /// </summary>
        /// <param name="id"></param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            UserService _service = new UserService();
            var userID = _service.GetUserID(HttpContext.User);
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                throw new HttpException(1001, "Note Delete Fail");
            }
            if (_service.CheckIsAdmin(HttpContext.User) || userID == note.UserID)
            {
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Delete Success", result = note });
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}

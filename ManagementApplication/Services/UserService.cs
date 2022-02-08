using System.Security.Claims;

namespace Management_Api.Services
{
    public class UserService
    {
        /// <summary>
        /// 관리자 여부 체크
        /// </summary>
        /// <returns></returns>
        public bool CheckIsAdmin(ClaimsPrincipal currentUser)
        {
            // Token 발급 시 Claim 객체에 저장 한 사용자 정보 확인하기
            var IsAdmin = false;
            if (currentUser.HasClaim(c => c.Type == ClaimTypes.Role))
            {
                //return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status200OK, currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value);
                IsAdmin = currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value == "Admin";
                return IsAdmin;
            }
            //else
            //{
            //return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized, "My error message");
            return IsAdmin;
            //}
        }

        /// <summary>
        /// Token 발급 시 Claim 객체에 저장 한 사용자 정보 확인하기
        /// </summary>
        /// <returns></returns>
            
        public string GetUserID(ClaimsPrincipal currentUser)
        {
            var currUserID = currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            return currUserID;
        }
    }
}

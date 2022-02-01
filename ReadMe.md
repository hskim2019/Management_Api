## jwt 인증을 적용한 API 구성해보기
---

## memo
---
* Built with
	- Framework : Asp.net core 6.0 (visual studio 2022)
	- Asp.net core web api
* Nuget package :  
	- Microsoft.EntityFrameworkCore
	- Microsoft.EntityFrameworkCore.InMemory
	- Microsoft.AspNetCore.Authentication.JwtBearer

## 접근 토큰 방식 인증
---
* 1. 사용자 로그인 -> 서버에서 jwt 응답(token 유효기간 설정)
* 2. 로그인 제외 API 요청 시 token 추가 -> 서버에서 요청에 포함 된 token이 변환된 것인지 검증하여 인증 시 응답

## step
---
#### 기본 api 구성
* 1) Users 모델 추가  
		- Models > User.cs
		- + DTO 모델
* 2) DB Context 추가  
		- Models > UserContext
* 3) DI 컨테이너에 DB Context 등록 & DB Context가 메모리 내 데이터베이스를 사용하도록 지정  
		- Program.cs
* 4) Controllers > ApiController 생성  
		- Controllers > UsersController.cs

#### jwt 인증 적용
* 5) JWT 인증 스키마를 등록  
		- Program.cs
		- appsettings.json
* 6) 인증정책 추가  
		- Common > Policies.cs
		- Admin/User 그룹으로 구분하여 인증처리
* 7) 서비스에 인증정책 사용 등록  
		- Program.cs
* 8) Login API에 jwt 인증 적용  
		- UserController.cs > Login & GenerateJWTToken
* 9) 인증이 필요한 컨트롤러에 [Authorize] 특성 추가 (인증 필요 없는 action 에 [AllowAnonymous] 특성 추가)

#### AuthorizeFilter 적용
* 10) 인증 필터
	- CustomAuthorizeFilter.cs 생성
	- 익명 접근(AllowAnonymous) 여부 metadata 로 확인 할 수 있음
	- 컨트롤러에 AuthorizeFilter(CustomAuthorizeFilter) 적용
	- 컨트롤러 실행 전 실행 할 것이 있으면 여기에 코드 추가 하기

* 11) 인증 오류 체크
	- HttpAppAuthorizationService.cs 생성
	- Program.cs 에 서비스 등록
	- 익명접근이 아닌 filter 를 거치는 action에 대해서 acthorizeResult를 통해서 user requirements 가 충족하는지 체크 할 수 있음
	- [참조](https://stackoverflow.com/questions/66662939/where-to-return-403-forbidden-status)

* 12) 인증 오류 처리
	- Program.cs

#### ActionFilter 적용
* 13) Action 실행 전, 후 적용할 필터  
	- ActionFilter.cs 생성
	- Programs.cs 에 service 등록

* pending...100)세션 test


## API 문서화
--- 
https://localhost:7084/swagger

- 회원가입 : POST /users
- 로그인 : POST /users/login
- 회원조회(전체) : GET /users

- 회원조회(전체_관리자) : GET /users/all
- 회원조회(상세) : GET /users/:UserIdx
- 회원정보수정: PUT /users
- 회원탈퇴 : DELETE /users

#### memo
---
* IAllowAnonymousFilter 가 core 3.0 이상에서는 not working 인 것 같아서 metadata 로 익명 접근 확인하도록 수정  
	- [참조_link](https://stackoverflow.com/questions/59305183/allowanonymous-attribute-is-not-working-in-net-core-api-2-2-please-consider)
	- [참조_link](https://stackoverflow.com/questions/60523559/check-whether-the-allow-anonymous-is-on-or-not-in-asp-net-core)


---
## Reference link
---
[참조link(msdn)](https://docs.microsoft.com/ko-kr/aspnet/core/tutorials/first-web-api?view=aspnetcore-6.0&tabs=visual-studio)  
[참조link_API구성](https://velog.io/@mygumi22/.NET-Core-3.1-Web-API-%EB%A7%8C%EB%93%A4%EA%B8%B0#orm-%EC%82%AC%EC%9A%A9%ED%95%98%EA%B8%B0)  
[참조link_API구성](https://blog.naver.com/PostView.naver?blogId=okcharles&logNo=222138969070&categoryNo=18&parentCategoryNo=0&viewDate=&currentPage=2&postListTopCurrentPage=1&from=postView&userTopListOpen=true&userTopListCount=5&userTopListManageOpen=false&userTopListCurrentPage=2)  

[참조link_jwt인증(msdn)](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-6.0)  
[참조link_jwt인증](https://lab.cliel.com/entry/ASPNET-Core-Web-API-JWT-%EC%9D%B8%EC%A6%9D)  
[참조link_jwt인증](https://jasonwatmore.com/post/2021/12/14/net-6-jwt-authentication-tutorial-with-example-api)  

[참조link_authorizefilter](https://referbruv.com/blog/posts/building-custom-responses-for-unauthorized-requests-in-aspnet-core)  
[참조link_authorizecheck](https://stackoverflow.com/questions/66662939/where-to-return-403-forbidden-status)
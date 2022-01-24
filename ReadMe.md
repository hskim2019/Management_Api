## memo
* Built with
	- Framework : Asp.net core 6.0 (visual studio 2022)
	- Asp.net core web api
* Nuget package :  
	- Microsoft.EntityFrameworkCore
	- Microsoft.EntityFrameworkCore.InMemory
	- Microsoft.AspNetCore.Authentication.JwtBearer

## step
#### �⺻ api ����
* 1) Users �� �߰�  
		- Models > User.cs
		- + DTO ��
* 2) DB Context �߰�  
		- Models > UserContext
* 3) DI �����̳ʿ� DB Context ��� & DB Context�� �޸� �� �����ͺ��̽��� ����ϵ��� ����  
		- Program.cs
* 4) Controllers > ApiController ����  
		- Controllers > UsersController.cs

#### jwt ���� ����
* 5) JWT ���� ��Ű���� ���  
		- Program.cs
		- appsettings.json
* 6) ������å �߰�  
		- Common > Policies.cs
		- Admin/User �׷����� �����Ͽ� ����ó��
* 7) ���񽺿� ������å ��� ���  
		- Program.cs
* 8) Login API�� jwt ���� ����  
		- UserController.cs > Login & GenerateJWTToken
* 9) ������ �ʿ��� ��Ʈ�ѷ��� [Authorize] Ư�� �߰�  

#### AuthorizeFilter ����
* 10) ���� ����
	- CustomAuthorizeFilter.cs ����
	- �͸� ����(AllowAnonymous) ���� metadata �� Ȯ�� �� �� ����
	- ******pending... CustomAuthorizeFilter ���� authorizeResult �� ���ؼ� 401 �ڵ� custom ó�� �Ǵ� Program.cs ���� Use������ ó��, 403 �ڵ�� filter �� ��ġ�� �ʾƼ� HttpAppAuthorizationService.cs ���� �׽�Ʈ ��...
			- [TEST](https://stackoverflow.com/questions/66662939/where-to-return-403-forbidden-status)
			- 1) HttpAppAuthorizationService.cs ����
			- 2) Program.cs - 1)�� ���� ���
			- 3) CustomAuthorizeFilter.cs �� OnResultExecutionAsync ������

#### ActionFilter ����
* 11) Action ���� ��, �� ������ ����  
	- ActionFilter.cs ����
	- Programs.cs �� service ���

* 100)���� test



## ���� ��ū ��� ����
* 1. ����� �α��� -> �������� jwt ����(token ��ȿ�Ⱓ ����)
* 2. �α��� ���� API ��û �� token �߰� -> �������� ��û�� ���� �� token�� ��ȯ�� ������ �����Ͽ� ���� �� ����

## API ����ȭ 
https://localhost:7084/swagger

- ȸ������ : POST /users
- �α��� : POST /users/login
- ȸ����ȸ(��ü) : GET /users

- ȸ����ȸ(��ü_������) : GET /users/all
- ȸ����ȸ(��) : GET /users/:UserIdx
- ȸ����������: PUT /users
- ȸ��Ż�� : DELETE /users

#### memo
* IAllowAnonymousFilter �� core 3.0 �̻󿡼��� not working �� �� ���Ƽ� metadata �� �͸� ���� Ȯ���ϵ��� ����  
	- [����_link](https://stackoverflow.com/questions/59305183/allowanonymous-attribute-is-not-working-in-net-core-api-2-2-please-consider)
	- [����_link](https://stackoverflow.com/questions/60523559/check-whether-the-allow-anonymous-is-on-or-not-in-asp-net-core)


---
## Reference link
---
[����link(msdn)](https://docs.microsoft.com/ko-kr/aspnet/core/tutorials/first-web-api?view=aspnetcore-6.0&tabs=visual-studio)
[����link_API����](https://velog.io/@mygumi22/.NET-Core-3.1-Web-API-%EB%A7%8C%EB%93%A4%EA%B8%B0#orm-%EC%82%AC%EC%9A%A9%ED%95%98%EA%B8%B0)
[����link_API����](https://blog.naver.com/PostView.naver?blogId=okcharles&logNo=222138969070&categoryNo=18&parentCategoryNo=0&viewDate=&currentPage=2&postListTopCurrentPage=1&from=postView&userTopListOpen=true&userTopListCount=5&userTopListManageOpen=false&userTopListCurrentPage=2)

[����link_jwt����(msdn)](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-6.0)
[����link_jwt����](https://lab.cliel.com/entry/ASPNET-Core-Web-API-JWT-%EC%9D%B8%EC%A6%9D)
[����link_jwt����](https://jasonwatmore.com/post/2021/12/14/net-6-jwt-authentication-tutorial-with-example-api)

[����link_authorizefilter](https://referbruv.com/blog/posts/building-custom-responses-for-unauthorized-requests-in-aspnet-core)
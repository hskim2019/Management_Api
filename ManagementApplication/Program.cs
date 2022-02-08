using ManagementApplication.Common;
using ManagementApplication.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;
using ErrorHandling.Api.Extensions;
//using Management_Api.Common;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// [3]-1 DI 컨테이너에 DB Context등록 & 데이터베이스 컨텍스트가 메모리 내 데이터베이스를 사용하도록 지정
builder.Services.AddDbContext<ManagementContext>(opt => opt.UseInMemoryDatabase("Management"));

// [5]-1 JWT 인증 스키마 등록 
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // token 발행자 사용 여부
        ValidateAudience = true, // token 받을 대상 사용 여부
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"], //일반적으로 jwt 인증을 수행하는 도메인
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
        ClockSkew = TimeSpan.Zero
    };

});


// [7] 서비스에 인증정책 사용 등록
builder.Services.AddAuthorization(config =>
{
    config.AddPolicy(Policies.Admin, Policies.AdminPolicy());
    config.AddPolicy(Policies.User, Policies.UserPolicy());
});

// [100]-1 세션 미들웨어 사용 등록
builder.Services.AddMvc();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);//Session Timeout.
});

// [11] : 인증오류 체크(Checks if a user meets a specific set of requirements and policies)
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // IHttpContextAccessor is no longer wired up by default, have to register first
builder.Services.AddSingleton<IAuthorizationService, HttpAppAuthorizationService>();

// [13] 액션 호출 될 때 마다 적용 할 것
builder.Services.AddControllers(option => 
{ 
    option.Filters.Add<ActionFilter>(); 
});


var app = builder.Build();

// [14] - ExceptionHandler
app.UseNativeGlobalExceptionHandler();

app.UseHttpsRedirection();
app.UseRouting();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // [3]-2 UseDeveloperExceptionPage
    //         개발 환경에서만 활성화 해야 함
    //         파이프라인에서 동기 및 비동기 예외 인스턴스를 캡처하고 HTML 오류 응답을 생성
    //app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
} 


// [12] 인증 오류 처리
app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized) // 401
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("Request Access Denied " + HttpStatusCode.Unauthorized.ToString());
    }

    if (context.Response.StatusCode == (int)HttpStatusCode.Forbidden) // 403
    {
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync("Request Access Denied " + HttpStatusCode.Unauthorized.ToString());
    }
});

app.UseAuthentication();
// [5]-2 JWT 인증 스키마 등록 
app.UseAuthorization();
// [100]-2 세션 미들웨어 사용 등록
app.UseSession();

//app.MapControllers();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();

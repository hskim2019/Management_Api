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

// [3]-1 DI �����̳ʿ� DB Context��� & �����ͺ��̽� ���ؽ�Ʈ�� �޸� �� �����ͺ��̽��� ����ϵ��� ����
builder.Services.AddDbContext<ManagementContext>(opt => opt.UseInMemoryDatabase("Management"));

// [5]-1 JWT ���� ��Ű�� ��� 
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // token ������ ��� ����
        ValidateAudience = true, // token ���� ��� ��� ����
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"], //�Ϲ������� jwt ������ �����ϴ� ������
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
        ClockSkew = TimeSpan.Zero
    };

});


// [7] ���񽺿� ������å ��� ���
builder.Services.AddAuthorization(config =>
{
    config.AddPolicy(Policies.Admin, Policies.AdminPolicy());
    config.AddPolicy(Policies.User, Policies.UserPolicy());
});

// [100]-1 ���� �̵���� ��� ���
builder.Services.AddMvc();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);//Session Timeout.
});

// [11] : �������� üũ(Checks if a user meets a specific set of requirements and policies)
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // IHttpContextAccessor is no longer wired up by default, have to register first
builder.Services.AddSingleton<IAuthorizationService, HttpAppAuthorizationService>();

// [13] �׼� ȣ�� �� �� ���� ���� �� ��
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
    //         ���� ȯ�濡���� Ȱ��ȭ �ؾ� ��
    //         ���������ο��� ���� �� �񵿱� ���� �ν��Ͻ��� ĸó�ϰ� HTML ���� ������ ����
    //app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
} 


// [12] ���� ���� ó��
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
// [5]-2 JWT ���� ��Ű�� ��� 
app.UseAuthorization();
// [100]-2 ���� �̵���� ��� ���
app.UseSession();

//app.MapControllers();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();

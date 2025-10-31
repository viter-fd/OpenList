using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using OpenList.Infrastructure.Data;
using OpenList.Core.Interfaces;
using OpenList.Infrastructure.Repositories;
using OpenList.Application.Interfaces;
using OpenList.Application.Services;
using OpenList.Infrastructure.Drivers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// 配置数据库
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 配置仓储和工作单元
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStorageRepository, StorageRepository>();
builder.Services.AddScoped<IShareRepository, ShareRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 配置应用服务
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<IFileSystemService, FileSystemService>();
builder.Services.AddScoped<IShareService, ShareService>();

// 配置存储驱动工厂
builder.Services.AddSingleton<IStorageDriverFactory, StorageDriverFactory>();

// 配置JWT认证
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 配置CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 配置Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OpenList API",
        Version = "v1",
        Description = "OpenList - 多存储文件列表系统 API",
        Contact = new OpenApiContact
        {
            Name = "OpenList Team",
            Url = new Uri("https://github.com/OpenListTeam/OpenList")
        },
        License = new OpenApiLicense
        {
            Name = "AGPL-3.0",
            Url = new Uri("https://www.gnu.org/licenses/agpl-3.0.txt")
        }
    });

    // 添加JWT认证支持
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// 应用数据库迁移
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenList API V1");
        c.RoutePrefix = string.Empty; // 设置Swagger UI为根路径
    });
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 启动信息
var serverConfig = builder.Configuration.GetSection("Server");
var port = serverConfig.GetValue<int>("Port", 5244);
var address = serverConfig.GetValue<string>("Address", "localhost");

Console.WriteLine("==================================================");
Console.WriteLine($"  OpenList API Server");
Console.WriteLine($"  Version: 1.0.0 (.NET 8.0 C# Edition)");
Console.WriteLine($"  Listening on: http://{address}:{port}");
Console.WriteLine($"  Swagger UI: http://{address}:{port}");
Console.WriteLine($"  Environment: {app.Environment.EnvironmentName}");
Console.WriteLine("==================================================");

app.Run($"http://{address}:{port}");

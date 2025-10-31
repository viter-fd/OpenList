using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OpenList.Application.DTOs;
using OpenList.Application.Interfaces;
using OpenList.Core.Entities;
using OpenList.Core.Interfaces;

namespace OpenList.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username);
        
        if (user == null)
        {
            throw new UnauthorizedAccessException("用户名或密码错误");
        }

        // 验证密码
        var hashedPassword = HashPassword(request.Password, user.Salt);
        if (user.PasswordHash != hashedPassword)
        {
            throw new UnauthorizedAccessException("用户名或密码错误");
        }

        // 检查用户是否启用
        if (user.IsDeleted)
        {
            throw new UnauthorizedAccessException("用户已被禁用");
        }

        // 生成 Token
        var token = GenerateJwtToken(user);
        
        return new LoginResponse
        {
            AccessToken = token,
            ExpiresIn = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "1440"),
            User = new UserDto
            {
                Id = (int)user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Role = user.Role.ToString(),
                Avatar = user.Avatar,
                CreatedAt = user.CreatedAt
            }
        };
    }

    public async Task<UserDto> CreateUserAsync(string username, string password, string displayName, string email, string role)
    {
        // 检查用户名是否已存在
        if (await _unitOfWork.Users.UsernameExistsAsync(username))
        {
            throw new InvalidOperationException("用户名已存在");
        }

        // 检查邮箱是否已存在
        if (!string.IsNullOrEmpty(email))
        {
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("邮箱已被使用");
            }
        }

        // 生成密码盐值和哈希
        var salt = GenerateSalt();
        var passwordHash = HashPassword(password, salt);

        // 解析角色
        if (!Enum.TryParse<UserRole>(role, true, out var userRole))
        {
            userRole = UserRole.User;
        }

        var user = new User
        {
            Username = username,
            PasswordHash = passwordHash,
            Salt = salt,
            DisplayName = displayName,
            Email = email,
            Role = userRole
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return new UserDto
        {
            Id = (int)user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = user.Role.ToString(),
            Avatar = user.Avatar,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured"));

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            return validatedToken != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return null;

        return new UserDto
        {
            Id = (int)user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = user.Role.ToString(),
            Avatar = user.Avatar,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("用户不存在");
        }

        // 验证旧密码
        var hashedOldPassword = HashPassword(oldPassword, user.Salt);
        if (user.PasswordHash != hashedOldPassword)
        {
            throw new UnauthorizedAccessException("原密码错误");
        }

        // 设置新密码
        var newSalt = GenerateSalt();
        user.Salt = newSalt;
        user.PasswordHash = HashPassword(newPassword, newSalt);

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured"));
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("DisplayName", user.DisplayName ?? user.Username)
        };

        if (!string.IsNullOrEmpty(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "1440")),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static string GenerateSalt()
    {
        var saltBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    private static string HashPassword(string password, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var combined = new byte[saltBytes.Length + passwordBytes.Length];
        
        Buffer.BlockCopy(saltBytes, 0, combined, 0, saltBytes.Length);
        Buffer.BlockCopy(passwordBytes, 0, combined, saltBytes.Length, passwordBytes.Length);
        
        var hash = SHA256.HashData(combined);
        return Convert.ToBase64String(hash);
    }
}

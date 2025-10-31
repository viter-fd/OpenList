using System.Security.Cryptography;
using OpenList.Application.DTOs;
using OpenList.Application.Interfaces;
using OpenList.Core.Entities;
using OpenList.Core.Interfaces;

namespace OpenList.Application.Services;

public class ShareService : IShareService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IShareRepository _shareRepository;

    public ShareService(IUnitOfWork unitOfWork, IShareRepository shareRepository)
    {
        _unitOfWork = unitOfWork;
        _shareRepository = shareRepository;
    }

    public async Task<ShareDto> CreateShareAsync(string path, int userId, string? password = null, 
        DateTime? expiresAt = null, int? maxDownloads = null)
    {
        // 生成唯一的分享码
        var code = await GenerateUniqueCodeAsync();

        var share = new Share
        {
            Code = code,
            Path = path,
            Password = password,
            ExpiresAt = expiresAt,
            MaxDownloads = maxDownloads,
            Downloads = 0,
            UserId = userId,
            IsEnabled = true
        };

        await _shareRepository.AddAsync(share);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(share);
    }

    public async Task<ShareDto?> GetShareByCodeAsync(string code, string? password = null)
    {
        var share = await _shareRepository.GetByCodeAsync(code);
        
        if (share == null || !share.IsEnabled)
        {
            return null;
        }

        // 检查是否过期
        if (share.ExpiresAt.HasValue && share.ExpiresAt.Value < DateTime.UtcNow)
        {
            return null;
        }

        // 检查下载次数
        if (share.MaxDownloads.HasValue && share.Downloads >= share.MaxDownloads.Value)
        {
            return null;
        }

        // 检查密码
        if (!string.IsNullOrEmpty(share.Password) && share.Password != password)
        {
            throw new UnauthorizedAccessException("分享密码错误");
        }

        return MapToDto(share);
    }

    public async Task<List<ShareDto>> GetUserSharesAsync(int userId)
    {
        var shares = await _shareRepository.GetByUserIdAsync(userId);
        return shares.Select(MapToDto).ToList();
    }

    public async Task<bool> DeleteShareAsync(string code, int userId)
    {
        var share = await _shareRepository.GetByCodeAsync(code);
        
        if (share == null || share.UserId != userId)
        {
            return false;
        }

        _shareRepository.Delete(share);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ToggleShareAsync(string code, int userId)
    {
        var share = await _shareRepository.GetByCodeAsync(code);
        
        if (share == null || share.UserId != userId)
        {
            return false;
        }

        share.IsEnabled = !share.IsEnabled;
        _shareRepository.Update(share);
        await _unitOfWork.SaveChangesAsync();

        return share.IsEnabled;
    }

    public async Task<bool> IncrementDownloadAsync(string code)
    {
        var share = await _shareRepository.GetByCodeAsync(code);
        
        if (share == null)
        {
            return false;
        }

        share.Downloads++;
        _shareRepository.Update(share);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private async Task<string> GenerateUniqueCodeAsync()
    {
        string code;
        do
        {
            code = GenerateRandomCode(8);
        } while (await _shareRepository.CodeExistsAsync(code));

        return code;
    }

    private static string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        var bytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        
        return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
    }

    private static ShareDto MapToDto(Share share)
    {
        return new ShareDto
        {
            Code = share.Code,
            Path = share.Path,
            HasPassword = !string.IsNullOrEmpty(share.Password),
            ExpiresAt = share.ExpiresAt,
            MaxDownloads = share.MaxDownloads,
            Downloads = share.Downloads,
            IsEnabled = share.IsEnabled,
            CreatedAt = share.CreatedAt,
            UserId = share.UserId
        };
    }
}

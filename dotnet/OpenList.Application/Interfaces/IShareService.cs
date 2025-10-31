using OpenList.Application.DTOs;

namespace OpenList.Application.Interfaces;

public interface IShareService
{
    Task<ShareDto> CreateShareAsync(string path, int userId, string? password = null, DateTime? expiresAt = null, int? maxDownloads = null);
    Task<ShareDto?> GetShareByCodeAsync(string code, string? password = null);
    Task<List<ShareDto>> GetUserSharesAsync(int userId);
    Task<bool> DeleteShareAsync(string code, int userId);
    Task<bool> ToggleShareAsync(string code, int userId);
    Task<bool> IncrementDownloadAsync(string code);
}

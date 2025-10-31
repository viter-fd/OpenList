using OpenList.Core.Entities;

namespace OpenList.Core.Entities;

public class Share : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? Password { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? MaxDownloads { get; set; }
    public int Downloads { get; set; }
    public long UserId { get; set; }
    public User? User { get; set; }
    public bool IsEnabled { get; set; } = true;
}

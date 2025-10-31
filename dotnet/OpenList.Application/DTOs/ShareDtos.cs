namespace OpenList.Application.DTOs;

public class ShareDto
{
    public string Code { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool HasPassword { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? MaxDownloads { get; set; }
    public int Downloads { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public long UserId { get; set; }
}

public class CreateShareRequest
{
    public string Path { get; set; } = string.Empty;
    public string? Password { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? MaxDownloads { get; set; }
}

public class AccessShareRequest
{
    public string Code { get; set; } = string.Empty;
    public string? Password { get; set; }
}

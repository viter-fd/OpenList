namespace OpenList.Core.Models;

/// <summary>
/// 文件信息
/// </summary>
public class FileItem
{
    /// <summary>
    /// 文件名
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 文件路径
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// 文件大小
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// 是否是目录
    /// </summary>
    public bool IsDirectory { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime Modified { get; set; }

    /// <summary>
    /// 修改时间（兼容属性）
    /// </summary>
    public DateTime ModifiedTime
    {
        get => Modified;
        set => Modified = value;
    }

    /// <summary>
    /// 签名
    /// </summary>
    public string? Sign { get; set; }

    /// <summary>
    /// 缩略图
    /// </summary>
    public string? Thumbnail { get; set; }

    /// <summary>
    /// 类型（数字）
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// 文件类型（字符串）
    /// </summary>
    public string? FileType { get; set; }

    /// <summary>
    /// MIME类型
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// 哈希信息
    /// </summary>
    public Dictionary<string, string>? HashInfo { get; set; }
}

/// <summary>
/// 文件列表
/// </summary>
public class FileList
{
    /// <summary>
    /// 当前路径
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// 文件项列表
    /// </summary>
    public List<FileItem> Items { get; set; } = new();

    /// <summary>
    /// 总数
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// README内容
    /// </summary>
    public string? Readme { get; set; }

    /// <summary>
    /// Header内容
    /// </summary>
    public string? Header { get; set; }

    /// <summary>
    /// 是否支持写入
    /// </summary>
    public bool Write { get; set; }
}

/// <summary>
/// 文件流信息
/// </summary>
public class FileStreamInfo
{
    /// <summary>
    /// 流
    /// </summary>
    public Stream? Stream { get; set; }

    /// <summary>
    /// URL（如果是重定向）
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// MIME类型
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// Content-Type（兼容属性）
    /// </summary>
    public string? ContentType
    {
        get => MimeType;
        set => MimeType = value;
    }

    /// <summary>
    /// 文件名
    /// </summary>
    public string? FileName { get; set; }
}

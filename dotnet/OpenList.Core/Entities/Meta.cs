namespace OpenList.Core.Entities;

/// <summary>
/// 元信息实体
/// </summary>
public class Meta : BaseEntity
{
    /// <summary>
    /// 路径
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// 是否隐藏
    /// </summary>
    public bool Hide { get; set; }

    /// <summary>
    /// README内容
    /// </summary>
    public string? Readme { get; set; }

    /// <summary>
    /// Header内容
    /// </summary>
    public string? Header { get; set; }

    /// <summary>
    /// 写入模式
    /// </summary>
    public bool Write { get; set; }

    /// <summary>
    /// WebDAV策略
    /// </summary>
    public string? WebdavPolicy { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}

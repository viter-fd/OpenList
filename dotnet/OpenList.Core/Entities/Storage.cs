namespace OpenList.Core.Entities;

/// <summary>
/// 存储实体
/// </summary>
public class Storage : BaseEntity
{
    /// <summary>
    /// 挂载路径
    /// </summary>
    public required string MountPath { get; set; }

    /// <summary>
    /// 存储名称
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 驱动类型
    /// </summary>
    public required string Driver { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 配置JSON
    /// </summary>
    public required string ConfigJson { get; set; }

    /// <summary>
    /// 提取文件夹
    /// </summary>
    public bool ExtractFolder { get; set; }

    /// <summary>
    /// 提取文件
    /// </summary>
    public bool ExtractFile { get; set; }

    /// <summary>
    /// Web代理
    /// </summary>
    public bool WebProxy { get; set; }

    /// <summary>
    /// WebDAV策略
    /// </summary>
    public string? WebdavPolicy { get; set; }

    /// <summary>
    /// 下载代理URL
    /// </summary>
    public string? DownProxyUrl { get; set; }

    /// <summary>
    /// 缓存过期时间（分钟）
    /// </summary>
    public int CacheExpiration { get; set; } = 30;

    /// <summary>
    /// 所属用户ID
    /// </summary>
    public long? UserId { get; set; }

    /// <summary>
    /// 所属用户
    /// </summary>
    public User? User { get; set; }
}

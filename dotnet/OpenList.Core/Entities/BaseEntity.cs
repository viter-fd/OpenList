namespace OpenList.Core.Entities;

/// <summary>
/// 实体基类
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 是否已删除
    /// </summary>
    public bool IsDeleted { get; set; }
}

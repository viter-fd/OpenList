namespace OpenList.Core.Entities;

/// <summary>
/// 设置实体
/// </summary>
public class Setting : BaseEntity
{
    /// <summary>
    /// 设置键
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// 设置值
    /// </summary>
    public required string Value { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 类型
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// 分组
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// 标志
    /// </summary>
    public int Flag { get; set; }
}

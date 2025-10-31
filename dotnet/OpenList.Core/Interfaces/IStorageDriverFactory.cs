using System.Threading;
using System.Threading.Tasks;

namespace OpenList.Core.Interfaces;

/// <summary>
/// 存储驱动工厂接口
/// </summary>
public interface IStorageDriverFactory
{
    /// <summary>
    /// 创建存储驱动实例
    /// </summary>
    /// <param name="driverName">驱动名称</param>
    /// <param name="configJson">配置JSON</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>存储驱动实例</returns>
    Task<IStorageDriver> CreateDriverAsync(string driverName, string configJson, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取所有可用的驱动名称
    /// </summary>
    /// <returns>驱动名称列表</returns>
    IEnumerable<string> GetAvailableDrivers();
}

using System.Text.Json;
using OpenList.Core.Interfaces;

namespace OpenList.Infrastructure.Drivers;

public class StorageDriverFactory : IStorageDriverFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Type> _drivers;

    public StorageDriverFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _drivers = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        RegisterDrivers();
    }

    public async Task<IStorageDriver> CreateDriverAsync(string driverName, string configJson, CancellationToken cancellationToken = default)
    {
        if (!_drivers.TryGetValue(driverName, out var driverType))
        {
            throw new NotSupportedException($"不支持的存储驱动: {driverName}");
        }

        // 创建驱动实例
        var driver = (IStorageDriver)(Activator.CreateInstance(driverType) 
            ?? throw new InvalidOperationException($"无法创建驱动实例: {driverName}"));

        // 异步初始化
        await driver.InitAsync(configJson, cancellationToken);

        return driver;
    }

    public IEnumerable<string> GetAvailableDrivers()
    {
        return _drivers.Keys;
    }

    private void RegisterDrivers()
    {
        // 仅注册本地驱动，其他驱动待接口修复后再注册
        _drivers["local"] = typeof(LocalStorageDriver);
    }
}

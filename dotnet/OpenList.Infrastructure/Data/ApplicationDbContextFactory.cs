using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OpenList.Infrastructure.Data;

/// <summary>
/// 设计时数据库上下文工厂
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlite("Data Source=openlist.db");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

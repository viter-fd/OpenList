using Microsoft.EntityFrameworkCore;
using OpenList.Core.Entities;

namespace OpenList.Infrastructure.Data;

/// <summary>
/// 应用数据库上下文
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// 用户
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// 存储
    /// </summary>
    public DbSet<Storage> Storages { get; set; }

    /// <summary>
    /// 元信息
    /// </summary>
    public DbSet<Meta> Metas { get; set; }

    /// <summary>
    /// 设置
    /// </summary>
    public DbSet<Setting> Settings { get; set; }

    /// <summary>
    /// 分享
    /// </summary>
    public DbSet<Share> Shares { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User配置
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DisplayName).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(200);
            
            entity.HasMany(e => e.Storages)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Storage配置
        modelBuilder.Entity<Storage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MountPath).IsUnique();
            entity.Property(e => e.MountPath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Driver).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ConfigJson).IsRequired();
        });

        // Meta配置
        modelBuilder.Entity<Meta>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Path).IsUnique();
            entity.Property(e => e.Path).IsRequired().HasMaxLength(1000);
        });

        // Setting配置
        modelBuilder.Entity<Setting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Value).IsRequired();
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
        });

        // Share配置
        modelBuilder.Entity<Share>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Path).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Password).HasMaxLength(100);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 软删除全局过滤器
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Storage>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Meta>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Setting>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Share>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 自动更新时间戳
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity && (
                e.State == EntityState.Added || 
                e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (BaseEntity)entityEntry.Entity;
            entity.UpdatedAt = DateTime.UtcNow;

            if (entityEntry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

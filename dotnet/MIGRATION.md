# OpenList Go to .NET Migration Guide

## 从 Go 版本迁移到 .NET 版本指南

本指南将帮助您从 OpenList 的 Go 版本迁移到 .NET/C# 版本。

## 📋 迁移前准备

### 1. 备份数据

在迁移前,请务必备份您的数据:

```bash
# 备份 Go 版本的数据目录
cp -r /path/to/go-openlist/data /backup/location/

# 备份数据库
cp /path/to/go-openlist/data/data.db /backup/location/
```

### 2. 导出配置

记录您当前的配置信息:
- 存储挂载点配置
- 用户账户信息
- 系统设置

## 🔄 数据迁移

### 数据库迁移

Go 版本使用 SQLite 数据库,与 .NET 版本兼容。但表结构可能有差异。

#### 方式一: 手动迁移(推荐)

1. **导出 Go 版本数据**

```bash
# 连接到 Go 版本数据库
sqlite3 /path/to/go-version/data.db

# 导出用户数据
.mode csv
.headers on
.output users.csv
SELECT * FROM x_users;
.output users.csv

# 导出存储数据
.output storages.csv
SELECT * FROM x_storages;
.output storages.csv
```

2. **导入到 .NET 版本**

使用提供的迁移脚本或手动创建数据。

#### 方式二: 使用迁移工具(开发中)

```bash
# 使用迁移工具
dotnet run --project OpenList.MigrationTool -- \
  --source /path/to/go-version/data.db \
  --target /path/to/dotnet-version/openlist.db
```

## 🗺️ 功能对应关系

### API 端点映射

| Go 版本 | .NET 版本 | 说明 |
|---------|-----------|------|
| `GET /api/public/settings` | `GET /api/system/info` | 系统信息 |
| `POST /api/auth/login` | `POST /api/auth/login` | 用户登录 |
| `GET /api/fs/list` | `GET /api/fs/list` | 文件列表 |
| `GET /api/fs/get` | `GET /api/fs/get` | 获取文件 |
| `POST /api/fs/form` | `POST /api/fs/upload` | 上传文件 |
| `GET /api/admin/storage/list` | `GET /api/storage` | 存储列表 |
| `POST /api/admin/storage/create` | `POST /api/storage` | 创建存储 |
| `POST /api/admin/user/create` | `POST /api/user` | 创建用户 |

### 配置文件映射

#### Go 版本 (config.json)
```json
{
  "force": false,
  "site_url": "",
  "cdn": "",
  "jwt_secret": "random_generated",
  "token_expires_in": 48,
  "database": {
    "type": "sqlite3",
    "host": "",
    "port": 0,
    "user": "",
    "password": "",
    "name": "",
    "db_file": "data\\data.db"
  },
  "scheme": {
    "address": "0.0.0.0",
    "http_port": 5244,
    "https_port": -1
  }
}
```

#### .NET 版本 (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=openlist.db"
  },
  "Jwt": {
    "Secret": "your-secret-key-must-be-at-least-32-characters-long",
    "Issuer": "OpenList",
    "Audience": "OpenList",
    "ExpirationMinutes": 1440
  },
  "Server": {
    "Port": 5244,
    "Address": "0.0.0.0"
  }
}
```

### 存储驱动映射

| Go Driver | .NET Driver | 状态 |
|-----------|-------------|------|
| `Local` | `LocalStorageDriver` | ✅ 已实现 |
| `AliyunDrive` | `AliyunStorageDriver` | 🚧 开发中 |
| `OneDrive` | `OneDriveStorageDriver` | 🚧 开发中 |
| `S3` | `S3StorageDriver` | 📋 计划中 |

## 🔧 配置转换

### 1. JWT 密钥转换

Go 版本和 .NET 版本的 JWT 实现略有不同,建议重新生成密钥:

```bash
# 生成新的 JWT 密钥
openssl rand -base64 32
```

### 2. 数据库连接字符串

```bash
# Go 版本
"db_file": "data/data.db"

# .NET 版本
"DefaultConnection": "Data Source=openlist.db"
```

### 3. 端口配置

```bash
# Go 版本
"http_port": 5244

# .NET 版本
"Server": {
  "Port": 5244
}
```

## ⚠️ 注意事项

### 1. 认证令牌

由于 JWT 实现的差异,现有的认证令牌在迁移后将失效。用户需要重新登录。

### 2. 密码哈希

如果密码哈希算法不同,用户可能需要重置密码:

```bash
# 使用管理员重置用户密码
dotnet run --project OpenList.Api -- reset-password --username admin
```

### 3. 文件路径

确保存储路径在两个版本之间保持一致:

```bash
# 检查并更新存储配置
# Go 版本: /data/storage
# .NET 版本: ./data/storage 或 /app/data/storage (Docker)
```

### 4. 数据库兼容性

虽然都使用 SQLite,但表结构可能有差异:

- Go 版本表名: `x_users`, `x_storages`
- .NET 版本表名: `Users`, `Storages`

## 🐛 故障排除

### 问题 1: 数据库连接失败

```bash
# 检查数据库文件权限
chmod 644 openlist.db

# 检查连接字符串
cat appsettings.json | grep ConnectionStrings
```

### 问题 2: JWT 验证失败

```bash
# 确认 JWT 密钥长度至少 32 字符
# 更新 appsettings.json 中的 Jwt:Secret
```

### 问题 3: 存储驱动未找到

```bash
# 检查存储配置
GET /api/storage

# 更新存储驱动名称
# Go: "Local" -> .NET: "local"
```

## 📊 迁移清单

- [ ] 备份 Go 版本数据
- [ ] 导出用户数据
- [ ] 导出存储配置
- [ ] 安装 .NET 运行时
- [ ] 配置 .NET 版本
- [ ] 导入用户数据
- [ ] 重新配置存储
- [ ] 测试基本功能
- [ ] 验证文件访问
- [ ] 更新前端配置(如有)
- [ ] 通知用户重新登录

## 🚀 迁移后优化

### 1. 性能调优

```json
{
  "Kestrel": {
    "Limits": {
      "MaxConcurrentConnections": 100,
      "MaxRequestBodySize": 52428800
    }
  }
}
```

### 2. 日志配置

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning"
      }
    }
  }
}
```

### 3. 缓存配置

```json
{
  "Cache": {
    "SlidingExpiration": "00:30:00",
    "AbsoluteExpiration": "01:00:00"
  }
}
```

## 📞 获取帮助

如果在迁移过程中遇到问题:

1. 查看 [GitHub Issues](https://github.com/OpenListTeam/OpenList/issues)
2. 加入 [Telegram 群组](https://t.me/OpenListTeam)
3. 查阅 [完整文档](https://doc.oplist.org)

## 🎉 迁移完成

迁移完成后,您应该能够:

- ✅ 使用新的 .NET API
- ✅ 访问所有原有数据
- ✅ 使用改进的性能
- ✅ 享受更好的可维护性

---

祝您迁移顺利! 🎊

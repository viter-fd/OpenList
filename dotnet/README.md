# OpenList - .NET 9/C# Edition

<div align="center">
  
  **OpenList** 的 .NET 9/C# 完整重写版本
  
  [![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
  [![C#](https://img.shields.io/badge/C%23-11-239120?logo=c-sharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
  [![License](https://img.shields.io/github/license/OpenListTeam/OpenList)](https://github.com/OpenListTeam/OpenList/blob/main/LICENSE)

</div>

---

## 📖 简介

这是 OpenList 项目使用 .NET 8.0 和 C# 11 的完整重写版本。采用清洁架构(Clean Architecture)设计模式,提供高性能、可扩展的多存储文件列表管理系统。

### ✨ 主要特性

- 🎯 **清洁架构**: 采用领域驱动设计(DDD)和清洁架构原则
- 🚀 **高性能**: 基于 ASP.NET Core 8.0,性能卓越
- 🔐 **安全认证**: JWT + 2FA 双因素认证
- 💾 **多存储支持**: 支持本地、S3、阿里云OSS等多种存储后端
- 🐳 **容器化部署**: 完整的 Docker 支持
- 📝 **API文档**: Swagger/OpenAPI 自动生成
- 🔧 **易于扩展**: 模块化设计,易于添加新功能

## 🏗️ 项目架构

```
OpenList.sln
├── OpenList.Core              # 核心领域层
│   ├── Entities/             # 实体模型
│   ├── Interfaces/           # 接口定义
│   └── Models/               # 领域模型
├── OpenList.Application       # 应用层
│   ├── DTOs/                 # 数据传输对象
│   ├── Interfaces/           # 应用服务接口
│   └── Services/             # 应用服务实现
├── OpenList.Infrastructure    # 基础设施层
│   ├── Data/                 # 数据库上下文
│   ├── Repositories/         # 仓储实现
│   └── Drivers/              # 存储驱动实现
└── OpenList.Api              # Web API层
    ├── Controllers/          # API控制器
    └── Middlewares/          # 中间件
```

### 架构设计原则

- **领域层(Core)**: 不依赖任何外部框架,纯粹的业务逻辑
- **应用层(Application)**: 应用用例和业务流程编排
- **基础设施层(Infrastructure)**: 数据访问、外部服务集成
- **表示层(Api)**: RESTful API和用户界面

## 🚀 快速开始

### 前置要求

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) 或更高版本
- [Docker](https://www.docker.com/) (可选,用于容器化部署)

### 本地运行

1. **克隆仓库**
```bash
git clone https://github.com/OpenListTeam/OpenList.git
cd OpenList/dotnet
```

2. **还原依赖**
```bash
dotnet restore
```

3. **配置数据库**

编辑 `OpenList.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=openlist.db"
  }
}
```

4. **应用数据库迁移**
```bash
cd OpenList.Api
dotnet ef database update
```

5. **运行项目**
```bash
dotnet run
```

6. **访问 API**
- API 地址: http://localhost:5244
- Swagger UI: http://localhost:5244

### Docker 部署

1. **构建镜像**
```bash
docker-compose build
```

2. **启动服务**
```bash
docker-compose up -d
```

3. **查看日志**
```bash
docker-compose logs -f
```

4. **停止服务**
```bash
docker-compose down
```

## 📚 API 文档

启动项目后,访问 Swagger UI 查看完整的 API 文档:

```
http://localhost:5244
```

### 主要 API 端点

#### 系统信息
- `GET /api/system/info` - 获取系统信息
- `GET /api/system/health` - 健康检查
- `GET /api/system/version` - 获取版本信息

#### 用户管理
- `POST /api/user` - 创建用户
- `GET /api/user` - 获取所有用户
- `GET /api/user/{id}` - 获取指定用户
- `DELETE /api/user/{id}` - 删除用户

#### 存储管理
- `POST /api/storage` - 创建存储
- `GET /api/storage` - 获取所有存储
- `GET /api/storage/{id}` - 获取指定存储
- `PUT /api/storage/{id}` - 更新存储
- `DELETE /api/storage/{id}` - 删除存储
- `PATCH /api/storage/{id}/toggle` - 启用/禁用存储

## ⚙️ 配置说明

### appsettings.json

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
  },
  "Storage": {
    "DefaultDriver": "local",
    "LocalRootPath": "./data/storage"
  }
}
```

### 环境变量

可以通过环境变量覆盖配置:

```bash
export ConnectionStrings__DefaultConnection="Data Source=/data/openlist.db"
export Jwt__Secret="your-secret-key"
export Server__Port=5244
```

## 🔧 开发指南

### 添加新的存储驱动

1. 在 `OpenList.Infrastructure/Drivers` 创建新驱动类:

```csharp
public class MyStorageDriver : BaseStorageDriver
{
    public override string Name => "mystorage";
    
    public override Task InitAsync(string configJson, CancellationToken cancellationToken = default)
    {
        // 初始化逻辑
    }
    
    // 实现其他必需方法...
}
```

2. 注册驱动到依赖注入容器

### 数据库迁移

```bash
# 添加迁移
dotnet ef migrations add MigrationName --project OpenList.Infrastructure --startup-project OpenList.Api

# 更新数据库
dotnet ef database update --project OpenList.Infrastructure --startup-project OpenList.Api

# 回滚迁移
dotnet ef database update PreviousMigrationName --project OpenList.Infrastructure --startup-project OpenList.Api
```

## 🧪 测试

```bash
# 运行所有测试
dotnet test

# 运行特定测试项目
dotnet test OpenList.Tests

# 生成代码覆盖率报告
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=opencover
```

## 📦 构建和发布

### 开发构建
```bash
dotnet build
```

### 发布版本
```bash
dotnet publish -c Release -o ./publish
```

### 创建可执行文件
```bash
dotnet publish -c Release -r linux-x64 --self-contained true
dotnet publish -c Release -r win-x64 --self-contained true
```

## 🔒 安全性

- ✅ JWT 令牌认证
- ✅ 双因素认证(2FA)支持
- ✅ 密码加密存储(SHA256 + Salt)
- ✅ CORS 配置
- ✅ HTTPS 支持
- ✅ 输入验证和清理

## 🤝 贡献指南

我们欢迎所有形式的贡献!

1. Fork 项目
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

## 📝 版本历史

### v1.0.0 - 2024-10-31
- 🎉 首个 .NET 版本发布
- ✨ 完整的清洁架构实现
- 🚀 基础 CRUD 操作
- 🔐 JWT 认证支持
- 💾 本地存储驱动
- 🐳 Docker 支持

## 📄 许可证

本项目采用 [AGPL-3.0](https://www.gnu.org/licenses/agpl-3.0.txt) 许可证。

## 🙏 致谢

- 感谢原 [OpenList](https://github.com/OpenListTeam/OpenList) 项目的所有贡献者
- 感谢 .NET 社区的支持

## 📞 联系我们

- GitHub: [@OpenListTeam](https://github.com/OpenListTeam)
- Telegram: [OpenList Group](https://t.me/OpenListTeam)

## ⭐ Star History

如果这个项目对你有帮助,请给它一个 Star ⭐️

---

<div align="center">
  Made with ❤️ by OpenList Team
</div>

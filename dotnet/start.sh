#!/bin/bash

# OpenList .NET 版本快速启动脚本

echo "==================================================";
echo "  OpenList .NET Edition - Quick Start Script";
echo "==================================================";
echo "";

# 检查 .NET 是否安装
if ! command -v dotnet &> /dev/null
then
    echo "❌ .NET SDK 未找到!";
    echo "请从以下地址安装 .NET 8.0 SDK:";
    echo "https://dotnet.microsoft.com/download/dotnet/8.0";
    exit 1
fi

echo "✅ .NET SDK 版本: $(dotnet --version)";
echo "";

# 检查是否在正确的目录
if [ ! -f "OpenList.sln" ]; then
    echo "❌ 未找到 OpenList.sln 文件!";
    echo "请确保在 dotnet 目录中运行此脚本。";
    exit 1
fi

# 还原依赖
echo "📦 正在还原 NuGet 包...";
dotnet restore
if [ $? -ne 0 ]; then
    echo "❌ NuGet 包还原失败!";
    exit 1
fi
echo "✅ NuGet 包还原成功";
echo "";

# 构建项目
echo "🔨 正在构建项目...";
dotnet build --configuration Release
if [ $? -ne 0 ]; then
    echo "❌ 项目构建失败!";
    exit 1
fi
echo "✅ 项目构建成功";
echo "";

# 应用数据库迁移
echo "🗄️  正在应用数据库迁移...";
cd OpenList.Api
dotnet ef database update --project ../OpenList.Infrastructure
if [ $? -ne 0 ]; then
    echo "⚠️  数据库迁移失败,继续启动...";
fi
echo "✅ 数据库准备就绪";
echo "";

# 创建数据目录
mkdir -p data/storage
echo "✅ 数据目录已创建";
echo "";

# 启动服务器
echo "🚀 正在启动 OpenList API 服务器...";
echo "";
dotnet run --configuration Release


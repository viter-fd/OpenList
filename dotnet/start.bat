@echo off
REM OpenList .NET 版本快速启动脚本 (Windows)

echo ==================================================
echo   OpenList .NET Edition - Quick Start Script
echo ==================================================
echo.

REM 检查 .NET 是否安装
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ❌ .NET SDK 未找到!
    echo 请从以下地址安装 .NET 8.0 SDK:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

echo ✅ .NET SDK 已安装
echo.

REM 检查是否在正确的目录
if not exist OpenList.sln (
    echo ❌ 未找到 OpenList.sln 文件!
    echo 请确保在 dotnet 目录中运行此脚本。
    pause
    exit /b 1
)

REM 还原依赖
echo 📦 正在还原 NuGet 包...
dotnet restore
if errorlevel 1 (
    echo ❌ NuGet 包还原失败!
    pause
    exit /b 1
)
echo ✅ NuGet 包还原成功
echo.

REM 构建项目
echo 🔨 正在构建项目...
dotnet build --configuration Release
if errorlevel 1 (
    echo ❌ 项目构建失败!
    pause
    exit /b 1
)
echo ✅ 项目构建成功
echo.

REM 应用数据库迁移
echo 🗄️  正在应用数据库迁移...
cd OpenList.Api
dotnet ef database update --project ..\OpenList.Infrastructure
echo ✅ 数据库准备就绪
echo.

REM 创建数据目录
if not exist data\storage mkdir data\storage
echo ✅ 数据目录已创建
echo.

REM 启动服务器
echo 🚀 正在启动 OpenList API 服务器...
echo.
dotnet run --configuration Release

pause

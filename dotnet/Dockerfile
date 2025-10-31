# 构建阶段
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制项目文件
COPY ["OpenList.Api/OpenList.Api.csproj", "OpenList.Api/"]
COPY ["OpenList.Application/OpenList.Application.csproj", "OpenList.Application/"]
COPY ["OpenList.Infrastructure/OpenList.Infrastructure.csproj", "OpenList.Infrastructure/"]
COPY ["OpenList.Core/OpenList.Core.csproj", "OpenList.Core/"]

# 还原依赖
RUN dotnet restore "OpenList.Api/OpenList.Api.csproj"

# 复制所有源代码
COPY . .

# 构建项目
WORKDIR "/src/OpenList.Api"
RUN dotnet build "OpenList.Api.csproj" -c Release -o /app/build

# 发布阶段
FROM build AS publish
RUN dotnet publish "OpenList.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 运行时阶段
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# 创建数据目录
RUN mkdir -p /app/data/storage

# 复制发布文件
COPY --from=publish /app/publish .

# 暴露端口
EXPOSE 5244

# 设置环境变量
ENV ASPNETCORE_URLS=http://+:5244
ENV ASPNETCORE_ENVIRONMENT=Production

# 入口点
ENTRYPOINT ["dotnet", "OpenList.Api.dll"]

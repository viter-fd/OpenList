#!/bin/bash

# OpenList API 测试脚本
# 测试核心功能：用户注册、登录、存储管理

API_BASE="http://localhost:5244/api"
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "=========================================="
echo "  OpenList API 功能测试"
echo "=========================================="
echo ""

# 测试系统信息
echo -e "${YELLOW}1. 测试系统信息接口${NC}"
RESPONSE=$(curl -s -w "\n%{http_code}" "$API_BASE/system/info")
HTTP_CODE=$(echo "$RESPONSE" | tail -n 1)
BODY=$(echo "$RESPONSE" | head -n -1)

if [ "$HTTP_CODE" -eq 200 ]; then
    echo -e "${GREEN}✓ 系统信息接口正常${NC}"
    echo "$BODY" | jq '.'
else
    echo -e "${RED}✗ 系统信息接口失败 (HTTP $HTTP_CODE)${NC}"
fi
echo ""

# 测试健康检查
echo -e "${YELLOW}2. 测试健康检查接口${NC}"
RESPONSE=$(curl -s -w "\n%{http_code}" "$API_BASE/system/health")
HTTP_CODE=$(echo "$RESPONSE" | tail -n 1)

if [ "$HTTP_CODE" -eq 200 ]; then
    echo -e "${GREEN}✓ 健康检查正常${NC}"
else
    echo -e "${RED}✗ 健康检查失败 (HTTP $HTTP_CODE)${NC}"
fi
echo ""

# 测试用户注册
echo -e "${YELLOW}3. 测试用户注册${NC}"
REGISTER_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$API_BASE/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "Test123456!",
    "email": "test@example.com"
  }')
HTTP_CODE=$(echo "$REGISTER_RESPONSE" | tail -n 1)
BODY=$(echo "$REGISTER_RESPONSE" | head -n -1)

if [ "$HTTP_CODE" -eq 200 ]; then
    echo -e "${GREEN}✓ 用户注册成功${NC}"
    echo "$BODY" | jq '.'
else
    echo -e "${YELLOW}⚠ 用户可能已存在或注册失败 (HTTP $HTTP_CODE)${NC}"
    echo "$BODY" | jq '.'
fi
echo ""

# 测试用户登录
echo -e "${YELLOW}4. 测试用户登录${NC}"
LOGIN_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$API_BASE/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "Test123456!"
  }')
HTTP_CODE=$(echo "$LOGIN_RESPONSE" | tail -n 1)
BODY=$(echo "$LOGIN_RESPONSE" | head -n -1)

if [ "$HTTP_CODE" -eq 200 ]; then
    echo -e "${GREEN}✓ 用户登录成功${NC}"
    TOKEN=$(echo "$BODY" | jq -r '.data.token')
    echo "Token: ${TOKEN:0:50}..."
else
    echo -e "${RED}✗ 用户登录失败 (HTTP $HTTP_CODE)${NC}"
    echo "$BODY" | jq '.'
    exit 1
fi
echo ""

# 测试创建本地存储
echo -e "${YELLOW}5. 测试创建本地存储${NC}"
mkdir -p /tmp/openlist-storage
STORAGE_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$API_BASE/storage" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "mountPath": "/local",
    "name": "本地存储",
    "driver": "local",
    "configJson": "{\"rootPath\":\"/tmp/openlist-storage\"}",
    "order": 0,
    "remark": "测试本地存储"
  }')
HTTP_CODE=$(echo "$STORAGE_RESPONSE" | tail -n 1)
BODY=$(echo "$STORAGE_RESPONSE" | head -n -1)

if [ "$HTTP_CODE" -eq 200 ]; then
    echo -e "${GREEN}✓ 存储创建成功${NC}"
    STORAGE_ID=$(echo "$BODY" | jq -r '.data.id')
    echo "存储ID: $STORAGE_ID"
    echo "$BODY" | jq '.data'
else
    echo -e "${RED}✗ 存储创建失败 (HTTP $HTTP_CODE)${NC}"
    echo "$BODY" | jq '.'
fi
echo ""

# 测试获取存储列表
echo -e "${YELLOW}6. 测试获取存储列表${NC}"
LIST_RESPONSE=$(curl -s -w "\n%{http_code}" -X GET "$API_BASE/storage" \
  -H "Authorization: Bearer $TOKEN")
HTTP_CODE=$(echo "$LIST_RESPONSE" | tail -n 1)
BODY=$(echo "$LIST_RESPONSE" | head -n -1)

if [ "$HTTP_CODE" -eq 200 ]; then
    echo -e "${GREEN}✓ 获取存储列表成功${NC}"
    echo "$BODY" | jq '.data'
else
    echo -e "${RED}✗ 获取存储列表失败 (HTTP $HTTP_CODE)${NC}"
fi
echo ""

# 测试文件操作 - 创建测试文件
echo -e "${YELLOW}7. 测试文件操作${NC}"
echo "测试内容" > /tmp/openlist-storage/test.txt

# 列出文件
FILES_RESPONSE=$(curl -s -w "\n%{http_code}" -X GET "$API_BASE/fs/list?path=/local/" \
  -H "Authorization: Bearer $TOKEN")
HTTP_CODE=$(echo "$FILES_RESPONSE" | tail -n 1)
BODY=$(echo "$FILES_RESPONSE" | head -n -1)

if [ "$HTTP_CODE" -eq 200 ]; then
    echo -e "${GREEN}✓ 文件列表获取成功${NC}"
    echo "$BODY" | jq '.data'
else
    echo -e "${RED}✗ 文件列表获取失败 (HTTP $HTTP_CODE)${NC}"
fi
echo ""

echo "=========================================="
echo -e "${GREEN}测试完成！${NC}"
echo "=========================================="

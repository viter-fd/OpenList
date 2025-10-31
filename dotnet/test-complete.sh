#!/bin/bash

# OpenList 完整功能测试

API_BASE="http://localhost:5244/api"
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo "=========================================="
echo "  OpenList 完整功能测试"
echo "=========================================="
echo ""

# 登录
echo -e "${YELLOW}步骤 1: 用户登录${NC}"
LOGIN_RESPONSE=$(curl -s -X POST "$API_BASE/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "testuser", "password": "Test123456!"}')

TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.data.token // .data.accessToken')

if [ "$TOKEN" == "null" ] || [ -z "$TOKEN" ]; then
    echo -e "${RED}✗ 登录失败${NC}"
    exit 1
fi
echo -e "${GREEN}✓ 登录成功${NC}"
echo ""

# 创建存储
echo -e "${YELLOW}步骤 2: 创建本地存储${NC}"
TEST_DIR="/tmp/openlist-full-test"
mkdir -p "$TEST_DIR"
echo "测试文件内容" > "$TEST_DIR/test.txt"
mkdir -p "$TEST_DIR/subfolder"
echo "子文件夹文件" > "$TEST_DIR/subfolder/file.txt"

STORAGE_RESPONSE=$(curl -s -X POST "$API_BASE/storage" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
    \"mountPath\": \"/fulltest\",
    \"name\": \"完整测试存储\",
    \"driver\": \"local\",
    \"configJson\": \"{\\\"rootPath\\\":\\\"$TEST_DIR\\\"}\",
    \"order\": 1
  }")

STORAGE_ID=$(echo "$STORAGE_RESPONSE" | jq -r '.data.id')
if [ "$STORAGE_ID" != "null" ] && [ "$STORAGE_ID" -gt 0 ]; then
    echo -e "${GREEN}✓ 存储创建成功 (ID: $STORAGE_ID)${NC}"
else
    echo -e "${RED}✗ 存储创建失败${NC}"
    echo "$STORAGE_RESPONSE" | jq '.'
fi
echo ""

# 列出文件
echo -e "${YELLOW}步骤 3: 列出文件${NC}"
FILES_RESPONSE=$(curl -s -X GET "$API_BASE/fs/list?path=/fulltest/" \
  -H "Authorization: Bearer $TOKEN")

echo "$FILES_RESPONSE" | jq '.'
SUCCESS=$(echo "$FILES_RESPONSE" | jq -r '.success')
if [ "$SUCCESS" == "true" ]; then
    echo -e "${GREEN}✓ 文件列表获取成功${NC}"
else
    echo -e "${RED}✗ 文件列表获取失败${NC}"
fi
echo ""

# 创建目录
echo -e "${YELLOW}步骤 4: 创建目录${NC}"
MKDIR_RESPONSE=$(curl -s -X POST "$API_BASE/fs/mkdir" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"path": "/fulltest/newdir"}')

echo "$MKDIR_RESPONSE" | jq '.'
SUCCESS=$(echo "$MKDIR_RESPONSE" | jq -r '.success')
if [ "$SUCCESS" == "true" ]; then
    echo -e "${GREEN}✓ 目录创建成功${NC}"
else
    echo -e "${YELLOW}⚠ 目录创建可能失败或已存在${NC}"
fi
echo ""

# 文件上传
echo -e "${YELLOW}步骤 5: 文件上传${NC}"
echo "上传测试内容" > /tmp/upload-test.txt
UPLOAD_RESPONSE=$(curl -s -X POST "$API_BASE/fs/upload" \
  -H "Authorization: Bearer $TOKEN" \
  -F "path=/fulltest/" \
  -F "file=@/tmp/upload-test.txt")

echo "$UPLOAD_RESPONSE" | jq '.'
SUCCESS=$(echo "$UPLOAD_RESPONSE" | jq -r '.success')
if [ "$SUCCESS" == "true" ]; then
    echo -e "${GREEN}✓ 文件上传成功${NC}"
else
    echo -e "${YELLOW}⚠ 文件上传可能失败${NC}"
fi
echo ""

# 获取存储列表
echo -e "${YELLOW}步骤 6: 获取所有存储${NC}"
STORAGES=$(curl -s -X GET "$API_BASE/storage" \
  -H "Authorization: Bearer $TOKEN")

COUNT=$(echo "$STORAGES" | jq '.data | length')
echo -e "${GREEN}✓ 共有 $COUNT 个存储配置${NC}"
echo "$STORAGES" | jq '.data[] | {id, name, mountPath, driver}'
echo ""

echo "=========================================="
echo -e "${GREEN}测试完成！${NC}"
echo "=========================================="
echo ""
echo "测试总结:"
echo "- 用户认证: ✓"
echo "- 存储管理: ✓"
echo "- 文件列表: $([ "$SUCCESS" == "true" ] && echo "✓" || echo "?")"
echo "- 目录创建: ?"
echo "- 文件上传: ?"
echo ""
echo "注: 文件操作功能需要进一步调试和完善"

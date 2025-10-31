#!/bin/bash

# 快速测试存储创建和列表功能

API_BASE="http://localhost:5244/api"
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m'

echo "1. 用户登录..."
LOGIN_RESPONSE=$(curl -s -X POST "$API_BASE/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "testuser", "password": "Test123456!"}')

TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.data.token')

if [ "$TOKEN" == "null" ] || [ -z "$TOKEN" ]; then
    echo -e "${RED}登录失败，无法获取token${NC}"
    exit 1
fi

echo -e "${GREEN}✓ 登录成功${NC}"
echo ""

echo "2. 创建本地存储..."
mkdir -p /tmp/openlist-test-storage

STORAGE_RESPONSE=$(curl -s -X POST "$API_BASE/storage" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "mountPath": "/testlocal",
    "name": "测试本地存储",
    "driver": "local",
    "configJson": "{\"rootPath\":\"/tmp/openlist-test-storage\"}",
    "order": 1
  }')

echo "$STORAGE_RESPONSE" | jq '.'
STORAGE_ID=$(echo "$STORAGE_RESPONSE" | jq -r '.data.id')
echo ""

echo "3. 获取存储列表..."
LIST_RESPONSE=$(curl -s -X GET "$API_BASE/storage" \
  -H "Authorization: Bearer $TOKEN")

echo "$LIST_RESPONSE" | jq '.'
COUNT=$(echo "$LIST_RESPONSE" | jq '.data | length')

if [ "$COUNT" -gt 0 ]; then
    echo -e "${GREEN}✓ 成功！找到 $COUNT 个存储${NC}"
else
    echo -e "${RED}✗ 存储列表为空${NC}"
fi
echo ""

echo "4. 获取指定存储..."
if [ "$STORAGE_ID" != "null" ] && [ "$STORAGE_ID" -gt 0 ]; then
    GET_RESPONSE=$(curl -s -X GET "$API_BASE/storage/$STORAGE_ID" \
      -H "Authorization: Bearer $TOKEN")
    echo "$GET_RESPONSE" | jq '.'
fi

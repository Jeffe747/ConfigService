#!/bin/bash
set -e

BASE_URL="http://localhost:5000"

echo "1. Creating App..."
APP_RES=$(curl -s -X POST "$BASE_URL/api/apps" -H "Content-Type: application/json" -d '{"name":"TestApp"}')
echo "App Response: $APP_RES"
APP_ID=$(echo $APP_RES | grep -o '"id":[0-9]*' | cut -d: -f2)
API_KEY=$(echo $APP_RES | grep -o '"apiKey":"[^"]*"' | cut -d'"' -f4)

echo "App ID: $APP_ID"
echo "API Key: $API_KEY"

echo "2. Creating Environment..."
ENV_RES=$(curl -s -X POST "$BASE_URL/api/apps/TestApp/envs" -H "Content-Type: application/json" -d '{"name":"Production"}')
echo "Env Response: $ENV_RES"

echo "3. Adding Config..."
CONF_RES=$(curl -s -X POST "$BASE_URL/api/apps/TestApp/envs/Production/config" -H "Content-Type: application/json" -d '{"key":"ConnectionStrings:Default", "value":"Server=ProdSQL;Database=TestDB;"}')
echo "Config Response: $CONF_RES"

echo "4. Testing Substitution..."
TEMPLATE='{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "Default": "REPLACE_ME"
  }
}'

SUB_RES=$(curl -s -X POST "$BASE_URL/config/TestApp/Production" \
     -H "Content-Type: application/json" \
     -H "X-App-Key: $API_KEY" \
     -d "$TEMPLATE")

echo "Substitution Result:"
echo "$SUB_RES"

if [[ "$SUB_RES" == *"Server=ProdSQL"* ]]; then
    echo "SUCCESS: Substitution worked!"
else
    echo "FAILURE: Substitution failed."
    exit 1
fi

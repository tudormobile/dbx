@echo off
REM get.cmd - Windows CMD script to GET a specific item endpoint

set "id=test"
set "itemId=58ad473a8df40df989e3abd56a9e217b48c2139e0ed6e3f7c837a0a79f32dd07"

curl -X GET "http://localhost:5284/api/v1/dbx/%id%/%itemId%" ^
  -H "Accept: application/json"

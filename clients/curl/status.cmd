@echo off
REM status.cmd - Windows CMD script to GET status endpoint

curl -X GET "http://localhost:5284/api/v1/dbx/status" ^
  -H "Accept: application/json"

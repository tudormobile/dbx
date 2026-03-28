@echo off
REM list.cmd - Windows CMD script to GET list endpoint

set "id=test"

curl -X GET "http://localhost:5284/api/v1/dbx/%id%" ^
  -H "Accept: application/json"

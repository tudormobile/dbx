@echo off
REM create-update-delete.cmd - Windows CMD script to CREATE, UPDATE, and DELETE a specific item endpoint

set "id=test"

REM Capture curl output to a variable
for /f "delims=" %%A in ('curl -X POST "http://localhost:5284/api/v1/dbx/%id%" ^
  -H "Accept: application/json" ^
  -H "Content-Type: application/json" ^
  -d "{ \"name\": \"Joe\", \"last\": \"Smith\", \"age\": 56 }"') do set "response=%%A"

REM Extract the last 64 characters between quotes at the end of the response
REM IMPORTANT! Assumes the response is in the format: {"itemId":"<64-character-id>"}
set "itemId=%response:~-66,64%"

echo.
echo Created item with itemId: %itemId%
echo.

echo.
echo Listing all items...
echo.
curl -X GET "http://localhost:5284/api/v1/dbx/%id%" ^
  -H "Accept: application/json"

echo.
echo Retrieve the created item...
echo.
curl -X GET "http://localhost:5284/api/v1/dbx/%id%/%itemId%" ^
  -H "Accept: application/json"

echo.
echo Update the item...
echo.
curl -X PUT "http://localhost:5284/api/v1/dbx/%id%/%itemId%" ^
  -H "Accept: application/json" ^
  -H "Content-Type: application/json" ^
  -d "{ \"name\": \"Jane\", \"last\": \"Doe\", \"age\": 30 }"

echo.
echo Retrieve the updated item...
echo.
curl -X GET "http://localhost:5284/api/v1/dbx/%id%/%itemId%" ^
  -H "Accept: application/json"

echo.
echo Delete the item...
echo.
curl -X DELETE "http://localhost:5284/api/v1/dbx/%id%/%itemId%" ^
  -H "Accept: application/json"

echo.
echo Listing all items after deletion...
echo.
curl -X GET "http://localhost:5284/api/v1/dbx/%id%" ^
  -H "Accept: application/json"


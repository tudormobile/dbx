# dbx - Data storage extensions
[![Build and Deploy (main)](https://github.com/tudormobile/dbx/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/tudormobile/dbx/actions/workflows/dotnet.yml)
[![Publish Docs (main)](https://github.com/tudormobile/dbx/actions/workflows/docs.yml/badge.svg?branch=main)](https://github.com/tudormobile/dbx/actions/workflows/docs.yml)
[![NuGet](https://img.shields.io/nuget/v/Tudormobile.Dbx.svg)](https://www.nuget.org/packages/Tudormobile.Dbx/)
[![License](https://img.shields.io/github/license/tudormobile/Dbx)](LICENSE.txt)

**Dbx** is a .NET library for adding lightweight data storage api to ASPNET applications and services.

## Quick Start

Install the package:

```bash
dotnet add package Tudormobile.Dbx
```

Optionally wire-up into your web application request chain:

```csharp
using Tudormobile.Dbx;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbx(options =>
{
    options.Prefix = "api/v1";  // endpoint mapping prefix
    options.DataPath = "data";  // repository location
});

// Configure the HTTP request pipeline.
var app = builder.Build();
app.UseDbx();
```

Alternatively, map your own endpoints and utilize the service directly:

```csharp
var dbxService = IDbxService.Create(new DbxOptions());
var response = await dbxService.GetStatusAsync();
return response;
```
You can also use DI to create and configure the IDbxService and then obtain an instance from the application container to service your endpoints.

## Response message
```csharp
public sealed record class DbxResponse
{
    public bool Success { get; init; }
    public object? Data { get; init; }
}
```
JSON:
```json
{
    "success" : true|false,
    "data" : { ... }
}
```
All **dbx** api calls return the response object shown above. In the case of success, an optional data object may be returned. In the case of failure, the data may contain an error object. In either case, the data object may also be (null) or missing from the JSON. For example, both `{"success" : false}` and `{"success" : true}` are valid responses. Always check for the *success* property and manage expectations when processing data.

## Dbx Endpoints
The following endpoints are available:

| Name | Method | Path | Description |
| ---- | ------- | ---- | ----------- |
| status | GET  | `dbx/status/` | Returns the status of the service |
| create | POST | `dbx/{id}/` | Creates a new item the indicated identifier |
| list   | GET  | `dbx/{id}/` | Lists items available for the indicated identifier |
| read   | GET  | `dbx/{id}/{itemId}` | Retrieves an individual item associated with an identifier |
| update | PUT  | `dbx/{id}/{itemId}` | Replaces an item |
| update | PATCH| `dbx/{id}/{itemId}` | Replaces some properties of an item |
| delete |DELETE| `dbx/{id}/{itemId}` | Deletes an item |
### Response Codes
- **200 Ok** - returned with success response message in the body; data varies
- **201 Created** - returned with successful response message; ***itemId*** is in the data field
- **404 Not Found** - returned with failure response message; used for most errors

#### Data Objects in Response Messages
The status endpoint returns a status object in the data property indicating the version of the dbx service. Create returns the newly-minted ***itemId*** for the stored. List returns an array of itemId values, read returns the stored object, and the delete endpoint as well as both update endpoints returns the **itemId** of the deleted or updated item. For create and updates, the data to replace or augment is contained as JSON in the ***body*** of the request.

### Identifiers and Item IDs
The identifier **{id}** above is a service or application identifier that serves as a namespace for item identifiers **{itemId}**, which are unique to the namespace. The item identifiers are created by Dbx library and returned in the *create* or *list* endpoints. 

### Authorization and Security
It is the host application responsibility to manage security. This is normally performed by adding an authroization layer to the request pipeline, typically involving specific request headers, such as api keys and application authorization keys. This is not managed by the **dbx** library. 

## Dbx Examples
The following examples demonstrate the service:
> [!NOTE]
> The examples below assume the **dbx** service is configured with and application defined *prefix* value of `api/v1` and using an application identifier of `id`.

1. Service Status
```sh
curl -X GET https://example.com/api/v1/dbx/status
```
Body; application/json; charset=utf-8, 45 bytes
```json
{
  "success": true,
  "data": {
    "version": "1.0.0.0"
  }
}
```
2. Create
```sh
curl -X POST https://example.com/api/v1/dbx/id \
     -H 'Content-Type: application/json' \
     -d '{"name":"John Doe","age":30}'
```
Body; application/json; charset=utf-8, xx bytes
```json
{
    "success": true,
    "data": "12345-6789-abc-def-0000000"
}
```
> [!IMPORTANT]
> Do not make any assumptions regarding the returned item idenditifer.
> It is simply a non-null, non-empty token to be used to reference the
> create item in other operations.

3. List
```sh
curl -X GET https://example.com/api/v1/dbx/id
```
Body; application/json; charset=utf-8, xx bytes
```json
{
    "success": true,
    "data": [
        "12345-6789-abc-def-0000001",
        "12345-6789-abc-def-0000002",
        "12345-6789-abc-def-0000003",
        "12345-6789-abc-def-0000004"
        ...
    ]
}
```
4. Read
```sh
curl -X GET https://example.com/api/v1/dbx/id/12345-6789-abc-def-0000000
```
Body; application/json; charset-utf-8; xx bytes
```json
{
    "success": true,
    "data": {"name":"John Doe","age":30}
}
```
5. Update (Full Update)
```sh
curl -X PUT https://example.com/api/v1/dbx/id/12345-6789-abc-def-0000000 \
     -H 'Content-Type: application/json' \
     -d '{"name":"Jane Doe","age":31}'
```
Body; application/json; charset=utf-8, xx bytes
```json
{
    "success": true,
    "data": "12345-6789-abc-def-0000000"
}
```
6. Update (Partial Update)
```sh
curl -X PATCH https://example.com/api/v1/dbx/id/12345-6789-abc-def-0000000 \
     -H 'Content-Type: application/json' \
     -d '{"age":65}'
```
Body; application/json; charset=utf-8, xx bytes
```json
{
    "success": true,
    "data": "12345-6789-abc-def-0000000"
}
```
7. Delete 
```sh
curl -X DELETE https://example.com/api/v1/dbx/id/12345-6789-abc-def-0000000
```
Body; application/json; charset=utf-8, xx bytes
```json
{
    "success": true,
    "data": "12345-6789-abc-def-0000000"
}
```
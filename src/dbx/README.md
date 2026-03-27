# Dbx

[![NuGet](https://img.shields.io/nuget/v/Tudormobile.Dbx.svg)](https://www.nuget.org/packages/Tudormobile.Dbx/)
[![License](https://img.shields.io/github/license/tudormobile/Dbx)](https://github.com/tudormobile/Dbx/blob/main/LICENSE.txt)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download)

**Dbx** is a compact REST base data store suitable for small amounts of no-SQL data, such as JSON documents. It intended as a lightweight data storage extension library for ASPNET web applications.

## Overview

Dbx enables web applications and services to:
- **Store and retrieve documents** via api endpoints
- **List available documents** via api endpoints
- **Delete documents** via api endpoints

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package Tudormobile.Dbx
```

Or via Package Manager Console:

```powershell
Install-Package Tudormobile.Dbx
```

Or add directly to your `.csproj`:

```xml
<PackageReference Include="Tudormobile.Dbx" Version="1.0.0" />
```

## Quick Start

### Configure the host application

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
app.Run();
```
### Client side access

```sh
curl -X GET http://www.example.com/dbx/status
curl -X PUT http://www.example.com/dbx/ -d '{"name": "John Doe", "role": "Developer"}'

```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/tudormobile/Dbx/blob/main/LICENSE) file for details.

## 🔗 Links

- **GitHub Repository:** https://github.com/tudormobile/Dbx
- **Issue Tracker:** https://github.com/tudormobile/Dbx/issues
- **NuGet Package:** https://www.nuget.org/packages/Tudormobile.Dbx/


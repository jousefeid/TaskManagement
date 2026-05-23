# Task Management API

A RESTful API built with ASP.NET Core .NET 9 and Clean Architecture.

Users can register, login, and manage their own Projects and Tasks securely using JWT authentication.

The project is divided into 4 layers: Domain, Application, Infrastructure, and API.

Entity Framework Core handles all database operations with SQL Server.

Passwords are hashed with BCrypt and all tokens expire after 24 hours.

Each user can only see and manage their own data — no cross-user access is possible.

Swagger UI is included for easy testing of all endpoints directly from the browser.

To run: update the connection string in appsettings.json, run `dotnet ef database update`, then press F5 in Visual Studio.

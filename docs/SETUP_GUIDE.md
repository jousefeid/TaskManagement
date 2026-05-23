# Task Management API — Complete Setup & Assessment Guide

## Solution Architecture

```
TaskManagement/
├── TaskManagement.sln
└── src/
    ├── TaskManagement.Domain/           ← Entities, Enums, BaseEntity (no dependencies)
    │   ├── Common/BaseEntity.cs
    │   ├── Entities/User.cs
    │   ├── Entities/Project.cs
    │   ├── Entities/ProjectTask.cs
    │   └── Enums/Enums.cs
    │
    ├── TaskManagement.Application/      ← Business logic, MediatR, FluentValidation
    │   ├── Common/
    │   │   ├── Behaviors/ValidationBehavior.cs
    │   │   ├── Exceptions/DomainExceptions.cs
    │   │   ├── Interfaces/IRepositories.cs
    │   │   └── Models/ApiResponse.cs, PaginatedResult.cs
    │   ├── DTOs/Auth/, Projects/, Tasks/
    │   ├── Features/
    │   │   ├── Auth/Commands/RegisterCommand.cs
    │   │   ├── Auth/Queries/LoginQuery.cs
    │   │   ├── Projects/Commands/ProjectCommands.cs
    │   │   ├── Projects/Queries/ProjectQueries.cs
    │   │   ├── Tasks/Commands/TaskCommands.cs
    │   │   └── Tasks/Queries/TaskQueries.cs
    │   └── DependencyInjection.cs
    │
    ├── TaskManagement.Infrastructure/   ← EF Core, SQL Server, JWT, BCrypt
    │   ├── Data/
    │   │   ├── AppDbContext.cs
    │   │   └── Configurations/EntityConfigurations.cs
    │   ├── Repositories/
    │   │   ├── Repository.cs            (generic base)
    │   │   └── ConcreteRepositories.cs  (User, Project, Task)
    │   ├── Services/
    │   │   ├── JwtService.cs
    │   │   ├── PasswordHasher.cs
    │   │   └── CurrentUserService.cs
    │   └── DependencyInjection.cs
    │
    └── TaskManagement.API/              ← Controllers, Middleware, Program.cs
        ├── Controllers/
        │   ├── AuthController.cs
        │   ├── ProjectsController.cs
        │   └── TasksController.cs
        ├── Extensions/ServiceExtensions.cs
        ├── Middleware/ExceptionHandlingMiddleware.cs
        ├── Program.cs
        ├── appsettings.json
        └── appsettings.Development.json
```

---

## 1. Solution Setup

### Step 1 — Install .NET 9 SDK (keep .NET 8 intact)

.NET SDKs are **side-by-side installs** — installing .NET 9 does NOT break .NET 8.

1. Go to: https://dotnet.microsoft.com/download/dotnet/9.0
2. Download **.NET 9 SDK (x64)** for Windows
3. Run the installer (no uninstall of .NET 8 needed)
4. Verify: open a new terminal and run:
   ```
   dotnet --list-sdks
   ```
   You should see both `8.x.x` and `9.x.x` listed.

### Step 2 — Make Visual Studio 2022 use .NET 9

Visual Studio 2022 **v17.8+** supports .NET 9. Check your version:
- Help → About Microsoft Visual Studio

If below 17.8, update via: Help → Check for Updates

After installing .NET 9 SDK, VS2022 automatically detects it. Projects targeting `net9.0` in their `.csproj` will use it automatically.

### Step 3 — Create the solution via CLI

You can use either the provided files directly, or recreate via CLI:

```bash
# Create solution
mkdir TaskManagement && cd TaskManagement
dotnet new sln -n TaskManagement

# Create projects
dotnet new classlib -n TaskManagement.Domain     -o src/TaskManagement.Domain     --framework net9.0
dotnet new classlib -n TaskManagement.Application -o src/TaskManagement.Application --framework net9.0
dotnet new classlib -n TaskManagement.Infrastructure -o src/TaskManagement.Infrastructure --framework net9.0
dotnet new webapi   -n TaskManagement.API         -o src/TaskManagement.API         --framework net9.0

# Add projects to solution
dotnet sln add src/TaskManagement.Domain/TaskManagement.Domain.csproj
dotnet sln add src/TaskManagement.Application/TaskManagement.Application.csproj
dotnet sln add src/TaskManagement.Infrastructure/TaskManagement.Infrastructure.csproj
dotnet sln add src/TaskManagement.API/TaskManagement.API.csproj

# Add project references
dotnet add src/TaskManagement.Application/TaskManagement.Application.csproj \
    reference src/TaskManagement.Domain/TaskManagement.Domain.csproj

dotnet add src/TaskManagement.Infrastructure/TaskManagement.Infrastructure.csproj \
    reference src/TaskManagement.Application/TaskManagement.Application.csproj

dotnet add src/TaskManagement.API/TaskManagement.API.csproj \
    reference src/TaskManagement.Application/TaskManagement.Application.csproj

dotnet add src/TaskManagement.API/TaskManagement.API.csproj \
    reference src/TaskManagement.Infrastructure/TaskManagement.Infrastructure.csproj
```

### Step 4 — Install NuGet packages

Run these from the **solution root folder**:

```bash
# ── Application layer ──────────────────────────────────────────────────────
dotnet add src/TaskManagement.Application package MediatR --version 12.4.1
dotnet add src/TaskManagement.Application package FluentValidation --version 11.11.0
dotnet add src/TaskManagement.Application package FluentValidation.DependencyInjectionExtensions --version 11.11.0
dotnet add src/TaskManagement.Application package Microsoft.Extensions.DependencyInjection.Abstractions --version 9.0.0

# ── Infrastructure layer ───────────────────────────────────────────────────
dotnet add src/TaskManagement.Infrastructure package Microsoft.EntityFrameworkCore --version 9.0.0
dotnet add src/TaskManagement.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.0
dotnet add src/TaskManagement.Infrastructure package Microsoft.EntityFrameworkCore.Tools --version 9.0.0
dotnet add src/TaskManagement.Infrastructure package BCrypt.Net-Next --version 4.0.3
dotnet add src/TaskManagement.Infrastructure package System.IdentityModel.Tokens.Jwt --version 8.3.2
dotnet add src/TaskManagement.Infrastructure package Microsoft.IdentityModel.Tokens --version 8.3.2
dotnet add src/TaskManagement.Infrastructure package Microsoft.Extensions.Configuration.Abstractions --version 9.0.0
dotnet add src/TaskManagement.Infrastructure package Microsoft.AspNetCore.Http.Abstractions --version 2.2.0

# ── API layer ──────────────────────────────────────────────────────────────
dotnet add src/TaskManagement.API package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.0.0
dotnet add src/TaskManagement.API package Swashbuckle.AspNetCore --version 7.2.0
dotnet add src/TaskManagement.API package Asp.Versioning.Http --version 8.1.0
dotnet add src/TaskManagement.API package Asp.Versioning.Mvc.ApiExplorer --version 8.1.0
```

---

## 2. Opening in Visual Studio 2022

1. File → Open → Project/Solution → select `TaskManagement.sln`
2. In **Solution Explorer**, right-click `TaskManagement.API` → **Set as Startup Project**
3. Open `src/TaskManagement.API/appsettings.json`
4. Update the connection string for your SQL Server:

   **LocalDB (default — works out of the box with VS2022):**
   ```json
   "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TaskManagementDb;Trusted_Connection=True;TrustServerCertificate=True"
   ```

   **SQL Server Express:**
   ```json
   "DefaultConnection": "Server=.\\SQLEXPRESS;Database=TaskManagementDb;Trusted_Connection=True;TrustServerCertificate=True"
   ```

   **Full SQL Server:**
   ```json
   "DefaultConnection": "Server=YOUR_SERVER;Database=TaskManagementDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True"
   ```

5. **IMPORTANT**: Change the JWT SecretKey in `appsettings.json` — use a real random string of 32+ characters:
   ```json
   "SecretKey": "MyActualSuperSecretKeyThatIsLong!2024"
   ```

---

## 3. Database Migration Steps

### Run these commands from the **solution root** folder (where `TaskManagement.sln` is):

```bash
# Step 1: Add the initial migration
# --project = where your DbContext lives
# --startup-project = the runnable project (has the connection string)
dotnet ef migrations add InitialCreate \
    --project src/TaskManagement.Infrastructure \
    --startup-project src/TaskManagement.API

# Step 2: Apply the migration to create the database
dotnet ef database update \
    --project src/TaskManagement.Infrastructure \
    --startup-project src/TaskManagement.API
```

### If you get errors:

| Error | Fix |
|-------|-----|
| `dotnet ef not found` | Run: `dotnet tool install --global dotnet-ef --version 9.0.0` |
| `No DbContext was found` | Make sure Infrastructure project references Application project |
| `Cannot open database` | Check connection string; ensure SQL Server / LocalDB is running |
| `Build failed` | Run `dotnet build` first to see compilation errors |
| LocalDB not found | Open VS2022 → Tools → SQL Server Object Explorer to start it |

### In Visual Studio (alternative):
- Tools → NuGet Package Manager → Package Manager Console
- Set "Default project" to `TaskManagement.Infrastructure`
- Run: `Add-Migration InitialCreate`
- Run: `Update-Database`

---

## 4. Running the API

Press **F5** (or Ctrl+F5 for no debug) in Visual Studio. The browser will open at `https://localhost:{port}` and redirect to Swagger UI.

Or via CLI:
```bash
cd src/TaskManagement.API
dotnet run
```

---

## 5. Testing the API with Swagger

### Step 1 — Register a user
- Expand **POST /api/v1/auth/register**
- Click "Try it out"
- Paste this body:
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "password": "Password1"
}
```
- Click **Execute**
- Copy the `token` value from the response

### Step 2 — Authorize Swagger
- Click the **Authorize 🔒** button (top right)
- Paste just the token value (not "Bearer " prefix — Swagger adds that)
- Click **Authorize**, then **Close**

### Step 3 — Create a Project
- Expand **POST /api/v1/projects**
- "Try it out" → Execute with:
```json
{
  "name": "My First Project",
  "description": "A project to test the API"
}
```
- Copy the `id` from the response

### Step 4 — Create a Task
- Expand **POST /api/v1/projects/{projectId}/tasks**
- Set `projectId` to the project `id` you copied
- Body:
```json
{
  "title": "Set up CI/CD pipeline",
  "description": "Configure GitHub Actions for automated deployment",
  "priority": 2,
  "dueDate": "2025-12-31T00:00:00Z"
}
```
Priority values: `0=Low, 1=Medium, 2=High, 3=Critical`

### Step 5 — Test ownership isolation
- Register a **second user** (different email)
- Login as the second user, get their token
- Authorize Swagger with the second user's token
- Try to GET the first user's projects → you'll get an **empty array** (not their data)
- Try to GET by project ID → you'll get **404** (not found, not 403 — prevents ID enumeration)

---

## 6. Postman Collection

Import this JSON into Postman (File → Import → Raw text):

```json
{
  "info": {
    "name": "Task Management API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "variable": [
    { "key": "baseUrl", "value": "https://localhost:7000" },
    { "key": "token", "value": "" },
    { "key": "projectId", "value": "" },
    { "key": "taskId", "value": "" }
  ],
  "item": [
    {
      "name": "Auth",
      "item": [
        {
          "name": "Register",
          "request": {
            "method": "POST",
            "url": "{{baseUrl}}/api/v1/auth/register",
            "header": [{ "key": "Content-Type", "value": "application/json" }],
            "body": {
              "mode": "raw",
              "raw": "{\"firstName\":\"John\",\"lastName\":\"Doe\",\"email\":\"john@example.com\",\"password\":\"Password1\"}"
            }
          }
        },
        {
          "name": "Login",
          "event": [{
            "listen": "test",
            "script": {
              "exec": [
                "var json = pm.response.json();",
                "pm.collectionVariables.set('token', json.data.token);"
              ]
            }
          }],
          "request": {
            "method": "POST",
            "url": "{{baseUrl}}/api/v1/auth/login",
            "header": [{ "key": "Content-Type", "value": "application/json" }],
            "body": {
              "mode": "raw",
              "raw": "{\"email\":\"john@example.com\",\"password\":\"Password1\"}"
            }
          }
        }
      ]
    },
    {
      "name": "Projects",
      "item": [
        {
          "name": "Get All Projects",
          "request": {
            "method": "GET",
            "url": "{{baseUrl}}/api/v1/projects",
            "header": [{ "key": "Authorization", "value": "Bearer {{token}}" }]
          }
        },
        {
          "name": "Create Project",
          "event": [{
            "listen": "test",
            "script": {
              "exec": [
                "var json = pm.response.json();",
                "pm.collectionVariables.set('projectId', json.data.id);"
              ]
            }
          }],
          "request": {
            "method": "POST",
            "url": "{{baseUrl}}/api/v1/projects",
            "header": [
              { "key": "Content-Type", "value": "application/json" },
              { "key": "Authorization", "value": "Bearer {{token}}" }
            ],
            "body": {
              "mode": "raw",
              "raw": "{\"name\":\"My Project\",\"description\":\"Test project\"}"
            }
          }
        },
        {
          "name": "Get Project By Id",
          "request": {
            "method": "GET",
            "url": "{{baseUrl}}/api/v1/projects/{{projectId}}",
            "header": [{ "key": "Authorization", "value": "Bearer {{token}}" }]
          }
        },
        {
          "name": "Update Project",
          "request": {
            "method": "PUT",
            "url": "{{baseUrl}}/api/v1/projects/{{projectId}}",
            "header": [
              { "key": "Content-Type", "value": "application/json" },
              { "key": "Authorization", "value": "Bearer {{token}}" }
            ],
            "body": {
              "mode": "raw",
              "raw": "{\"name\":\"Updated Project\",\"description\":\"Updated description\"}"
            }
          }
        },
        {
          "name": "Delete Project",
          "request": {
            "method": "DELETE",
            "url": "{{baseUrl}}/api/v1/projects/{{projectId}}",
            "header": [{ "key": "Authorization", "value": "Bearer {{token}}" }]
          }
        }
      ]
    },
    {
      "name": "Tasks",
      "item": [
        {
          "name": "Get All Tasks",
          "request": {
            "method": "GET",
            "url": "{{baseUrl}}/api/v1/projects/{{projectId}}/tasks",
            "header": [{ "key": "Authorization", "value": "Bearer {{token}}" }]
          }
        },
        {
          "name": "Create Task",
          "event": [{
            "listen": "test",
            "script": {
              "exec": [
                "var json = pm.response.json();",
                "pm.collectionVariables.set('taskId', json.data.id);"
              ]
            }
          }],
          "request": {
            "method": "POST",
            "url": "{{baseUrl}}/api/v1/projects/{{projectId}}/tasks",
            "header": [
              { "key": "Content-Type", "value": "application/json" },
              { "key": "Authorization", "value": "Bearer {{token}}" }
            ],
            "body": {
              "mode": "raw",
              "raw": "{\"title\":\"My Task\",\"description\":\"Task details\",\"priority\":1,\"dueDate\":\"2025-12-31T00:00:00Z\"}"
            }
          }
        },
        {
          "name": "Update Task",
          "request": {
            "method": "PUT",
            "url": "{{baseUrl}}/api/v1/projects/{{projectId}}/tasks/{{taskId}}",
            "header": [
              { "key": "Content-Type", "value": "application/json" },
              { "key": "Authorization", "value": "Bearer {{token}}" }
            ],
            "body": {
              "mode": "raw",
              "raw": "{\"title\":\"Updated Task\",\"description\":\"Updated\",\"status\":1,\"priority\":2,\"dueDate\":\"2025-12-31T00:00:00Z\"}"
            }
          }
        },
        {
          "name": "Delete Task",
          "request": {
            "method": "DELETE",
            "url": "{{baseUrl}}/api/v1/projects/{{projectId}}/tasks/{{taskId}}",
            "header": [{ "key": "Authorization", "value": "Bearer {{token}}" }]
          }
        }
      ]
    }
  ]
}
```

**Note:** Change `https://localhost:7000` to your actual port. Find it in VS → Properties → Debug → App URL, or check the console output when running.

The **Login** and **Create Project/Task** requests have automatic tests that save the token and IDs to collection variables — run them in order for a seamless flow.

---

## 7. How This Satisfies Assessment Criteria

### Clean Architecture
- **Domain** has zero dependencies — no NuGet packages, no framework references.
- **Application** depends only on Domain + abstractions. It defines interfaces (`IRepository`, `IJwtService`) which Infrastructure implements — not the other way around. This is the Dependency Inversion Principle.
- **Infrastructure** implements Application's interfaces. Swapping SQL Server for PostgreSQL means replacing only `UseSqlServer()` with `UseNpgsql()` — no changes to Domain or Application.
- **API** only wires things together (controllers + DI). It knows nothing about EF Core or BCrypt.

### SOLID
- **S** — Single Responsibility: each handler does one thing; repositories handle only data access; JwtService only handles tokens.
- **O** — Open/Closed: adding a new feature = add new Command/Query/Handler. No existing handlers are modified.
- **L** — Liskov: `Repository<T>` is fully substitutable by concrete `UserRepository`, `ProjectRepository`.
- **I** — Interface Segregation: `IUserRepository` extends `IRepository<User>` with only user-specific methods.
- **D** — Dependency Inversion: Application defines interfaces, Infrastructure implements them. Controllers depend on `IMediator`, not concrete handlers.

### Dependency Injection
- All services registered in `DependencyInjection.cs` in each layer's own extension method.
- Scoped lifetime for repositories and services (one per HTTP request).
- Constructor injection everywhere — no `new` keyword for services.

### Validation
- `FluentValidation` validators defined alongside each Command/Query.
- `ValidationBehavior<TRequest, TResponse>` MediatR pipeline runs all validators before the handler.
- Validation errors collected and thrown as `ValidationException` → mapped to HTTP 400 with field-level error messages.

### Error Handling
- `ExceptionHandlingMiddleware` catches all unhandled exceptions.
- Each custom exception maps to its appropriate HTTP status code.
- Internal errors return a generic message — never expose stack traces.
- All responses use the `ApiResponse<T>` wrapper: `{ success, message, data, errors }`.

### Scalability
- CQRS via MediatR: reads and writes are separated. Read queries can be optimized independently (e.g. add caching, read replicas) without touching command handlers.
- Soft delete: data is never lost, enabling audit trails and recovery.
- API Versioning: `/api/v1/` prefix means you can introduce `/api/v2/` without breaking existing clients.
- Paginati on model ready in `PaginatedResult<T>` for when data sets grow.
- Global query filters ensure soft-deleted records are never accidentally returned.
- BCrypt with work factor 12: secure password hashing that can be tuned upward as hardware improves.

---

## 8. Common Issues & Quick Fixes

| Issue | Solution |
|-------|----------|
| 401 on all requests after login | Make sure you're sending `Bearer {token}` in the Authorization header |
| 404 on project that exists | You're logged in as a different user than who created it |
| Migration fails: no Design package | Add `Microsoft.EntityFrameworkCore.Design` to API project |
| Swagger shows no endpoints | Check that controllers have `[ApiController]` and `[Route]` attributes |
| LocalDB connection failed | Open SQL Server Object Explorer in VS → start LocalDB instance |
| JWT token expired | Token is valid for 24h; login again to get a new one |
| Duplicate email on register | Email `john@example.com` is already in the DB; use a different one |

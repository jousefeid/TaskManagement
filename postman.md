
## 1. Running the API

Press **F5** (or Ctrl+F5 for no debug) in Visual Studio. The browser will open at `https://localhost:{port}` and redirect to Swagger UI.

Or via CLI:
```bash
cd src/TaskManagement.API
dotnet run
```

---

## 2. Testing the API with Swagger

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


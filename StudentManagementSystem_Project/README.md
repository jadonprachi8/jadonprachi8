# Student Management System — ASP.NET Core Web API

A Student Management System built with **ASP.NET Core 8 Web API**, following a clean layered
architecture (Controller → Service → Repository), secured with **JWT authentication**, with
**global exception handling**, **Serilog logging**, and **Swagger** documentation.

## Features

- CRUD APIs for students: Get all, Get by id, Add, Update, Delete
- JWT-based authentication (`/api/auth/login`) — all `Students` endpoints require a valid token
- Layered architecture: `Controllers` → `Services` → `Repositories` → `Data` (EF Core)
- Global exception handling middleware returning consistent JSON error responses
- Serilog logging to console and rolling daily log files (`Logs/log-*.txt`)
- Swagger UI with a built-in "Authorize" button for JWT tokens
- SQL Server via Entity Framework Core (code-first, auto-migrates on startup)
- Unit tests (xUnit + Moq) for the service layer
- Dockerfile + docker-compose (API + SQL Server) for containerized runs

## Project Structure

```
StudentManagementSystem/               # solution root
├── StudentManagementSystem.sln
├── StudentManagementSystem/            # Web API project
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   └── StudentsController.cs
│   ├── Services/                       # business logic layer
│   │   ├── IStudentService.cs / StudentService.cs
│   │   └── ITokenService.cs / TokenService.cs
│   ├── Repositories/                   # data access layer
│   │   ├── IStudentRepository.cs
│   │   └── StudentRepository.cs
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Models/
│   │   └── Student.cs
│   ├── DTOs/
│   │   ├── StudentDto.cs / CreateStudentDto.cs / UpdateStudentDto.cs / ApiResponse.cs
│   │   └── Auth/LoginRequestDto.cs / LoginResponseDto.cs
│   ├── Middleware/
│   │   ├── GlobalExceptionMiddleware.cs
│   │   └── ExceptionMiddlewareExtensions.cs
│   ├── Exceptions/
│   │   ├── NotFoundException.cs
│   │   └── BadRequestException.cs
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Dockerfile
│   └── docker-compose.yml
└── StudentManagementSystem.Tests/      # xUnit test project
    └── StudentServiceTests.cs
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, a full instance, or the Docker container included below)
- (Optional) Docker Desktop, if you want to run everything containerized

## Configuration

Edit `StudentManagementSystem/appsettings.json` before running:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=StudentManagementDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "THIS_IS_A_SAMPLE_SUPER_SECRET_KEY_CHANGE_ME_1234567890",
    "Issuer": "StudentManagementSystem",
    "Audience": "StudentManagementSystemClient",
    "ExpiryMinutes": "60"
  },
  "AdminCredentials": {
    "Username": "admin",
    "Password": "Admin@123"
  }
}
```

**Important:** change `Jwt:Key` and `AdminCredentials` before any real deployment. The
`Jwt:Key` must be at least 32 characters for HMAC-SHA256.

> This project uses a single configured admin account (no `Users` table) for simplicity, since
> the assignment only requires a `Student` table. Swap `AuthController`/`TokenService` for a real
> user store if you need multi-user auth later.

## Setup & Run (local, without Docker)

1. **Restore packages**
   ```bash
   cd StudentManagementSystem
   dotnet restore
   ```

2. **Update the connection string** in `appsettings.json` to point at your SQL Server instance.

3. **Create the initial migration** (only needed once, or whenever the model changes):
   ```bash
   dotnet tool install --global dotnet-ef   # if you don't already have it
   dotnet ef migrations add InitialCreate
   ```

4. **Run the API** — migrations are applied automatically on startup (`db.Database.Migrate()`
   in `Program.cs`), so you don't need a separate `dotnet ef database update` step:
   ```bash
   dotnet run
   ```

5. Open Swagger UI in your browser (URL printed in the console, typically):
   ```
   http://localhost:5080/swagger
   ```

## Setup & Run with Docker (bonus)

This spins up SQL Server **and** the API together:

```bash
cd StudentManagementSystem   # folder containing the Dockerfile
docker-compose up --build
```

- API: `http://localhost:8080/swagger`
- SQL Server: `localhost:1433` (sa / YourStrong@Passw0rd)

## Using the API

### 1. Log in to get a JWT token

```
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}
```

Response:
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOi...",
    "expiresAt": "2026-07-21T15:30:00Z",
    "username": "admin"
  }
}
```

### 2. Call the Students endpoints with the token

In Swagger, click **Authorize** and enter `Bearer <token>`. With curl:

```bash
TOKEN="eyJhbGciOi..."

# Get all students
curl -H "Authorization: Bearer $TOKEN" http://localhost:5080/api/students

# Add a student
curl -X POST http://localhost:5080/api/students \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"John Doe","email":"john@example.com","age":21,"course":"Computer Science"}'

# Update a student
curl -X PUT http://localhost:5080/api/students/1 \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"John Doe","email":"john@example.com","age":22,"course":"Data Science"}'

# Delete a student
curl -X DELETE http://localhost:5080/api/students/1 \
  -H "Authorization: Bearer $TOKEN"
```

### Endpoints summary

| Method | Endpoint              | Auth required | Description          |
|--------|------------------------|:-------------:|-----------------------|
| POST   | `/api/auth/login`      | No             | Get JWT token         |
| GET    | `/api/students`        | Yes            | Get all students      |
| GET    | `/api/students/{id}`   | Yes            | Get a student by id   |
| POST   | `/api/students`        | Yes            | Add a new student     |
| PUT    | `/api/students/{id}`   | Yes            | Update a student      |
| DELETE | `/api/students/{id}`   | Yes            | Delete a student      |

All responses follow the same envelope:
```json
{ "success": true, "message": "...", "data": { } }
```

## Error Handling

All unhandled exceptions are caught by `GlobalExceptionMiddleware` and returned as JSON with an
appropriate HTTP status code:

- `404 Not Found` — `NotFoundException` (e.g. student id doesn't exist)
- `400 Bad Request` — `BadRequestException` (e.g. duplicate email) or model validation failures
- `401 Unauthorized` — invalid/missing JWT, or bad login credentials
- `500 Internal Server Error` — anything unexpected (logged with full stack trace via Serilog,
  but the client only receives a generic message)

## Logging

Serilog writes to:
- Console (structured, for local dev)
- `Logs/log-YYYYMMDD.txt` (rolling daily file, for persistence)

Request logging (`UseSerilogRequestLogging`) logs every HTTP request with method, path, status
code, and duration. Service methods also log key business events (student created/updated/
deleted, failed logins, not-found lookups).

## Running Unit Tests (bonus)

```bash
cd StudentManagementSystem.Tests
dotnet test
```

Covers the service layer: fetching, creating (including duplicate-email rejection), updating,
and the not-found paths for get/update/delete.

## Notes on Architecture Decisions

- **Repository pattern**: `IStudentRepository`/`StudentRepository` isolate all EF Core / SQL
  concerns from business logic, making the service layer easily unit-testable with mocks.
- **Service layer**: owns validation rules that go beyond simple field validation (e.g. unique
  email), and maps entities to DTOs so the API never leaks EF Core entities directly.
- **DTOs everywhere**: request/response models are separate from the `Student` entity, keeping
  the public API contract stable even if the database schema evolves.
- **Consistent envelope** (`ApiResponse<T>`): every endpoint returns the same
  `{ success, message, data }` shape, success or failure, which simplifies client-side handling.

## Possible Next Steps (not included, out of scope for this assignment)

- Multi-user auth backed by a real `Users` table with hashed passwords and refresh tokens
- Pagination/filtering/sorting on `GET /api/students`
- A minimal React/Angular front-end consuming this API

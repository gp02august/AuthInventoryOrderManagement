# AuthInventoryOrderManagement

A microservices-based application built with ASP.NET Core and PostgreSQL, containing three independent services:

* `AuthService` — user registration, login, JWT authentication and role-based authorization
* `InventoryService` — product management, pagination and stock management
* `OrderService` — order creation, order history, cancellation and Inventory Service integration

The services communicate through REST APIs and use JWT-based authentication with `ADMIN` and `USER` roles.

---

## Architecture

```text
                         ┌──────────────────┐
                         │   AuthService     │
                         │                  │
                         │ Register / Login │
                         │ JWT / Roles      │
                         └────────┬─────────┘
                                  │
                                  │ JWT
                                  ▼
┌──────────────────┐       ┌──────────────────┐
│ InventoryService │◄──────│   OrderService   │
│                  │ REST  │                  │
│ Product CRUD     │       │ Create Orders    │
│ Stock Management │       │ My Orders        │
│ Pagination       │       │ Cancel Orders    │
└────────┬─────────┘       └────────┬─────────┘
         │                          │
         ▼                          ▼
  PostgreSQL DB              PostgreSQL DB
```

Each service has its own database and is responsible for its own data.

---

## Technologies

* ASP.NET Core
* .NET 8+
* C#
* Entity Framework Core
* PostgreSQL
* Npgsql
* JWT Bearer Authentication
* BCrypt password hashing
* REST APIs
* Swagger / OpenAPI
* Serilog
* Role-Based Authorization
* Global Exception Handling
* `HttpClient` for service-to-service communication

---

## Prerequisites

Before running the application, make sure the following are installed:

* .NET 8 SDK or later
* Visual Studio 2026 or another compatible IDE
* PostgreSQL
* Git

Optional:

* `dotnet-ef` for EF Core migrations

Install `dotnet-ef`:

```powershell
dotnet tool install --global dotnet-ef
```

---

## Project Structure

```text
AuthInventoryOrderManagement/
│
├── AuthService/
│   ├── Controllers/
│   ├── Data/
│   ├── DTO/
│   ├── Entities/
│   ├── Middleware/
│   ├── Repository/
│   ├── Services/
│   ├── Logs/
│   ├── Program.cs
│   └── appsettings.json
│
├── InventoryService/
│   ├── Controllers/
│   ├── Data/
│   ├── DTO/
│   ├── Entities/
│   ├── Middleware/
│   ├── Repository/
│   ├── Services/
│   ├── Logs/
│   ├── Program.cs
│   └── appsettings.json
│
├── OrderService/
│   ├── Controllers/
│   ├── Data/
│   ├── DTO/
│   ├── Entities/
│   ├── Middleware/
│   ├── Repository/
│   ├── Services/
│   ├── Logs/
│   ├── Program.cs
│   └── appsettings.json
│
└── README.md
```

---

# Services

## 1. AuthService

`AuthService` is responsible for user authentication and JWT token generation.

### Features

* User registration
* BCrypt password hashing
* User login
* JWT token generation
* JWT validation
* `ADMIN` and `USER` roles
* Active/inactive user validation
* Current user information
* Global exception handling
* Application logging

### APIs

```text
POST /api/auth/register
POST /api/auth/login
GET  /api/auth/me
```

### Registration

Newly registered users are assigned the `USER` role by default.

Passwords are stored using BCrypt hashing rather than plain text.

### JWT

JWT tokens contain the authenticated user's identity and role.

The other services validate the JWT using the same:

```text
JWT Key
Issuer
Audience
```

configuration.

---

# 2. InventoryService

`InventoryService` manages products and their stock.

### Features

* Product creation
* Product retrieval
* Product retrieval by ID
* Product pagination
* Product update
* Soft delete/deactivation
* Stock reduction
* Stock restoration
* Admin-only product management
* Role-based authorization
* Global exception handling
* Application logging

### APIs

```text
POST   /api/products
GET    /api/products
GET    /api/products/{id}
PUT    /api/products/{id}
DELETE /api/products/{id}

POST   /api/products/{id}/reduce_stock
```

### Authorization

Product management operations are restricted to `ADMIN`.

Regular `USER` accounts cannot create, update, delete or directly reduce product stock.

### Pagination

The product listing API supports pagination using page number and page size.

Example:

```text
GET /api/products?pageNumber=1&pageSize=10
```

The response contains:

* Items
* PageNumber
* PageSize
* TotalCount
* TotalPages

---

# 3. OrderService

`OrderService` handles order processing and communicates with `InventoryService`.

### Features

* Create order
* Multiple products per order
* Duplicate product aggregation
* Product validation
* Stock validation
* Stock deduction through InventoryService
* Order confirmation
* User-specific order history
* Admin access to orders
* Order ownership validation
* Order cancellation
* Stock restoration during cancellation
* Compensation when order processing fails
* Global exception handling
* Application logging

### APIs

```text
POST  /api/orders
GET   /api/orders/my-orders
GET   /api/orders/{id}
PATCH /api/orders/{id}/cancel
```

### Order Processing Flow

```text
User creates order
        ↓
Validate JWT
        ↓
Validate order items
        ↓
Get products from InventoryService
        ↓
Validate product status
        ↓
Validate available stock
        ↓
Reduce stock
        ↓
Create order
        ↓
Create order items
        ↓
Set order status to CONFIRMED
```

If stock reduction or order creation fails after stock has already been reduced, the service attempts to restore the previously reduced stock.

---

## Order Authorization

### USER

A `USER` can:

* Create orders
* View their own orders
* View their own order by ID
* Cancel their own confirmed orders

A user cannot access another user's order.

### ADMIN

An `ADMIN` can:

* Access orders belonging to other users
* Manage products through InventoryService

---

# Authentication Flow

The normal request flow is:

```text
1. Register user
       ↓
2. Login
       ↓
3. Receive JWT token
       ↓
4. Send token in Authorization header
       ↓
5. Service validates JWT
       ↓
6. Role-based authorization is applied
       ↓
7. Request is processed
```

Use the following HTTP header for protected APIs:

```text
Authorization: Bearer <JWT_TOKEN>
```

---

# Databases

Each microservice uses its own PostgreSQL database.

```text
AuthService
    ↓
auth_db

InventoryService
    ↓
inventory database

OrderService
    ↓
order database
```

The services do not directly access another service's database.

For example:

```text
OrderService
     │
     │ REST API
     ▼
InventoryService
     │
     ▼
Inventory Database
```

OrderService does not directly query the InventoryService database.

---

# Configuration

Each service contains its own `appsettings.json`.

Configure the PostgreSQL connection string and JWT settings in the respective service configuration.

Example structure:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=your_database;Username=your_user;Password=your_password"
  },
  "Jwt": {
    "Key": "your-jwt-secret-key",
    "Issuer": "AuthService",
    "Audience": "Microservices"
  }
}
```

For `OrderService`, configure its own order database connection string and the InventoryService base URL.

Do not commit real database passwords or JWT signing keys to source control.

---

# Logging

The application uses `ILogger` with Serilog for application logging.

Logging is configured for all three services.

```text
AuthService
    ↓
Logs/authservice-*.log

InventoryService
    ↓
Logs/inventoryservice-*.log

OrderService
    ↓
Logs/orderservice-*.log
```

### Application logs include events such as:

**AuthService**

* Successful registration
* Successful login
* Authentication-related exceptions

**InventoryService**

* Product creation
* Product update
* Product deactivation
* Stock reduction
* Stock restoration
* Exceptions

**OrderService**

* Successful order creation
* Order cancellation
* Stock compensation failures
* Exceptions

Sensitive information such as passwords and JWT access tokens is not written to application logs.

---

# Global Exception Handling

Each service contains a `GlobalExceptionMiddleware`.

The middleware catches unhandled exceptions and converts them into appropriate HTTP responses.

Examples:

```text
ArgumentException
        ↓
400 Bad Request

InvalidOperationException
        ↓
400 Bad Request

KeyNotFoundException
        ↓
404 Not Found

UnauthorizedAccessException
        ↓
401 Unauthorized

Other unexpected exceptions
        ↓
500 Internal Server Error
```

Exceptions are also logged using the application's logging infrastructure.

---

# Swagger

Each service exposes Swagger/OpenAPI documentation in the development environment.

Swagger supports JWT authorization through the `Bearer` security scheme.

To test protected APIs:

1. Login through `AuthService`.
2. Copy the returned access token.
3. Open Swagger for the required service.
4. Click **Authorize**.
5. Enter:

```text
Bearer <your-token>
```

6. Execute the protected API.

---

# Running the Application

Start PostgreSQL first.

Then run all three services:

```text
AuthService
InventoryService
OrderService
```

The services must be running simultaneously because `OrderService` communicates with `InventoryService`.

---

# Basic Testing Flow

## Step 1 — Register

```text
POST /api/auth/register
```

Create a normal user.

---

## Step 2 — Login

```text
POST /api/auth/login
```

Copy the JWT access token from the response.

---

## Step 3 — Authenticate

Use the JWT token with:

```text
GET /api/auth/me
```

---

## Step 4 — Create/Manage Products

Login with an `ADMIN` account and use:

```text
POST /api/products
GET  /api/products
PUT  /api/products/{id}
DELETE /api/products/{id}
```

---

## Step 5 — Create Order

Login with a `USER` account and use:

```text
POST /api/orders
```

The OrderService validates the product and stock through InventoryService.

---

## Step 6 — View Orders

Use:

```text
GET /api/orders/my-orders
```

A normal user receives only their own orders.

An admin can retrieve an order by ID:

```text
GET /api/orders/{id}
```

---

## Step 7 — Cancel Order

The user can cancel their own confirmed order:

```text
PATCH /api/orders/{id}/cancel
```

When cancellation succeeds, the ordered stock is restored.

---

# Role Summary

| Operation                   | USER |                   ADMIN |
| --------------------------- | ---: | ----------------------: |
| Register                    |  Yes |                     Yes |
| Login                       |  Yes |                     Yes |
| View own profile            |  Yes |                     Yes |
| Create product              |   No |                     Yes |
| Update product              |   No |                     Yes |
| Delete product              |   No |                     Yes |
| Direct stock reduction      |   No |                     Yes |
| Create order                |  Yes | Authentication required |
| View own orders             |  Yes |                     Yes |
| Access another user's order |   No |                     Yes |
| Cancel own order            |  Yes |                     Yes |
| Cancel another user's order |   No |                     Yes |

---

# Security Notes

* Passwords are hashed using BCrypt.
* APIs use JWT Bearer authentication.
* Role-based authorization is applied to protected operations.
* JWT tokens are validated for issuer, audience, signing key and lifetime.
* Passwords and access tokens should never be stored in logs.
* Production secrets should be stored using secure secret-management mechanisms rather than committed to source control.

---

# Current Implementation Status

The current implementation includes:

* JWT authentication
* User registration and login
* `ADMIN` / `USER` roles
* JWT-protected APIs
* Product CRUD
* Product pagination
* Stock reduction/restoration
* Admin-only inventory operations
* Order creation
* Multi-product orders
* Order history
* Order ownership validation
* Admin order access
* Order cancellation
* Stock restoration on cancellation
* Global exception handling
* Structured application logging
* File logging configuration
* Swagger/OpenAPI
* OrderService → InventoryService REST communication

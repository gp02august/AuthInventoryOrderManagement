# AuthInventoryOrderManagement

Microservices sample containing three ASP.NET Core (.NET 9) services:
- `AuthService` — authentication, JWT issuance
- `InventoryService` — product inventory
- `OrderService` — order creation (calls `InventoryService`)

This README explains how to clone, configure, and run the solution locally.

---

## Prerequisites

- .NET 9 SDK installed (required to build and run projects)
- Visual Studio 2026 (optional) or any editor/IDE that supports .NET 9
- PostgreSQL running locally (default configuration below)
- (Optional) `dotnet-ef` tool to run EF Core migrations:
  - Install: `dotnet tool install --global dotnet-ef`

Repository URL:

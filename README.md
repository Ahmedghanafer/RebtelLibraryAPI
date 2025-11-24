# Rebtel Library API

A modern C# .NET 8 gRPC library management system built with clean architecture principles, CQRS pattern, and Entity Framework Core.

## 🏗️ Architecture

This project follows **Clean Architecture** principles with strict layer separation:

```
┌─────────────────┐
│     Tests       │
├─────────────────┤
│      API        │ ← gRPC Services
├─────────────────┤
│ Infrastructure  │ ← EF Core, Repositories
├─────────────────┤
│   Application   │ ← CQRS Handlers, MediatR
├─────────────────┤
│     Domain      │ ← Entities, Business Rules
└─────────────────┘
```

### 📁 Project Structure

```
RebtelLibraryAPI.sln
├── src/
│   ├── RebtelLibraryAPI.Domain/          # Domain entities and business rules
│   ├── RebtelLibraryAPI.Application/     # CQRS handlers and application logic
│   ├── RebtelLibraryAPI.Infrastructure/  # EF Core, repositories, external services
│   └── RebtelLibraryAPI.API/            # gRPC services and API layer
├── tests/
│   ├── RebtelLibraryAPI.UnitTests/      # Domain logic tests
│   ├── RebtelLibraryAPI.IntegrationTests/ # Database operation tests
│   ├── RebtelLibraryAPI.FunctionalTests/  # gRPC endpoint tests
│   └── RebtelLibraryAPI.SystemTests/    # Complete workflow tests
├── docker-compose.yml                   # Containerization setup
├── Dockerfile
└── README.md
```

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Docker Desktop (for containerized development)
- SQL Server (local or via Docker)

### Quick Start

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd RebtelLibraryAPI
   ```

2. **Run with Docker (Recommended)**
   ```bash
   docker-compose up --build
   ```

   This will start:
   - SQL Server database on port 1433
   - gRPC API on port 5230 (HTTP) and 7097 (HTTPS)

3. **Run locally (Development)**
   ```bash
   # Restore packages
   dotnet restore

   # Build the solution
   dotnet build

   # Run the API
   dotnet run --project src/RebtelLibraryAPI.API
   ```

### Database Setup

The application automatically configures the database on first startup using EF Core migrations.

**Connection String:**
```
Server=localhost,1433;Database=RebtelLibrary;User Id=sa;Password=Rebtel@Library123;TrustServerCertificate=true;
```
or Database=RebtelLibrary;Trusted_Connection=true; for local development on SSMS.
## 🧪 Testing

Nothing special just run them using any IDE

# Database Migrations

This directory contains Entity Framework Core database migrations for the Rebtel Library API.

## Migration Structure

- Each migration is generated with a timestamp prefix to ensure chronological ordering
- Migrations contain both `Up` and `Down` methods for forward and rollback operations
- Design-time model information is stored in `ApplicationDbContextModelSnapshot.cs`

## Creating New Migrations

When the domain model is updated, create new migrations using:

```bash
# From the Infrastructure project directory
dotnet ef migrations add <MigrationName> --startup-project ../RebtelLibraryAPI.API --project .

# Example:
dotnet ef migrations add AddBookEntity --startup-project ../RebtelLibraryAPI.API --project .
```

## Applying Migrations

To apply migrations to the database:

```bash
# From the API project directory
dotnet ef database update --startup-project . --project ../RebtelLibraryAPI.Infrastructure
```

## Database Context Configuration

The `ApplicationDbContext` is configured in `Program.cs` using connection strings from app settings:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
```

## Future Migrations

As domain entities are added (Book, Borrower, Loan), corresponding migrations will be created here to:

1. Create tables with proper schema
2. Add constraints and indexes
3. Seed initial data
4. Handle schema evolution over time
# Seed Data

This directory contains database seed data for development and testing purposes.

## Seed Data Structure

Seed data will be organized by entity type:

- `Books/` - Sample books for the library catalog
- `Borrowers/` - Sample borrower accounts
- `Loans/` - Sample loan records

## Adding Seed Data

When entities are implemented, add seed data using:

```csharp
modelBuilder.Entity<Book>().HasData(
    new Book { Id = 1, Title = "Clean Architecture", Author = "Robert C. Martin", ISBN = "978-0134494166" },
    // ... more books
);
```

## Environment-Specific Seeding

Seed data can be applied conditionally based on environment:

```csharp
if (builder.Environment.IsDevelopment())
{
    // Apply development seed data
}
```
# e ASP.NET Core Tutorial with Entity Framework Core and Dapper 

## 1. Install Packages

### If Using Only **Entity Framework Core**:

To use **EF Core** with MySQL, you need the following packages:

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Pomelo.EntityFrameworkCore.MySql
```

### If Using **EF Core** and **Dapper** Together:

To use both **EF Core** and **Dapper**, install these packages:

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Pomelo.EntityFrameworkCore.MySql
dotnet add package Dapper
dotnet add package MySqlConnector
```

---

## 2. When to Use Entity Framework Core vs. Dapper

| Feature                   | Entity Framework Core (EF Core)                       | Dapper                            |
| ------------------------- | ----------------------------------------------------- | --------------------------------- |
| **Abstraction**           | High-level abstraction over database, works with LINQ | Low-level abstraction, direct SQL |
| **Control over SQL**      | Less control, generates SQL from LINQ                 | Full control, you write raw SQL   |
| **Performance**           | Slower, especially with complex queries               | Much faster for simple queries    |
| **Migrations**            | Automatic schema migrations                           | No migration support              |
| **Complex relationships** | Great for managing relationships (e.g., foreign keys) | Requires manual handling          |
| **Ease of use**           | Easier for CRUD operations, built-in model validation | More manual coding needed         |

---

## 3. Different SQL Connection Strings

### For MySQL (Using Entity Framework Core):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=pethospital;User Id=root;Password=yourpassword;"
}
```

### For SQL Server (Using Entity Framework Core):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\SQLEXPRESS;Database=FirstToDo;Trusted_Connection=True;"
}
```

### Dapper Usage Example:

With **Dapper**, you can define SQL queries directly in your code:

```csharp
var query = "SELECT * FROM books";
var books = await connection.QueryAsync<Book>(query);
```

---

## 4. Interface for Entity Framework Core

When using EF Core, you typically prepare your context (database connection manager) like this:

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books { get; set; }
    public DbSet<Pet> Pets { get; set; }
}
```

This `AppDbContext` class is your interface with the database for EF Core. It manages all the interactions with the database and provides access to your tables.

---

## 5. Setting Up Your Program with Both EF Core and Dapper

Here's an example of how to configure your `Program.cs` to use both **EF Core** and **Dapper**:

```csharp
using Microsoft.EntityFrameworkCore;
using Dapper;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// Set up Entity Framework Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), new MySqlServerVersion(new Version(8, 0, 23))));

// Set up Dapper with MySQL
builder.Services.AddScoped<MySqlConnection>(sp =>
    new MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();

// Example route using EF Core to get all pets
app.MapGet("/getallpets", async (AppDbContext db) =>
{
    var pets = await db.Pets.ToListAsync();
    return Results.Ok(pets);
});

// Example route using Dapper to get all books
app.MapGet("/getallbooks", async (MySqlConnection connection) =>
{
    var books = await connection.QueryAsync<Book>("SELECT * FROM books");
    return Results.Ok(books);
});

app.Run();
```

---

This simple tutorial shows how to use **Entity Framework Core** and **Dapper** together, covering when to use each, different connection strings, and the basic setup in your `Program.cs` file.

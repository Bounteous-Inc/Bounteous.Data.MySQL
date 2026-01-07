# Bounteous.Data.MySQL

A specialized Entity Framework Core data access library for MySQL databases in .NET 8+ applications. This library extends the base `Bounteous.Data` functionality with MySQL-specific configurations, optimizations, and database provider settings.

## 📦 Installation

Install the package via NuGet:

```bash
dotnet add package Bounteous.Data.MySQL
```

Or via Package Manager Console:

```powershell
Install-Package Bounteous.Data.MySQL
```

## 🚀 Quick Start

### 1. Configure Services

```csharp
using Bounteous.Data.MySQL;
using Microsoft.Extensions.DependencyInjection;

public void ConfigureServices(IServiceCollection services)
{
    // Register the module
    services.AddModule<ModuleStartup>();
    
    // Register your connection string provider
    services.AddSingleton<IConnectionStringProvider, MyConnectionStringProvider>();
    
    // Register your MySQL DbContext factory
    services.AddScoped<IDbContextFactory<MyDbContext>, MyDbContextFactory>();
}
```

### 2. Create Your MySQL DbContext Factory

```csharp
using Bounteous.Data.MySQL;
using Microsoft.EntityFrameworkCore;

public class MyDbContextFactory : MySqlDbContextFactory<MyDbContext>
{
    public MyDbContextFactory(IConnectionBuilder connectionBuilder, IDbContextObserver observer)
        : base(connectionBuilder, observer)
    {
    }

    protected override MyDbContext Create(DbContextOptions<DbContextBase> options, IDbContextObserver observer)
    {
        return new MyDbContext(options, observer);
    }
}
```

### 3. Configure Connection String Provider

```csharp
using Bounteous.Data;
using Microsoft.Extensions.Configuration;

public class MyConnectionStringProvider : IConnectionStringProvider
{
    private readonly IConfiguration _configuration;

    public MyConnectionStringProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string ConnectionString => _configuration.GetConnectionString("MySQLConnection") 
        ?? throw new InvalidOperationException("MySQL connection string not found");
}
```

### 4. Use Your MySQL Context

```csharp
public class CustomerService
{
    private readonly IDbContextFactory<MyDbContext> _contextFactory;

    public CustomerService(IDbContextFactory<MyDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Customer> CreateCustomerAsync(string name, string email, Guid userId)
    {
        using var context = _contextFactory.Create().WithUserId(userId);
        
        var customer = new Customer 
        { 
            Name = name, 
            Email = email 
        };
        
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        
        return customer;
    }
}
```

## 🏗️ Architecture Overview

Bounteous.Data.MySQL builds upon the foundation of `Bounteous.Data` and provides MySQL-specific enhancements:

- **MySQL Provider Integration**: Uses `MySql.EntityFrameworkCore` for optimal MySQL performance
- **Connection Resilience**: Built-in retry policies for MySQL connection failures
- **MySQL-Specific Optimizations**: Configured for MySQL's unique characteristics
- **Naming Conventions**: Supports MySQL naming conventions and best practices
- **Audit Trail Support**: Inherits automatic auditing from base `Bounteous.Data`
- **Soft Delete Support**: Logical deletion capabilities optimized for MySQL

## 🔧 Key Features

### MySQL-Specific DbContext Factory

The `MySqlDbContextFactory<T>` class provides MySQL-optimized configuration:

```csharp
public abstract class MySqlDbContextFactory<T> : DbContextFactory<T> where T : IDbContext
{
    protected override DbContextOptions<DbContextBase> ApplyOptions(bool sensitiveDataLoggingEnabled = false)
    {
        return new DbContextOptionsBuilder<DbContextBase>()
            .UseMySQL(ConnectionBuilder.AdminConnectionString, mySqlOptions => 
            { 
                mySqlOptions.EnableRetryOnFailure(); 
            })
            .EnableSensitiveDataLogging(sensitiveDataLoggingEnabled)
            .EnableDetailedErrors()
            .Options;
    }
}
```

**Features:**
- **Retry on Failure**: Automatic retry for transient MySQL connection issues
- **Sensitive Data Logging**: Configurable logging for debugging (disabled in production)
- **Detailed Errors**: Enhanced error reporting for development
- **MySQL Provider**: Uses official MySQL Entity Framework provider

### Connection Management

MySQL-specific connection handling with built-in resilience:

```csharp
// Connection string format for MySQL
"Server=localhost;Database=MyDatabase;Uid=username;Pwd=password;"

// With additional MySQL-specific options
"Server=localhost;Database=MyDatabase;Uid=username;Pwd=password;CharSet=utf8mb4;SslMode=Required;"
```

### MySQL Naming Conventions

The library includes support for MySQL naming conventions:

```csharp
// Configure naming conventions in your DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // Apply MySQL naming conventions
    modelBuilder.UseSnakeCaseNamingConvention();
}
```

## 📚 Usage Examples

### Basic CRUD Operations with MySQL

```csharp
public class ProductService
{
    private readonly IDbContextFactory<MyDbContext> _contextFactory;

    public ProductService(IDbContextFactory<MyDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Product> CreateProductAsync(string name, decimal price, Guid userId)
    {
        using var context = _contextFactory.Create().WithUserId(userId);
        
        var product = new Product 
        { 
            Name = name, 
            Price = price,
            CreatedOn = DateTime.UtcNow
        };
        
        context.Products.Add(product);
        await context.SaveChangesAsync();
        
        return product;
    }

    public async Task<List<Product>> GetProductsAsync(int page = 1, int size = 50)
    {
        using var context = _contextFactory.Create();
        
        return await context.Products
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedOn)
            .ToPaginatedListAsync(page, size);
    }

    public async Task<Product> UpdateProductAsync(Guid productId, string name, decimal price, Guid userId)
    {
        using var context = _contextFactory.Create().WithUserId(userId);
        
        var product = await context.Products.FindById(productId);
        product.Name = name;
        product.Price = price;
        
        await context.SaveChangesAsync();
        return product;
    }
}
```

### MySQL-Specific Query Operations

```csharp
public class OrderService
{
    private readonly IDbContextFactory<MyDbContext> _contextFactory;

    public OrderService(IDbContextFactory<MyDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var context = _contextFactory.Create();
        
        return await context.Orders
            .Where(o => o.CreatedOn >= startDate && o.CreatedOn <= endDate)
            .Where(o => !o.IsDeleted)
            .Include(o => o.Customer)
            .OrderByDescending(o => o.CreatedOn)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalSalesAsync(DateTime startDate, DateTime endDate)
    {
        using var context = _contextFactory.Create();
        
        return await context.Orders
            .Where(o => o.CreatedOn >= startDate && o.CreatedOn <= endDate)
            .Where(o => !o.IsDeleted)
            .SumAsync(o => o.TotalAmount);
    }
}
```

### Soft Delete Operations

```csharp
public async Task DeleteProductAsync(Guid productId, Guid userId)
{
    using var context = _contextFactory.Create().WithUserId(userId);
    
    var product = await context.Products.FindById(productId);
    
    // Soft delete - sets IsDeleted = true
    product.IsDeleted = true;
    
    await context.SaveChangesAsync();
}
```

## 🔧 Configuration Options

### MySQL Connection String Options

```csharp
// Basic connection string
"Server=localhost;Database=MyDatabase;Uid=username;Pwd=password;"

// With additional MySQL options
"Server=localhost;Database=MyDatabase;Uid=username;Pwd=password;" +
"CharSet=utf8mb4;" +
"SslMode=Required;" +
"ConnectionTimeout=30;" +
"DefaultCommandTimeout=30;"
```

### MySQL-Specific DbContext Configuration

```csharp
public class MyDbContext : DbContextBase
{
    public MyDbContext(DbContextOptions<DbContextBase> options, IDbContextObserver observer)
        : base(options, observer)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure MySQL-specific settings
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)");
            
        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasColumnType("decimal(18,2)");
    }
}
```

## 🎯 Target Framework

- **.NET 8.0** and later

## 📋 Dependencies

- **Bounteous.Data** (0.0.6) - Base data access functionality
- **Microsoft.EntityFrameworkCore** (9.0.3) - Entity Framework Core
- **MySql.EntityFrameworkCore** (9.0.0) - MySQL provider for EF Core
- **EntityFrameworkCore.NamingConventions** (8.0.0) - Naming convention support
- **Microsoft.Extensions.Configuration.Abstractions** (9.0.3) - Configuration management

## 🔗 Related Projects

- [Bounteous.Data](../Bounteous.Data/) - Base data access library
- [Bounteous.Core](../Bounteous.Core/) - Core utilities and patterns
- [Bounteous.Data.PostgreSQL](../Bounteous.Data.PostgreSQL/) - PostgreSQL-specific implementation

## 🤝 Contributing

This library is maintained by Xerris Inc. For contributions, please contact the development team.

## 📄 License

See [LICENSE](LICENSE) file for details.

---

*This library provides MySQL-specific enhancements to the Bounteous.Data framework, ensuring optimal performance and compatibility with MySQL databases in enterprise .NET applications.*
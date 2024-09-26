// Database connection manager 

using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Add your tables here
    public DbSet<Book> Books { get; set; }
    public DbSet<Pet> Pets { get; set; }


}

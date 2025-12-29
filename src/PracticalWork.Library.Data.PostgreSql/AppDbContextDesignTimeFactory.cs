using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PracticalWork.Library.Data.PostgreSql;

public class AppDbContextDesignTimeFactory    : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Username=postgres;Password=myhome1861;Database=BookArchiveDb");

        return new AppDbContext(optionsBuilder.Options);
    }
}
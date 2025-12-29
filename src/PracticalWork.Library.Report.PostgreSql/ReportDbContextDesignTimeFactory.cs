using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PracticalWork.Library.Report.PostgreSql;

public class ReportDbContextDesignTimeFactory
    : IDesignTimeDbContextFactory<ReportDbContext>
{
    public ReportDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ReportDbContext>();

        optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Username=postgres;Password=myhome1861;Database=reports");

        return new ReportDbContext(optionsBuilder.Options);
    }
}
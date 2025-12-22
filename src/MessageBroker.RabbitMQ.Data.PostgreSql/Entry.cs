using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MessageBroker.RabbitMQ.Data.PostgreSql;

public static class Entry
{
    public static IServiceCollection AddReportsPostgres(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ReportsDbContext>(opt =>
            opt.UseNpgsql(
                configuration.GetConnectionString("ReportsDb")));

        return services;
    }
}

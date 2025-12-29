using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Report.PostgreSql.Repositories;

namespace PracticalWork.Library.Report.PostgreSql;

public static class Entry
{
    private static readonly Action<DbContextOptionsBuilder> DefaultOptionsAction = (_) => { };

    /// <summary>
    /// Добавления зависимостей для работы с БД
    /// </summary>
    public static IServiceCollection AddPostgreSqlReport(this IServiceCollection serviceCollection, Action<DbContextOptionsBuilder> optionsAction)
    {
        serviceCollection.AddDbContext<ReportDbContext>(optionsAction ?? DefaultOptionsAction);

        serviceCollection.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        serviceCollection.AddScoped<IReportRepository, ReportRepository>();

        return serviceCollection;
    }
}
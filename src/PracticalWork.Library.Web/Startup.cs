using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PracticalWork.Library.Cache.Redis;
using PracticalWork.Library.Controllers;
using PracticalWork.Library.Data.Minio;
using PracticalWork.Library.Data.PostgreSql;
using PracticalWork.Library.Exceptions;
using PracticalWork.Library.Web.Configuration;
using PracticalWork.Library.MessageBroker.Rabbit;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PracticalWork.Library.Report.PostgreSql;
using PracticalWork.Library.Configuration;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Web.Services;
using PracticalWork.Library.Web.Jobs;
using Quartz;

namespace PracticalWork.Library.Web;

public class Startup
{
    private static string _basePath;
    private IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;

        _basePath = string.IsNullOrWhiteSpace(Configuration["GlobalPrefix"]) ? "" : $"/{Configuration["GlobalPrefix"].Trim('/')}";
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<DomainExceptionFilter<AppException>>();
        
        services.AddPostgreSqlStorage(cfg =>
        {
            var npgsqlDataSource = new NpgsqlDataSourceBuilder(Configuration["App:DbConnectionString"])
                .EnableDynamicJson()
                .Build();

            cfg.UseNpgsql(npgsqlDataSource);
        });
        services.AddPostgreSqlReport(cfg =>
        {
            var npgsqlDataSource = new NpgsqlDataSourceBuilder(Configuration["App:DbConnectionStringReport"])
                .EnableDynamicJson()
                .Build();

            cfg.UseNpgsql(npgsqlDataSource);
        });

        services.AddMvc(opt =>
            {
                opt.Filters.AddService<DomainExceptionFilter<AppException>>();
            })
            .AddApi()
            .AddControllersAsServices()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

        services.AddSwaggerGen(c =>
        {
            c.UseOneOfForPolymorphism();
            c.OperationFilter<SwaggerFileOperationFilter>();
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "PracticalWork.Library.Contracts.xml"));
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "PracticalWork.Library.Controllers.xml"));
        });
        
        services.AddDomain();
        services.AddCache(Configuration);
        services.AddMinioFileStorage(Configuration);
        services.AddMessageBroker(Configuration);

        services.Configure<EmailSettings>(Configuration.GetSection("App:Email"));
        services.Configure<EmailTemplateSettings>(Configuration.GetSection("App:EmailTemplates"));
        services.Configure<ArchiveSettings>(Configuration.GetSection("App:Archive"));
        services.Configure<JobSettings>(Configuration.GetSection("App:Jobs"));

        services.AddScoped<IEmailService, SmtpEmailService>();
        
        var jobSettings = Configuration.GetSection("App:Jobs").Get<JobSettings>() ?? new JobSettings();
        services.AddQuartz(q =>
        {
            AddJob<ReturnRemindersJob>(q, jobSettings, "ReturnReminders");
            AddJob<WeeklyReportJob>(q, jobSettings, "WeeklyReport");
            AddJob<ArchiveJob>(q, jobSettings, "ArchiveBooks");
        });

        services.AddQuartzHostedService(opt => opt.WaitForJobsToComplete = true);
    }

    private static void AddJob<TJob>(IServiceCollectionQuartzConfigurator q,
        JobSettings jobSettings, string jobKey)
        where TJob : IJob
    {
        var key = new JobKey(jobKey);
        if (!jobSettings.Jobs.TryGetValue(jobKey, out var cfg))
            throw new InvalidOperationException(
                $"Cron expression for job '{jobKey}' not found in configuration (App:Jobs:Jobs:{jobKey})");

        q.AddJob<TJob>(opts => opts.WithIdentity(key));
        q.AddTrigger(opts => opts
            .ForJob(key)
            .WithIdentity($"{jobKey}-trigger")
            .WithCronSchedule(cfg.CronExpression));
    }

    [UsedImplicitly]
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime,
        ILogger logger, IServiceProvider serviceProvider)
    {
        try
        {
            logger.LogInformation("Applying database migrations...");
            MigrationsRunner.ApplyMigrations(logger, serviceProvider, "Library API").GetAwaiter().GetResult();
            ReportMigrationsRunner.ApplyMigrations(logger, serviceProvider, "Report API").GetAwaiter().GetResult();
            logger.LogInformation("Database migrations applied successfully");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to apply database migrations");
            throw;
        }
        app.UsePathBase(new PathString(_basePath));

        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var exceptionHandler =
                    context.Features.Get<IExceptionHandlerFeature>();

                var exception = exceptionHandler?.Error;

                var response = new ValidationProblemDetails
                {
                    Title = "Произошла внутренняя ошибка сервера",
                    Errors =
                    {
                        { exception?.GetType().Name ?? "Error", new[] { exception?.Message } }
                    }
                };

                await context.Response.WriteAsJsonAsync(response);
            });
        });

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                var descriptions = endpoints.DescribeApiVersions();
                foreach (var description in descriptions)
                {
                    var url = $"/swagger/{description.GroupName}/swagger.json";
                    var name = description.GroupName.ToUpperInvariant();
                    options.SwaggerEndpoint(url, name);
                }
            });
            endpoints.MapControllers();
        });
    }
}

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
    }

    [UsedImplicitly]
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime,
        ILogger logger, IServiceProvider serviceProvider)
    {
        try
        {
            logger.LogInformation("Applying database migrations...");
            MigrationsRunner.ApplyMigrations(logger, serviceProvider, "Library API").GetAwaiter().GetResult();
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

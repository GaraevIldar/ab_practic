using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PracticalWork.Library.Abstractions.MessageBroker;
using PracticalWork.Library.Events;
using PracticalWork.Library.MessageBroker.Rabbit.Abstractions;
using PracticalWork.Library.MessageBroker.Rabbit.Consumer;
using PracticalWork.Library.MessageBroker.Rabbit.Publishers;
using PracticalWork.Library.MessageBroker.Rabbit.Services;
using PracticalWork.Library.MessageBroker.Rabbit.Workers;

namespace PracticalWork.Library.MessageBroker.Rabbit;

public static class Entry
{
    /// <summary>
    /// Регистрация зависимостей для брокера сообщений
    /// </summary>
    public static IServiceCollection AddMessageBroker(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var librarySection = configuration.GetSection("App:RabbitMQ:Library");
        var reportsSection = configuration.GetSection("App:RabbitMQ:Reports");

        static string QueueName(IConfiguration section, string subsection, string key = "QueueName")
            => section.GetSection(subsection)[key] ?? throw new InvalidOperationException(
                $"Конфигурация RabbitMQ: не задано {subsection}:{key} (App:RabbitMQ)");

        services
            .AddKeyedSingleton<IRabbitMQConsumer, SystemActivityConsumer<BookCreatedEvent>>(
                QueueName(librarySection, "BookCreate"))
            .AddKeyedSingleton<IRabbitMQConsumer, SystemActivityConsumer<BookArchivedEvent>>(
                QueueName(librarySection, "BookArchive"))
            .AddKeyedSingleton<IRabbitMQConsumer, SystemActivityConsumer<BookReturnedEvent>>(
                QueueName(librarySection, "BookReturn"))
            .AddKeyedSingleton<IRabbitMQConsumer, SystemActivityConsumer<BookBorrowedEvent>>(
                QueueName(librarySection, "BookBorrow"))
            .AddKeyedSingleton<IRabbitMQConsumer, SystemActivityConsumer<ReaderCreatedEvent>>(
                QueueName(librarySection, "ReaderCreate"))
            .AddKeyedSingleton<IRabbitMQConsumer, SystemActivityConsumer<ReaderClosedEvent>>(
                QueueName(librarySection, "ReaderClose"))
            .AddKeyedSingleton<IRabbitMQConsumer, ReportGenerateConsumer>(
                reportsSection["QueueName"] ?? throw new InvalidOperationException("Конфигурация RabbitMQ: не задано Reports:QueueName"));
        
        services.AddSingleton<RabbitConsumerService>();

        services.AddScoped<IRabbitPublisher, RabbitPublisher>();

        services.AddSingleton<RabbitMqChannelManager>();
        services.AddSingleton<IRabbitChannelManager>(sp =>
            sp.GetRequiredService<RabbitMqChannelManager>());
        services.AddSingleton<IInitializable>(sp =>
            sp.GetRequiredService<RabbitMqChannelManager>());

        services.AddSingleton<RabbitSetupService>();
        
        services.AddHostedService<ConsumersBackgroundService>();
        

        return services;
    }
}
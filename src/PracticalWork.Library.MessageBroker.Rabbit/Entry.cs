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

        services
            .AddKeyedSingleton<IRabbitMQConsumer, SystemActivityConsumer<BookCreatedEvent>>(
                librarySection["BookCreate:QueueName"])
            .AddKeyedSingleton<IRabbitMQConsumer, SystemActivityConsumer<BookArchivedEvent>>(
                librarySection["BookArchive:QueueName"])
            .AddKeyedSingleton<IRabbitMQConsumer, SystemActivityConsumer<BookReturnedEvent>>(
                librarySection["BookReturn:QueueName"])
            .AddKeyedSingleton<IRabbitMQConsumer, SystemActivityConsumer<BookBorrowedEvent>>(
                librarySection["BookBorrow:QueueName"])
            .AddKeyedSingleton<IRabbitMQConsumer, SystemActivityConsumer<ReaderCreatedEvent>>(
                librarySection["ReaderCreate:QueueName"])
            .AddKeyedSingleton<IRabbitMQConsumer, SystemActivityConsumer<ReaderClosedEvent>>(
                librarySection["ReaderClose:QueueName"])
            .AddKeyedSingleton<IRabbitMQConsumer, ReportGenerateConsumer>(
                reportsSection["QueueName"]);
        
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
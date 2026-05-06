#nullable enable
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PracticalWork.Library.Abstractions.MessageBroker;
using PracticalWork.Library.Configuration;
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
        var reportsSection = configuration.GetSection("App:RabbitMQ:Reports");
        
        var libraryCfg = configuration.GetSection("App:RabbitMQ:Library").Get<LibraryRabbitConfig>()
            ?? throw new InvalidOperationException("Конфигурация App:RabbitMQ:Library не задана или неверна");

        ValidateLibraryConfig(libraryCfg);
        services.Configure<LibraryRabbitConfig>(configuration.GetSection("App:RabbitMQ:Library"));

        services
            .AddKeyedSingleton<IRabbitMQConsumer, SystemActivityConsumer<BookCreatedEvent>>(
                libraryCfg.BookCreate.QueueName)
            .AddKeyedSingleton<IRabbitMQConsumer, SystemActivityConsumer<BookArchivedEvent>>(
                libraryCfg.BookArchive.QueueName)
            .AddKeyedSingleton<IRabbitMQConsumer, SystemActivityConsumer<BookReturnedEvent>>(
                libraryCfg.BookReturn.QueueName)
            .AddKeyedSingleton<IRabbitMQConsumer, SystemActivityConsumer<BookBorrowedEvent>>(
                libraryCfg.BookBorrow.QueueName)
            .AddKeyedSingleton<IRabbitMQConsumer, SystemActivityConsumer<ReaderCreatedEvent>>(
                libraryCfg.ReaderCreate.QueueName)
            .AddKeyedSingleton<IRabbitMQConsumer, SystemActivityConsumer<ReaderClosedEvent>>(
                libraryCfg.ReaderClose.QueueName)
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

    private static void ValidateLibraryConfig(LibraryRabbitConfig c)
    {
        if (string.IsNullOrEmpty(c.ExchangeName))
            throw new InvalidOperationException("App:RabbitMQ:Library:ExchangeName не задан");
        EnsureBinding(c.BookCreate, "BookCreate");
        EnsureBinding(c.BookArchive, "BookArchive");
        EnsureBinding(c.BookBorrow, "BookBorrow");
        EnsureBinding(c.BookReturn, "BookReturn");
        EnsureBinding(c.ReaderCreate, "ReaderCreate");
        EnsureBinding(c.ReaderClose, "ReaderClose");
    }

    private static void EnsureBinding(QueueBindingConfig? b, string name)
    {
        if (b == null || string.IsNullOrEmpty(b.QueueName) || string.IsNullOrEmpty(b.RoutingKey))
            throw new InvalidOperationException($"App:RabbitMQ:Library:{name}:QueueName и RoutingKey обязательны");
    }
}
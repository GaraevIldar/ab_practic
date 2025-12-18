using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PracticalWork.Library.MessageBroker.Rabbit.Options;

namespace PracticalWork.Library.MessageBroker.Rabbit;

public static class Entry
{
    /// <summary>
    /// Регистрация зависимостей RabbitMQ
    /// </summary>
    public static IServiceCollection AddRabbitMq(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.Configure<RabbitMqOptions>(configuration.GetSection("App:RabbitMq"));

        serviceCollection.AddSingleton<RabbitMqConnectionFactory>();
        serviceCollection.AddScoped<IEventPublisher, RabbitMqEventPublisher>();

        return serviceCollection;
    }
}
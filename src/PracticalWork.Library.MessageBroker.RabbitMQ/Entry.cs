using Microsoft.Extensions.DependencyInjection;

namespace PracticalWork.Library.MessageBroker.RabbitMQ;

public static class Entry
{
    public static IServiceCollection AddMinioFileStorage(this IServiceCollection serviceCollection)
    {
        return serviceCollection;
    }
}
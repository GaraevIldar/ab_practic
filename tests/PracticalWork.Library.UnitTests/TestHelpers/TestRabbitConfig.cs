using Microsoft.Extensions.Options;
using PracticalWork.Library.Configuration;

namespace PracticalWork.Library.UnitTests.TestHelpers;

internal static class TestRabbitConfig
{
    public static IOptions<LibraryRabbitConfig> Create()
    {
        return Options.Create(new LibraryRabbitConfig
        {
            ExchangeName = "library.exchange",
            BookCreate = new QueueBindingConfig { QueueName = "book.create.queue", RoutingKey = "book.create.key" },
            BookArchive = new QueueBindingConfig { QueueName = "book.archive.queue", RoutingKey = "book.archive.key" },
            BookBorrow = new QueueBindingConfig { QueueName = "book.borrow.queue", RoutingKey = "book.borrow.key" },
            BookReturn = new QueueBindingConfig { QueueName = "book.return.queue", RoutingKey = "book.return.key" },
            ReaderCreate = new QueueBindingConfig { QueueName = "reader.create.queue", RoutingKey = "reader.create.key" },
            ReaderClose = new QueueBindingConfig { QueueName = "reader.close.queue", RoutingKey = "reader.close.key" },
        });
    }
}

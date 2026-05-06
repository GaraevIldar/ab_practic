namespace PracticalWork.Library.Configuration;

/// <summary>
/// Конфигурация RabbitMQ для событий библиотеки (очереди и маршруты).
/// Должна совпадать со структурой App:RabbitMQ:Library в appsettings.
/// </summary>
public class LibraryRabbitConfig
{
    public string ExchangeName { get; set; } = default!;
    public QueueBindingConfig BookCreate { get; set; } = default!;
    public QueueBindingConfig BookArchive { get; set; } = default!;
    public QueueBindingConfig BookBorrow { get; set; } = default!;
    public QueueBindingConfig BookReturn { get; set; } = default!;
    public QueueBindingConfig ReaderCreate { get; set; } = default!;
    public QueueBindingConfig ReaderClose { get; set; } = default!;
}

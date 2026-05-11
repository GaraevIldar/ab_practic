using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PracticalWork.Library.Abstractions.MessageBroker;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Configuration;
using PracticalWork.Library.Events;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Services;

public sealed class ArchiveService : IArchiveService
{
    private readonly IBookRepository _bookRepository;
    private readonly IRabbitPublisher _publisher;
    private readonly ILogger<ArchiveService> _logger;
    private readonly LibraryRabbitConfig _rabbit;
    private readonly TimeProvider _timeProvider;
    private const string RoutingKey = "book.archived";

    public ArchiveService(IBookRepository bookRepository, IRabbitPublisher publisher,
        ILogger<ArchiveService> logger, IOptions<LibraryRabbitConfig> rabbitConfig,
        TimeProvider timeProvider)
    {
        _bookRepository = bookRepository;
        _publisher = publisher;
        _logger = logger;
        _rabbit = rabbitConfig.Value;
        _timeProvider = timeProvider;
    }

    public async Task<ArchiveResult> ArchiveOldBooks(int yearsWithoutBorrow, int maxBooksPerRun)
    {
        var started = DateTime.UtcNow;
        var result = new ArchiveResult();

        var books = await _bookRepository.GetBooksForArchive(yearsWithoutBorrow, maxBooksPerRun);
        result.TotalProcessed = books.Count;

        foreach (var book in books)
        {
            try
            {
                await _bookRepository.MoveToArchive(book.Id);

                var evt = new BookArchivedEvent(book.Id, book.Title,
                    $"Книга не выдавалась более {yearsWithoutBorrow} лет", _timeProvider.GetUtcNow().UtcDateTime);
                await _publisher.PublishAsync(_rabbit.ExchangeName, RoutingKey, evt);

                result.Archived++;
                _logger.LogInformation("Archived book {BookId} '{Title}'", book.Id, book.Title);
            }
            catch (Exception ex)
            {
                result.Skipped++;
                result.SkipReasons.Add($"{book.Id}: {ex.Message}");
                _logger.LogWarning(ex, "Failed to archive book {BookId}", book.Id);
            }
        }

        result.ExecutionTime = DateTime.UtcNow - started;
        return result;
    }
}

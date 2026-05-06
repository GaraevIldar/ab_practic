using Microsoft.Extensions.Logging;
using PracticalWork.Library.Abstractions.MessageBroker;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Events;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Services;

public sealed class ArchiveService : IArchiveService
{
    private readonly IBookRepository _bookRepository;
    private readonly IRabbitPublisher _publisher;
    private readonly ILogger<ArchiveService> _logger;
    private const string Exchange = "library.events";
    private const string RoutingKey = "book.archived";

    public ArchiveService(IBookRepository bookRepository, IRabbitPublisher publisher, ILogger<ArchiveService> logger)
    {
        _bookRepository = bookRepository;
        _publisher = publisher;
        _logger = logger;
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
                    $"Книга не выдавалась более {yearsWithoutBorrow} лет", DateTime.UtcNow);
                await _publisher.PublishAsync(Exchange, RoutingKey, evt);

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

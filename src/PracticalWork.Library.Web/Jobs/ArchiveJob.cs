using Microsoft.Extensions.Options;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Configuration;
using Quartz;

namespace PracticalWork.Library.Web.Jobs;

[DisallowConcurrentExecution]
public sealed class ArchiveJob : IJob
{
    private readonly IArchiveService _archiveService;
    private readonly ArchiveSettings _archiveSettings;
    private readonly ILogger<ArchiveJob> _logger;
    private readonly TimeProvider _timeProvider;

    public ArchiveJob(
        IArchiveService archiveService,
        IOptions<ArchiveSettings> archiveSettings,
        ILogger<ArchiveJob> logger,
        TimeProvider timeProvider)
    {
        _archiveService = archiveService;
        _archiveSettings = archiveSettings.Value;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("ArchiveJob started at {Time}", _timeProvider.GetUtcNow());

        var result = await _archiveService.ArchiveOldBooks(
            _archiveSettings.YearsWithoutBorrow,
            _archiveSettings.MaxBooksPerRun);

        _logger.LogInformation(
            "ArchiveJob finished: processed={Total}, archived={Archived}, skipped={Skipped}, elapsed={Elapsed}",
            result.TotalProcessed, result.Archived, result.Skipped, result.ExecutionTime);

        foreach (var reason in result.SkipReasons)
            _logger.LogWarning("Archive skip: {Reason}", reason);
    }
}

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Quartz;

namespace PracticalWork.Library.Controllers.Api.v1;

[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/jobs")]
public class JobsController : ControllerBase
{
    private readonly ISchedulerFactory _schedulerFactory;

    public JobsController(ISchedulerFactory schedulerFactory)
    {
        _schedulerFactory = schedulerFactory;
    }

    /// <summary>
    /// Запустить джоб вручную по имени
    /// </summary>
    /// <param name="jobName">Имя джоба: ReturnReminders, WeeklyReport, ArchiveBooks</param>
    [HttpPost("{jobName}/trigger")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> TriggerJob(string jobName)
    {
        var validJobs = new[] { "ReturnReminders", "WeeklyReport", "ArchiveBooks" };
        if (!validJobs.Contains(jobName))
            return NotFound($"Джоб '{jobName}' не найден. Доступные: {string.Join(", ", validJobs)}");

        var scheduler = await _schedulerFactory.GetScheduler();
        var key = new JobKey(jobName);

        if (!await scheduler.CheckExists(key))
            return NotFound($"Джоб '{jobName}' не зарегистрирован в планировщике");

        await scheduler.TriggerJob(key);
        return Ok($"Джоб '{jobName}' запущен");
    }
}

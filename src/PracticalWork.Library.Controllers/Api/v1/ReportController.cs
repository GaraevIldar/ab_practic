using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Contracts.v1.Pagination;
using PracticalWork.Library.Contracts.v1.Report;

namespace PracticalWork.Library.Controllers.Api.v1;


[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/reports")]
public class ReportController: Controller
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }
    
    /// <summary>
    /// Получение страницы записей о событиях системы
    /// </summary>
    /// <param name="request">объект пагинации</param>
    /// <returns>страница с записями</returns>
    [HttpPost("/activity")]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetActivityLogs(ActivityLogsPaginationRequest request)
    {
        var result = await _reportService.GetActivityLogs(
            request);
        return Ok(result);
    }
    
    /// <summary>
    /// Создать отчет csv
    /// </summary>
    /// <param name="request">отчет с фильтрами</param>
    /// <returns>created</returns>
    [HttpPost("/generate")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CreateReportCsv(ReportCreateRequest request)
    {
        await _reportService.CreateReport(
            request.PeriodFrom, 
            request.PeriodTo, 
            request.EventTypes.ToArray());
        return Created();
    }
    
    /// <summary>
    /// Получить созданные отчеты
    /// </summary>
    /// <returns>информация об отчетах</returns>
    [HttpGet("/")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetGeneratedReports()
    {
        var result = await _reportService.GetListReadyReports();
        return Ok(result);
    }
    
    /// <summary>
    /// Получение ссылки на файл отчета
    /// </summary>
    /// <param name="reportName">название файла отчета</param>
    /// <returns>url отчета</returns>
    [HttpPost("/{reportName}/download")]
    [Produces("text/plain")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetGeneratedReportUrl(string reportName)
    {
        var result = await _reportService.GetReportUrl(reportName);
        return Ok(result);
    }
}
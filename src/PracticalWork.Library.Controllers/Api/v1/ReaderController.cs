using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Contracts.v1.Abstracts;
using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Controllers.Filters;
using PracticalWork.Library.Controllers.Mappers.v1;
using PracticalWork.Library.Controllers.Validations.v1;


namespace PracticalWork.Library.Controllers.Api.v1;

/// <summary>
/// Контроллер для управления читателями библиотеки
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/readers")]
public class ReaderController : Controller
{
    private readonly IReaderService _readerService;

    public ReaderController(IReaderService readerService)
    {
        _readerService = readerService;
    }

    /// <summary>
    /// Создание нового читателя
    /// </summary>
    /// <param name="request">Данные для создания читателя</param>
    /// <returns>Идентификатор созданного читателя</returns>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CreateReaderResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    [ServiceFilter(typeof(GenericValidationFilter<CreateReaderRequest>))]
    public async Task<IActionResult> CreateReader(CreateReaderRequest request)
    {
        var result = await _readerService.CreateReader(request.ToReader());
        
        return Content(result.ToString());
    }

    /// <summary>
    /// Продление срока действия библиотечной карточки читателя
    /// </summary>
    /// <param name="id">Идентификатор читателя</param>
    /// <param name="request">Данные для продления карточки</param>
    /// <returns>Идентификатор карточки читателя</returns>
    [HttpPost("{id}/extend")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ExtendReaderResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    [ServiceFilter(typeof(GenericValidationFilter<ExtendReaderRequest>))]
    public async Task<IActionResult> ExtendReader(Guid id, ExtendReaderRequest request)
    {
        var result = await _readerService.ExtendReaderCard(id, request);
        
        return Content(result.ToString());
    }
    
    /// <summary>
    /// Закрытие библиотечной карточки читателя
    /// </summary>
    /// <param name="id">Идентификатор читателя</param>
    /// <returns>Результат операции закрытия карточки</returns>
    [HttpPost("{id}/close")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CloseReaderCardResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CloseCard(Guid id)
    {
        var result = await _readerService.CloseReaderCard(id);
        
        return Content(result);
    }
    
    /// <summary>
    /// Получение списка книг, взятых читателем
    /// </summary>
    /// <param name="id">Идентификатор читателя</param>
    /// <returns>Список книг, находящихся у читателя</returns>
    [HttpGet("{id}/books")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CloseReaderCardResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetBooksReaders(Guid id)
    {
        var result = await _readerService.GetBooksReaders(id);
        
        return Ok(result);
    }
}
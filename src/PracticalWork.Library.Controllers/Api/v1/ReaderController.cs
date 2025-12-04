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

    /// <summary> Создание читателя</summary>
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

    /// <summary> Обновление времени карточки читателя</summary>
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
    
    /// <summary> Закрытие карточки читателя</summary>
    [HttpPost("{id}/close")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CloseReaderCardResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CloseCard(Guid id)
    {
        var result = await _readerService.CloseReaderCard(id);
        
        return Content(result.ToString());
    }
}
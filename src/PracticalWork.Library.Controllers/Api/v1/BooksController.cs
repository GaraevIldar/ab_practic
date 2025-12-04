using Asp.Versioning;
using Microsoft.AspNetCore.Http;
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
[Route("api/v{version:apiVersion}/books")]
public class BooksController : Controller
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    /// <summary> Создание новой книги</summary>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(CreateBookResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    [ServiceFilter(typeof(GenericValidationFilter<CreateBookRequest>))]    
    public async Task<IActionResult> CreateBook(CreateBookRequest request)
    {
        var result = await _bookService.CreateBook(request.ToBook());

        return Content(result.ToString());
    }
    /// <summary> Обновление книги</summary>
    [HttpPut("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    [ServiceFilter(typeof(GenericValidationFilter<UpdateBookRequest>))]
    public async Task<IActionResult> UpdateBook(Guid id, UpdateBookRequest request)
    {
        await _bookService.UpdateBook(id, request.ToBook());

        return Ok();
    }
    /// <summary> Перемешение книги в архив</summary>
    [HttpPost("{id}/archive")]
    [Produces("application/json")]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> MoveToArchive(Guid id)
    {
        await _bookService.MoveToArchive(id);

        return Ok();
    }
    /// <summary>Получение всех книг</summary>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BookListResponse), 200)]
    public async Task<IActionResult> GetBooks()
    {
        var result = await _bookService.GetBooks();
        
        return Ok(result);
    }

    /// <summary>Добавление деталей книги</summary>
    [HttpPost("{id}/details")]
    [Produces("application/json")]
    [ServiceFilter(typeof(GenericValidationFilter<AddBookDetailsRequest>))]
    public async Task<IActionResult> AddDetails([FromForm] AddBookDetailsRequest request)
    {
        await _bookService.AddBookDetailsAsync(request.Id, request.Description, request.CoverImage);
        return Ok();
    }
}

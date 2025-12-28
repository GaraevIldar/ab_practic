using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Contracts.v1.Pagination;

namespace PracticalWork.Library.Controllers.Api.v1;


/// <summary>
/// Контроллер для управления библиотечными операциями
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/library")]
public class LibraryController: ControllerBase
{
    private readonly ILibraryService _libraryService;
    
    public LibraryController(ILibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    /// <summary>
    /// Выдача книги читателю
    /// </summary>
    /// <param name="request">Запрос на выдачу книги</param>
    /// <returns>Идентификатор взятия книги</returns>
    [HttpPost("borrow")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BorrowBookResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> BorrowBook(BorrowBookRequest request)
    {
        var result = await _libraryService.BorrowBook(request.IdBook, request.IdReader);
        
        return Content(result.ToString());
    }

    /// <summary>
    /// Получение списка всех доступных книг
    /// </summary>
    /// <returns>Список книг, доступных для выдачи</returns>
    /// <remarks>
    /// Возвращает только книги, которые:
    /// - Не находятся в архиве
    /// - В данный момент не выданы читателям
    /// Список включает основные сведения о книгах: название, автор, год издания.
    /// </remarks>
    [HttpGet("books")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BookListResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetBooks([FromQuery] PaginationBookRequest request)
    {
        var result = await _libraryService.GetBooksNoArchive(request.PageNumber, request.PageSize, request.Status, request.Category, request.Author);
        
        return Ok(result);
    }
    /// <summary>
    /// Возврат книги в библиотеку
    /// </summary>
    /// <param name="request">Запрос на возврат книги</param>
    /// <returns>Результат операции возврата книги</returns>
    [HttpPost("return")]
    [Produces("application/json")]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ReturnBook(ReturnBookRequest request)
    {
        var result = await _libraryService.ReturnBook(request.Id);
        
        return Content(result.ToString());
    }
    
    /// <summary>
    /// Получение детальной информации о книге
    /// </summary>
    /// <param name="idOrTitle">
    /// Идентификатор книги (GUID) или название книги.
    /// </param>
    /// <returns>Детальная информация о книге</returns>
    [HttpGet("library/books/{idOrTitle}/details")]
    public async Task<IActionResult> GetBookDetails(string idOrTitle)
    {
        var result = await _libraryService.GetBookDetailsAsync(idOrTitle);
        return Ok(result);
    }
}
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Contracts.v1.Books.Response;

namespace PracticalWork.Library.Controllers.Api.v1;

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

    /// <summary> Получение книги</summary>
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

    /// <summary> Получение всех книги</summary>
    [HttpGet("books")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BookListResponse), 200)]
    public async Task<IActionResult> GetBooks()
    {
        var result = await _libraryService.GetBooksNoArchive();
        
        return Ok(result);
    }

    /// <summary> Возврат книги</summary>
    [HttpPost("return")]
    [Produces("application/json")]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ReturnBook(ReturnBookRequest request)
    {
        var result = await _libraryService.ReturnBook(request.Id);
        
        return Content(result.ToString());
    }
    
    /// <summary> Получение деталей книги</summary>
    [HttpGet("library/books/{idOrTitle}/details")]
    public async Task<IActionResult> GetBookDetails(string idOrTitle)
    {
        var result = await _libraryService.GetBookDetailsAsync(idOrTitle);
        return Ok(result);
    }
}
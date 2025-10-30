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

    [HttpGet("books")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BookListResponse), 200)]
    public async Task<IActionResult> GetBooks()
    {
        var result = await _libraryService.GetBooksNoArchive();
        
        return Ok(result);
    }

    [HttpPost("return")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ReturnBookResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ReturnBook(ReturnBookRequest request)
    {
        var result = await _libraryService.ReturnBook(request.Id);
        
        return Content(result.ToString());
    }
    
}
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using PracticalWork.Library.Exceptions;
using PracticalWork.Library.Exceptions.Book;
using PracticalWork.Library.Exceptions.Library;
using PracticalWork.Library.Exceptions.Reader;
using PracticalWork.Library.Exceptions.Report;

namespace PracticalWork.Library.Web.Configuration;

/// <summary>
/// Фильтр предназначен для трансформации доменных исключений в Bad Request
/// </summary>
/// <typeparam name="TAppException"> Доменное исключение </typeparam>
[UsedImplicitly]
public class DomainExceptionFilter<TAppException> : IAsyncActionFilter where TAppException : Exception
{
    protected readonly ILogger Logger;

    public DomainExceptionFilter(ILogger<DomainExceptionFilter<TAppException>> logger)
    {
        Logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();
        if (HasException(resultContext))
        {
            TryHandleException(resultContext, resultContext.Exception);
        }
    }

    private static bool HasException(ActionExecutedContext context) => context.Exception != null && !context.ExceptionHandled;

    protected virtual void TryHandleException(ActionExecutedContext context, Exception exception)
    {
        ObjectResult result;

        switch (exception)
        {
            case InvalidOperationException:
                result = new BadRequestObjectResult(BuildProblemDetails(exception));
                break;
            
            case BookNotFoundException:
            case ReaderNotFoundException:
            case BookBorrowNotFoundException:
            case ReportNotFoundException:
                result = new NotFoundObjectResult(BuildProblemDetails(exception));
                break;
            
            case BookAlreadyArchivedException:
            case BookArchivedException:
            case BookBorrowedException:
            case BookNotAvailableException:
            case ReaderHasBorrowedBooksException:
                result = new ConflictObjectResult(BuildProblemDetails(exception));
                break;

            case BookServiceException:
            case ReaderServiceException:
            case LibraryServiceException:
            case DbUpdateException:
            case DbException:
                result = new ObjectResult(BuildProblemDetails(exception))
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                break;

            default:
                return;
        }

        context.Result = result;
        context.ExceptionHandled = true;
        Logger.LogError(exception, "Unhandled domain exception transformed to HTTP response.");
    }



    protected static ValidationProblemDetails BuildProblemDetails(Exception exception)
    {
        var exceptionName = exception.GetType().Name;
        var errorMessages = new[] { exception.Message };

        // Используем ValidationProblemDetails, а не базовый или свой тип, т. к. он обвешан атрибутами сериализации
        // и так мы можем гарантировать идентичный ответ и при ошибках валидации, и при доменных исключениях:
        var problemDetails = new ValidationProblemDetails
        {
            Title = "Произошла ошибка во время выполнения запроса.",
            Errors = { { exceptionName, errorMessages } }
        };

        return problemDetails;
    }
}

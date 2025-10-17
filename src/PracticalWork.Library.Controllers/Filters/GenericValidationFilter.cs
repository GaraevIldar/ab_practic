using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace PracticalWork.Library.Controllers.Filters;

public class GenericValidationFilter<T> : IAsyncActionFilter
{
    private readonly IValidator<T> _validator;

    public GenericValidationFilter(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var parameter = context.ActionArguments
            .Values
            .FirstOrDefault(arg => arg is T);

        if (parameter is T dto)
        {
            ValidationResult result = await _validator.ValidateAsync(dto);

            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                context.Result = new BadRequestObjectResult(new { Errors = errors });
                return;
            }
        }
        else
        {
            context.Result = new BadRequestObjectResult($"Модель типа {typeof(T).Name} не передана или неверного типа");
            return;
        }

        await next();
    }
}
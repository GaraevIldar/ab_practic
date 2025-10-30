using FluentValidation;
using PracticalWork.Library.Contracts.v1.Books.Request;

namespace PracticalWork.Library.Controllers.Validations.v1;

public class ExtendReaderRequestValidator : AbstractValidator<ExtendReaderRequest>
{
    public ExtendReaderRequestValidator()
    {
        RuleFor(x => x.NewExpiryDate)
            .NotEmpty()
            .WithMessage("Дата окончания обязательна")

            .GreaterThan(DateOnly.FromDateTime(DateTime.Now))
            .WithMessage("Дата окончания должна быть в будущем");
    }
}
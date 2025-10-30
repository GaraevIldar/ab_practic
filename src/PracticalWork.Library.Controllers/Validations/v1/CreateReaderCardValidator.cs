using FluentValidation;
using PracticalWork.Library.Contracts.v1.Books.Request;

namespace PracticalWork.Library.Controllers.Validations.v1;

public class CreateReaderCardValidator : AbstractValidator<CreateReaderRequest>
{
    public CreateReaderCardValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("ФИО обязательно для заполнения.")
            .MinimumLength(2)
            .WithMessage("ФИО должно содержать минимум 2 символа.")
            .MaximumLength(200)
            .WithMessage("ФИО не должно превышать 200 символов.")
            .Matches(@"^[a-zA-Zа-яА-ЯёЁ\s\-]+$")
            .WithMessage("ФИО может содержать только буквы, пробелы и дефисы.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Номер телефона обязателен для заполнения.")
            .Matches(@"^\+?[0-9\s\-\(\)]{10,15}$")
            .WithMessage("Номер телефона должен быть в допустимом формате.")
            .Must(BeAValidPhoneNumber)
            .WithMessage("Номер телефона содержит недопустимые символы.");

        RuleFor(x => x.ExpiryDate)
            .NotEmpty()
            .WithMessage("Дата окончания действия обязательна для заполнения.")
            .GreaterThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Дата окончания действия должна быть в будущем.")
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(5)))
            .WithMessage("Срок действия не может превышать 5 лет.");
    }

    private bool BeAValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        var digitsOnly = System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"[^\d+]", "");
        
        return System.Text.RegularExpressions.Regex.IsMatch(digitsOnly, @"^\+?[0-9]{10,15}$");
    }
}
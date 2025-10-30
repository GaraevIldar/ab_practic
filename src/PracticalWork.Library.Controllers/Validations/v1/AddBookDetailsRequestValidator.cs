using FluentValidation;
using Microsoft.AspNetCore.Http;
using PracticalWork.Library.Contracts.v1.Books.Request;

namespace PracticalWork.Library.Controllers.Validations.v1;

public sealed class AddBookDetailsRequestValidator : AbstractValidator<AddBookDetailsRequest>
{
    public AddBookDetailsRequestValidator()
    {
        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Описание не может превышать 2000 символов.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        // Валидация файла обложки
        RuleFor(x => x.CoverImage)
            .SetValidator(new CoverImageValidator())
            .When(x => x.CoverImage != null);
    }
}

public class CoverImageValidator : AbstractValidator<IFormFile>
{
    public CoverImageValidator()
    {
        // Проверка размера файла (максимум 5MB)
        RuleFor(x => x.Length)
            .LessThanOrEqualTo(10 * 1024 * 1024) // 10MB
            .WithMessage("Размер обложки не должен превышать 10 МБ.");

        // Проверка формата файла
        RuleFor(x => x.ContentType)
            .Must(contentType => new[] { "image/jpeg", "image/png", "image/gif", "image/webp" }
                .Contains(contentType?.ToLower()))
            .WithMessage("Допустимые форматы обложки: JPEG, PNG, GIF, WebP.");

        // Альтернативная проверка по расширению файла
        RuleFor(x => x.FileName)
            .Must(fileName => 
                new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }
                    .Contains(Path.GetExtension(fileName).ToLower()))
            .WithMessage("Допустимые расширения файлов: JPG, JPEG, PNG, GIF, WebP.");

        // Дополнительная проверка на пустой файл
        RuleFor(x => x.Length)
            .GreaterThan(0)
            .WithMessage("Файл обложки не может быть пустым.")
            .When(x => x != null);
    }
}
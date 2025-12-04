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
        
        RuleFor(x => x.CoverImage)
            .SetValidator(new CoverImageValidator())
            .When(x => x.CoverImage != null);
    }
}

public class CoverImageValidator : AbstractValidator<IFormFile>
{
    public CoverImageValidator()
    {
        RuleFor(x => x.Length)
            .LessThanOrEqualTo(10 * 1024 * 1024) 
            .WithMessage("Размер обложки не должен превышать 10 МБ.");
        
        RuleFor(x => x.ContentType)
            .Must(contentType => new[] { "image/jpeg", "image/png", "image/gif", "image/webp" }
                .Contains(contentType?.ToLower()))
            .WithMessage("Допустимые форматы обложки: JPEG, PNG, GIF, WebP.");
        
        RuleFor(x => x.FileName)
            .Must(fileName => 
                new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }
                    .Contains(Path.GetExtension(fileName).ToLower()))
            .WithMessage("Допустимые расширения файлов: JPG, JPEG, PNG, GIF, WebP.");
        
        RuleFor(x => x.Length)
            .GreaterThan(0)
            .WithMessage("Файл обложки не может быть пустым.")
            .When(x => x != null);
    }
}
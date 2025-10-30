using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Controllers.Mappers.v1;

public static class ReaderExtensions
{
    public static Reader ToReader(this CreateReaderRequest request)
    {
        var now = DateTime.UtcNow;

        return new Reader
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            ExpiryDate = request.ExpiryDate,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
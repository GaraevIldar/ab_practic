using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Storage;

public interface IReaderRepository
{
    Task<Guid> CreateReader(Reader book);
    Task<Guid> UpdateReaderExpiryDateAsync(Guid id, ExtendReaderRequest request);
    Task<bool> IsReaderExist(Guid id);
    Task<Guid> CloseReaderCard(Guid id);
    Task<IList<Book>> GetReaderBooks(Guid readerId);
}
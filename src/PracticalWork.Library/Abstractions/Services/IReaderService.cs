using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Contracts.v1.Books.Response;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Services;

public interface IReaderService
{
    Task<Guid> CreateReader(Reader book);

    Task<Guid> ExtendReaderCard(Guid id, ExtendReaderRequest request);
    Task<CloseReaderCardResponse> CloseReaderCard(Guid id);
}
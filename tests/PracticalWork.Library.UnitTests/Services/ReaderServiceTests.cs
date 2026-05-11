using FluentAssertions;
using Moq;
using PracticalWork.Library.Abstractions.MessageBroker;
using PracticalWork.Library.Abstractions.Storage;
using PracticalWork.Library.Contracts.v1.Books.Request;
using PracticalWork.Library.Exceptions;
using PracticalWork.Library.Exceptions.Reader;
using PracticalWork.Library.Models;
using PracticalWork.Library.Services;
using PracticalWork.Library.UnitTests.TestHelpers;
using Xunit;

namespace PracticalWork.Library.UnitTests.Services;

public class ReaderServiceTests
{
    private readonly Mock<IReaderRepository> _repo = new();
    private readonly Mock<IRabbitPublisher> _publisher = new();
    private readonly ReaderService _sut;

    public ReaderServiceTests()
    {
        _sut = new ReaderService(_repo.Object, _publisher.Object, TestRabbitConfig.Create(), TimeProvider.System);
    }

    [Fact]
    public async Task CreateReader_PublishesAndReturnsId()
    {
        var newId = Guid.NewGuid();
        var reader = new Reader
        {
            FullName = "Иванов И.И.",
            PhoneNumber = "+79991234567",
            ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))
        };
        _repo.Setup(r => r.CreateReader(reader)).ReturnsAsync(newId);

        var result = await _sut.CreateReader(reader);

        result.Should().Be(newId);
        _publisher.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<It.IsAnyType>()), Times.Once);
    }

    [Fact]
    public async Task CreateReader_WhenRepositoryThrows_WrapsInServiceException()
    {
        _repo.Setup(r => r.CreateReader(It.IsAny<Reader>())).ThrowsAsync(new InvalidOperationException("db"));

        var act = () => _sut.CreateReader(new Reader());

        await act.Should().ThrowAsync<ReaderServiceException>();
    }

    [Fact]
    public async Task ExtendReaderCard_WhenReaderNotFound_Throws()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.IsReaderExist(id)).ReturnsAsync(false);

        var act = () => _sut.ExtendReaderCard(id, new ExtendReaderRequest(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))));

        await act.Should().ThrowAsync<ReaderNotFoundException>();
    }

    [Fact]
    public async Task ExtendReaderCard_WhenReaderExists_CallsRepository()
    {
        var id = Guid.NewGuid();
        var request = new ExtendReaderRequest(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)));
        _repo.Setup(r => r.IsReaderExist(id)).ReturnsAsync(true);
        _repo.Setup(r => r.UpdateReaderExpiryDateAsync(id, request)).ReturnsAsync(id);

        var result = await _sut.ExtendReaderCard(id, request);

        result.Should().Be(id);
    }

    [Fact]
    public async Task CloseReaderCard_WhenReaderNotFound_Throws()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.IsReaderExist(id)).ReturnsAsync(false);

        var act = () => _sut.CloseReaderCard(id);

        await act.Should().ThrowAsync<ReaderNotFoundException>();
    }

    [Fact]
    public async Task CloseReaderCard_WhenHasBorrows_Throws()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.IsReaderExist(id)).ReturnsAsync(true);
        _repo.Setup(r => r.IsBookBorrowsExist(id)).ReturnsAsync(true);
        _repo.Setup(r => r.GetBookNonReturners(id)).ReturnsAsync("Война и мир");

        var act = () => _sut.CloseReaderCard(id);

        await act.Should().ThrowAsync<ReaderHasBorrowedBooksException>();
    }

    [Fact]
    public async Task CloseReaderCard_WhenValid_PublishesAndReturnsId()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.IsReaderExist(id)).ReturnsAsync(true);
        _repo.Setup(r => r.IsBookBorrowsExist(id)).ReturnsAsync(false);
        _repo.Setup(r => r.GetReaderFullNameById(id)).ReturnsAsync("Иванов");
        _repo.Setup(r => r.CloseReaderCard(id)).ReturnsAsync(id);

        var result = await _sut.CloseReaderCard(id);

        result.Should().Be(id.ToString());
        _publisher.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<It.IsAnyType>()), Times.Once);
    }

    [Fact]
    public async Task GetBooksReaders_WhenReaderNotFound_Throws()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.IsReaderExist(id)).ReturnsAsync(false);

        var act = () => _sut.GetBooksReaders(id);

        await act.Should().ThrowAsync<ReaderNotFoundException>();
    }

    [Fact]
    public async Task GetBooksReaders_WhenReaderExists_ReturnsBooks()
    {
        var id = Guid.NewGuid();
        var expected = new List<Book> { new() { Id = Guid.NewGuid(), Title = "T" } };
        _repo.Setup(r => r.IsReaderExist(id)).ReturnsAsync(true);
        _repo.Setup(r => r.GetReaderBooks(id)).ReturnsAsync(expected);

        var result = await _sut.GetBooksReaders(id);

        result.Should().BeEquivalentTo(expected);
    }
}

using System.Text.Json;
using FluentAssertions;
using PracticalWork.Library.Events;
using PracticalWork.Library.Models;
using Xunit;

namespace PracticalWork.Library.UnitTests.Domain;

public class ActivityLogTests
{
    [Fact]
    public void SerializeEvent_ReturnsValidJson()
    {
        var bookId = Guid.NewGuid();
        var log = new ActivityLog
        {
            EventType = "book.created",
            EventDate = DateTime.UtcNow,
            Event = new BookCreatedEvent(bookId, "Война и мир",
                "FictionBook", new[] { "Л. Толстой" }, 1869)
        };

        var json = log.SerializeEvent();

        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("book.created");
        json.Should().Contain("library-service");
    }

    [Fact]
    public void DeserializeEvent_BookCreated_ReturnsCorrectType()
    {
        var evt = new BookCreatedEvent(Guid.NewGuid(), "Книга",
            "ScientificBook", new[] { "Автор" }, 2020);
        var json = JsonSerializer.Serialize(evt);

        var result = ActivityLog.DeserializeEvent("book.created", json);

        result.Should().BeOfType<BookCreatedEvent>();
        ((BookCreatedEvent)result).Title.Should().Be("Книга");
    }

    [Fact]
    public void DeserializeEvent_BookArchived_ReturnsCorrectType()
    {
        var evt = new BookArchivedEvent(Guid.NewGuid(), "Книга", "причина", DateTime.UtcNow);
        var json = JsonSerializer.Serialize(evt);

        var result = ActivityLog.DeserializeEvent("book.archived", json);

        result.Should().BeOfType<BookArchivedEvent>();
    }

    [Fact]
    public void DeserializeEvent_BookBorrowed_ReturnsCorrectType()
    {
        var evt = new BookBorrowedEvent(Guid.NewGuid(), Guid.NewGuid(), "T", "R",
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)));
        var json = JsonSerializer.Serialize(evt);

        var result = ActivityLog.DeserializeEvent("book.borrowed", json);

        result.Should().BeOfType<BookBorrowedEvent>();
    }

    [Fact]
    public void DeserializeEvent_BookReturned_ReturnsCorrectType()
    {
        var evt = new BookReturnedEvent(Guid.NewGuid(), Guid.NewGuid(), "T", "R",
            DateOnly.FromDateTime(DateTime.UtcNow));
        var json = JsonSerializer.Serialize(evt);

        var result = ActivityLog.DeserializeEvent("book.returned", json);

        result.Should().BeOfType<BookReturnedEvent>();
    }

    [Fact]
    public void DeserializeEvent_ReaderCreated_ReturnsCorrectType()
    {
        var evt = new ReaderCreatedEvent(Guid.NewGuid(), "Иванов И.И.",
            "+79991234567", DateTime.UtcNow.AddYears(1));
        var json = JsonSerializer.Serialize(evt);

        var result = ActivityLog.DeserializeEvent("reader.created", json);

        result.Should().BeOfType<ReaderCreatedEvent>();
    }

    [Fact]
    public void DeserializeEvent_ReaderClosed_ReturnsCorrectType()
    {
        var evt = new ReaderClosedEvent(Guid.NewGuid(), "Иванов И.И.", DateTime.UtcNow);
        var json = JsonSerializer.Serialize(evt);

        var result = ActivityLog.DeserializeEvent("reader.closed", json);

        result.Should().BeOfType<ReaderClosedEvent>();
    }

    [Fact]
    public void DeserializeEvent_UnknownType_ThrowsJsonException()
    {
        var act = () => ActivityLog.DeserializeEvent("unknown.event", "{}");

        act.Should().Throw<JsonException>();
    }
}

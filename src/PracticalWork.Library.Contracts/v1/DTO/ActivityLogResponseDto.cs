namespace PracticalWork.Library.Contracts.v1.DTO;

/// <summary>
/// DTO записи активности системы
/// </summary>
public record ActivityLogResponseDto(
    string EventType,
    DateTime EventDate,
    object Event,
    string Cursor
);
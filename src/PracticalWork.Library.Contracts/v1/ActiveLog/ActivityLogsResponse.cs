using PracticalWork.Library.Contracts.v1.DTO;

namespace PracticalWork.Library.Contracts.v1.ActiveLog;

/// <summary>
/// Ответ API получения логов активности
/// </summary>
public record ActivityLogsResponse(
    IReadOnlyList<ActivityLogResponseDto> Items,
    string NextCursor
);
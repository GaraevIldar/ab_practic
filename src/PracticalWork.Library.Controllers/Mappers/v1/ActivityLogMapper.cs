using PracticalWork.Library.Contracts.v1.DTO;

namespace PracticalWork.Library.Controllers.Mappers.v1;

public static class ActivityLogMapper
{
    public static ActivityLogResponseDto ToDto(this ActivityLog log)
    {
        return new ActivityLogResponseDto(
            log.EventType,
            log.EventDate,
            log.Event,
            log.Cursor.Encode()
        );
    }
}

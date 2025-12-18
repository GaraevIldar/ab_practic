namespace MessageBroker.RabbitMQ.Data.PostgreSql.Entities;

public class ActivityLog
{
    public Guid Id { get; set; }
    public string EventType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Payload { get; set; }
}

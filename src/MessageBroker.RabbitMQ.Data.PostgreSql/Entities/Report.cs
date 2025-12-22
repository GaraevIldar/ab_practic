namespace MessageBroker.RabbitMQ.Data.PostgreSql.Entities;

public class Report
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public long Size { get; set; }
}

namespace PracticalWork.Library.MessageBroker.RabbitMQ;

public class ReportMetadata
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public long Size { get; set; }
}

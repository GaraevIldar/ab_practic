namespace PracticalWork.Library.MessageBroker.Rabbit.Options;

public class RabbitMqOptions
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Exchange { get; set; } = null!;
}
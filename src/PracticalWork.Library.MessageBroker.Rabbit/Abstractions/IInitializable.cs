namespace PracticalWork.Library.MessageBroker.Rabbit.Abstractions;

public interface IInitializable
{
    Task InitializeAsync();
    bool IsInit { get; set; }
}
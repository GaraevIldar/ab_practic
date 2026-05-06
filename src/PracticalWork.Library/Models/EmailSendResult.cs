namespace PracticalWork.Library.Models;

public sealed class EmailSendResult
{
    public bool IsSuccess { get; private set; }
    public string ErrorMessage { get; private set; }

    public static EmailSendResult Success() => new() { IsSuccess = true };
    public static EmailSendResult Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}

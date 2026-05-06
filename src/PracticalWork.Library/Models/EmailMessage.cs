namespace PracticalWork.Library.Models;

public sealed class EmailMessage
{
    public string To { get; set; }
    public string Subject { get; set; }
    public string HtmlBody { get; set; }
    public string PlainTextBody { get; set; }
}

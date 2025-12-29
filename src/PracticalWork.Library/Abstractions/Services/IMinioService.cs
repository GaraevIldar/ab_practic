namespace PracticalWork.Library.Data.Minio;

public interface IMinioService
{
    string Endpoint { get; }
    Task<string> UploadFileAsync(string objectName, Stream fileStream, string contentType);
    Task<string> GetFileLinkAsync(string bucket, string fileName);
}
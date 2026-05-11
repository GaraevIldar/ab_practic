namespace PracticalWork.Library.Data.Minio;

/// <summary>
/// Сервис хранилища файлов MinIO
/// </summary>
public interface IMinioService
{
    /// <summary>
    /// Адрес MinIO endpoint
    /// </summary>
    string Endpoint { get; }

    /// <summary>
    /// Загрузка файла в bucket по умолчанию
    /// </summary>
    Task<string> UploadFileAsync(string objectName, Stream fileStream, string contentType);

    /// <summary>
    /// Загрузка файла в указанный bucket
    /// </summary>
    Task<string> UploadFileAsync(string bucket, string objectName, Stream fileStream, string contentType);

    /// <summary>
    /// Получение presigned URL для скачивания файла
    /// </summary>
    Task<string> GetFileLinkAsync(string bucket, string fileName);
}
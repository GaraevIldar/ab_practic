using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using PracticalWork.Library.Data.Minio;

public class MinioService : IMinioService
{
    private readonly MinioClient _client;
    private readonly MinioOptions _options;
    public string Endpoint => _options.Endpoint;


    public MinioService(IOptions<MinioOptions> options)
    {
        _options = options.Value;

        _client = (MinioClient)new MinioClient()
            .WithEndpoint(_options.Endpoint)
            .WithCredentials(_options.AccessKey, _options.SecretKey)
            .Build();
        
        var exists = _client.BucketExistsAsync(new BucketExistsArgs()
            .WithBucket(_options.BucketName)).Result;
        if (!exists)
        {
            _client.MakeBucketAsync(new MakeBucketArgs()
                .WithBucket(_options.BucketName)).Wait();
        }
    }

    public async Task<string> UploadFileAsync(string objectName, Stream fileStream, string contentType)
    {
        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectName)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType));
        
        return $"{_options.BucketName}/{objectName}";
    }
}
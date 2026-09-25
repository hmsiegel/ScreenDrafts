namespace ScreenDrafts.Common.Infrastructure.Storage;

internal sealed class R2FileStorage(IAmazonS3 s3, R2Settings settings) : IFileStorage
{
  private readonly IAmazonS3 _s3 = s3;
  private readonly R2Settings _settings = settings;

  public async Task UploadAsync(
    string key,
    Stream content,
    string contentType,
    string cacheControl,
    CancellationToken cancellationToken = default
  )
  {
    // PutObject needs a known length. Uploads are capped small (avatars/covers),
    // so buffer rather than depend on the request stream being seekable.
    using var buffer = new MemoryStream();
    await content.CopyToAsync(buffer, cancellationToken);
    buffer.Position = 0;

    var request = new PutObjectRequest
    {
      BucketName = _settings.BucketName,
      Key = key,
      InputStream = buffer,
      ContentType = contentType,
      DisablePayloadSigning = true,
    };
    request.Headers.CacheControl = cacheControl;

    await _s3.PutObjectAsync(request, cancellationToken);
  }
}

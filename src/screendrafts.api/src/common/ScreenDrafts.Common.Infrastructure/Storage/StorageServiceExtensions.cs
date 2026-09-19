using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Hosting;
using ScreenDrafts.Common.Application.Storage;

namespace ScreenDrafts.Common.Infrastructure.Storage;

public static class StorageServiceExtensions
{
  /// <summary>
  /// R2 when configured. Without R2 config: local wwwroot outside Production,
  /// startup failure in Production (a silent local fallback there would lose
  /// every upload on the next deploy).
  /// </summary>
  public static IServiceCollection AddFileStorage(
    this IServiceCollection services,
    IConfiguration configuration,
    IHostEnvironment environment
  )
  {
    ArgumentNullException.ThrowIfNull(configuration);

    var settings = configuration.GetSection(R2Settings.SectionName).Get<R2Settings>();

    var configured =
      settings is not null
      && !string.IsNullOrWhiteSpace(settings.AccountId)
      && !string.IsNullOrWhiteSpace(settings.AccessKeyId)
      && !string.IsNullOrWhiteSpace(settings.SecretAccessKey)
      && !string.IsNullOrWhiteSpace(settings.BucketName);

    if (!configured)
    {
      if (environment.IsProduction())
      {
        throw new InvalidOperationException(
          "R2 storage is not configured. Set R2:AccountId, R2:AccessKeyId, "
            + "R2:SecretAccessKey and R2:BucketName."
        );
      }

      services.TryAddSingleton<IFileStorage, LocalFileStorage>();
      return services;
    }

    var s3Config = new AmazonS3Config
    {
      ServiceURL = $"https://{settings!.AccountId}.r2.cloudflarestorage.com",
      AuthenticationRegion = "auto",
      ForcePathStyle = true,
      // AWSSDK.S3 v4 adds checksum headers by default; R2 rejects some of them.
      RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
      ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,
    };

    using (
      var client = new AmazonS3Client(
        new BasicAWSCredentials(settings.AccessKeyId, settings.SecretAccessKey),
        s3Config
      )
    )
    {
      services.TryAddSingleton<IAmazonS3>(client);
    }

    services.TryAddSingleton(settings);
    services.TryAddSingleton<IFileStorage, R2FileStorage>();

    return services;
  }
}

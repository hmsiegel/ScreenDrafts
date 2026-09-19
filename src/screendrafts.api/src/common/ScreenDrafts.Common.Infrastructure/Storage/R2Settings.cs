namespace ScreenDrafts.Common.Infrastructure.Storage;

internal sealed class R2Settings
{
  public const string SectionName = "R2";

  public string AccountId { get; init; } = default!;
  public string AccessKeyId { get; init; } = default!;
  public string SecretAccessKey { get; init; } = default!;
  public string BucketName { get; init; } = default!;
}

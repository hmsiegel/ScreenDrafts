namespace ScreenDrafts.Modules.Reporting.PublicApi;

public sealed record DrafterHonorificResponse
{
  public required int HonorificValue { get; init; }
  public required string HonorificName { get; init; }
  public required int AppearanceCount { get; init; }
}

namespace ScreenDrafts.Modules.Drafts.Features.Participants.Get;

internal sealed record HonorificResponse
{
  public required int HonorificValue { get; init; }
  public required string HonorificName { get; init; }
  public required int AppearanceCount { get; init; }
}

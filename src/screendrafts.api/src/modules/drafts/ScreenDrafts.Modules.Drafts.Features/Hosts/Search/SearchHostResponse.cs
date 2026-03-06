namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Search;

internal sealed record SearchHostResponse
{
  public required string PublicId { get; init; }
  public required string PersonPublicId { get; init; }
  public required string FirstName { get; init; }
  public required string LastName { get; init; }
  public string? DisplayName { get; init; }
  public required int HostedDraftPartCount { get; init; }
}

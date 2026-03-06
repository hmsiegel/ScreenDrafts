namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Get;

internal sealed record GetHostResponse
{
  public required string PublicId { get; init; }
  public required string PublicPersonId { get; init; }
  public required string FirstName { get; init; }
  public required string LastName { get; init; }
  public string? DisplayName { get; init; }
  public IReadOnlyList<HostedDraftPartResponse> HostedDraftParts { get; init; } = [];
}


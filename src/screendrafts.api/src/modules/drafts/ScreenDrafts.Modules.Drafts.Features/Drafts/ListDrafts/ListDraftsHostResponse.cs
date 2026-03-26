namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListDrafts;

public sealed record ListDraftsHostResponse
{
  public string HostPublicId { get; init; } = default!;
  public string DisplayName { get; init; } = default!;
  public HostRole Role { get; init; } = default!;
}

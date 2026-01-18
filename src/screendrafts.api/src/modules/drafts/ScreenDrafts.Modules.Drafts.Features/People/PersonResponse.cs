namespace ScreenDrafts.Modules.Drafts.Features.People;

internal sealed record PersonResponse
{
  public Guid UserId { get; init; }
  public string PublicId { get; init; } = default!;
  public string FirstName { get; init; } = default!;
  public string LastName { get; init; } = default!;
  public string DisplayName { get; init; } = default!;
  public string? DrafterPublicId { get; init; }
  public string? HostPublicId { get; init; }
}

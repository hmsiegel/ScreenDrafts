namespace ScreenDrafts.Modules.Drafts.Features.Drafters.Get;

internal sealed record Response
{
  public required string PersonId { get; init; } 
  public required string DrafterId { get; init; } 
  public required string DisplayName { get; init; } 

  public required bool IsRetired { get; init; }
  public DateTime? RetiredOnUtc { get; init; }
}

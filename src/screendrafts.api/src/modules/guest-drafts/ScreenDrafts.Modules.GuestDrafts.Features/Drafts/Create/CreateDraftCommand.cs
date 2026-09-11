namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Create;

internal sealed record CreateDraftCommand : ICommand<string>
{
  public required string OwnerUserPublicId { get; init; }
  public required string Title { get; init; }
  public required string Type { get; init; }
  public DateOnly? DraftDate { get; init; }
  public required int NumberOfPicks { get; init; }

  /// <summary>
  /// Only used for non-fixed types (MiniMega/Super/Mega). Ignored for
  /// Standard/MiniSuper, which always apply the confirmed fixed template
  /// (GuestDraftBoardTemplates) regardless of what's sent here.
  /// </summary>
  public IReadOnlyList<GuestDraftPositionInput> Positions { get; init; } = [];
}

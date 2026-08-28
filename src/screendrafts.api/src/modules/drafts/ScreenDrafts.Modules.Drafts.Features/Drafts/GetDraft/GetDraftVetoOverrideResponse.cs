namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed record GetDraftVetoOverrideResponse
{
  public Guid IssuedByParticipantId { get; init; }
  public string? IssuedByDisplayName { get; init; }
  public string? ActedByPublicId { get; init; }
  public string? ActedByDisplayName { get; init; }

  /// <summary>
  /// Stamped automatically with the draft's FungibleTokenName when SpentFromFungiblePool
  /// is true. There is no front-end field for entering this manually.
  /// </summary>
  public string? Note { get; init; }

  /// <summary>
  /// True when this override was paid for by a fungible token rather than a normal,
  /// awarded override.
  /// </summary>
  public bool SpentFromFungiblePool { get; init; }
}

namespace ScreenDrafts.Modules.Drafts.Features.Drafters.ReadModel;

public sealed record RolloverHistoryReadModel
{
  public Guid ToDraftPartId { get; init; }
  public int ToDraftPartIndex { get; init; }
  public string ToDraftTitle { get; init; } = string.Empty;

  public Guid ParticipantIdValue { get; init; }
  public ParticipantKind ParticipantKind { get; init; } = default!;

  public Guid FromDraftPartId { get; init; }
  public int FromDraftPartIndex { get; init; }
  public string FromDraftTitle { get; init; } = string.Empty;

  public int VetoesRollingIn { get; init; }
  public int VetoOverridesRollingIn { get; init; }
}

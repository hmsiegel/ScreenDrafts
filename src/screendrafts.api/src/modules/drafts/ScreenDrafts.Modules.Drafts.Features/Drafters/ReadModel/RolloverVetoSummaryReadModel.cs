using ScreenDrafts.Modules.Drafts.Domain.Participants;

namespace ScreenDrafts.Modules.Drafts.Features.Drafters.ReadModel;

public sealed record RolloverSummaryReadModel
{
  public Guid DrafttPartId { get; init; }
  public int DraftPartIndex { get; init; }
  public string DraftTitle { get; init; } = string.Empty;
  public Guid ParticipantIdValue { get; init; }
  public ParticipantKind ParticipantKind { get; init; } = default!;

  public int VetoesRollingIn { get; init; }
  public int VetoOverridesRollingIn { get; init; }
  public int VetoesUsed { get; init; }
  public int VetoOverridesUsed { get; init; }
  public int VetoesRollingOver { get; init; }
  public int VetoOverridesRollingOver { get; init; }
}

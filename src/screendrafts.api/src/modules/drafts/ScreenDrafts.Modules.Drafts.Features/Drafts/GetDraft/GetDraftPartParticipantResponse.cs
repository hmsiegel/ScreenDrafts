namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed record GetDraftPartParticipantResponse
{
  public Guid ParticipantIdValue { get; init; }
  public ParticipantKind ParticipantKindValue { get; init; } = default!;
  public int StartingVetoes { get; init; }
  public int VetoesUsed { get; init; }
  public int RolloverVetoes { get; init; }
  public int TriviaVetoes { get; init; }
  public int VetoOverridesUsed { get; init; }
  public int RolloverVetoOverride { get; init; }
  public int TriviaVetoOverride { get; init; }
  public int CommissionerOverride { get; init; }
}

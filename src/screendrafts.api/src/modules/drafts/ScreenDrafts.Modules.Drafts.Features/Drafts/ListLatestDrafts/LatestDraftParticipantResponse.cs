namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListLatestDrafts;

internal sealed record LatestDraftParticipantResponse 
{
  public Guid ParticipantIdValue { get; init; }
  public ParticipantKind ParticipantKindValue { get; init; } = default!;
  public string DisplayName { get; init; } = default!;
}

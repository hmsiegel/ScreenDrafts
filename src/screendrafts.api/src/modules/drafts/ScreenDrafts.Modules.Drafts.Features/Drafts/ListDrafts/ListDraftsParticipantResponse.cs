namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListDrafts;

public sealed record ListDraftsParticipantResponse
{
  public Guid ParticipantIdValue { get; init; }
  public ParticipantKind ParticipantKindValue { get; init; } = default!;
  public string DisplayName { get; init; } = default!;
}

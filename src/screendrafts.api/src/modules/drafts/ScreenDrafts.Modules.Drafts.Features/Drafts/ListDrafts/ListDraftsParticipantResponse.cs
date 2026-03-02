namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListDrafts;

public sealed record ListDraftsParticipantResponse
{
  public Guid ParticipantIdValue { get; init; }
  public int ParticipantKindValue { get; init; }
  public string DisplayName { get; init; } = default!;
}

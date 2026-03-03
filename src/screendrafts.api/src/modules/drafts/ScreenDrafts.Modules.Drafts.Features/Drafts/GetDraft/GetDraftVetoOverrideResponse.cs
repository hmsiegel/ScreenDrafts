namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed record GetDraftVetoOverrideResponse
{
  public Guid IssuedByParticipantId { get; init; }
  public string? ActedByPublicId { get; init; }
}

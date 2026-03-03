namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed record GetDraftVetoResponse
{
  public Guid IssuedByParticipantId { get; init; }
  public string? ActedByPublicId { get; init; }
  public bool IsOverriden { get; init; }
  public string? Note { get; init; }
  public DateTime OccurredOnUtc { get; init; }
  public GetDraftVetoOverrideResponse? Override { get; init; }
}

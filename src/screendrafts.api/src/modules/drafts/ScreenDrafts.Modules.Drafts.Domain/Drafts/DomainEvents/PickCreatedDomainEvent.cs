namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class PickCreatedDomainEvent(
  Guid pickId,
  Guid participantId,
  string participantKind,
  Guid draftPartId,
  int position,
  int playOrder,
  Guid movieId) : DomainEvent
{
  public Guid PickId { get; init; } = pickId;
  public Guid ParticipantId { get; init; } = participantId;
  public string ParticipantKind { get; init; } = participantKind;
  public Guid DraftPartId { get; init; } = draftPartId;
  public int Position { get; init; } = position;
  public int PlayOrder { get; init; } = playOrder;
  public Guid MovieId { get; init; } = movieId;
}

namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.DomainEvents;

public sealed class TriviaResultsAssignedDomainEvent(
  Guid draftPartId,
  IReadOnlyList<(Guid ParticipantId, int Position)> triviaResults) : DomainEvent
{
  public Guid DraftPartId { get; init; } = draftPartId;
  public IReadOnlyList<(Guid ParticipantId, int Position)> TriviaResults { get; init; } = triviaResults;
}

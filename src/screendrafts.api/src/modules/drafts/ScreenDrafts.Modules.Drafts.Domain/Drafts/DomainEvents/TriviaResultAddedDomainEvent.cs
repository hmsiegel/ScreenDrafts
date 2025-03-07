﻿namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.DomainEvents;

public sealed class TriviaResultAddedDomainEvent(
  Guid draftId,
  Guid drafterId,
  int position,
  int questionsWon) : DomainEvent
{
  public Guid DraftId { get; init; } = draftId;

  public Guid DrafterId { get; init; } = drafterId;

  public int Position { get; init; } = position;

  public int QuestionsWon { get; init; } = questionsWon;
}

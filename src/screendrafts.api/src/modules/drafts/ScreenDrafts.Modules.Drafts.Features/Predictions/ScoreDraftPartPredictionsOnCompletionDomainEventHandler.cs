namespace ScreenDrafts.Modules.Drafts.Features.Predictions;

internal class ScoreDraftPartPredictionsOnCompletionDomainEventHandler(ISender sender)
  : DomainEventHandler<DraftPartCompletedDomainEvent>
{
  private readonly ISender _sender = sender;

  public override async Task Handle(
    DraftPartCompletedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    var command = new ScoreDraftPartPredictionsCommand
    {
      DraftPartId = domainEvent.DraftPartPublicId,
      FinalTmdbIds = domainEvent.LandedTmdbIds,
    };

    var result = await _sender.Send(command, cancellationToken);

    // No prediction rules configured for this part — predictions were off. Not an error.
    if (result.IsFailure && result.Errors[0].Code != "PredictionErrors.RulesNotFound")
    {
      throw new ScreenDraftsException(
        $"Prediction scoring failed for draft part '{domainEvent.DraftPartPublicId}': "
          + result.Errors[0].Description
      );
    }
  }
}

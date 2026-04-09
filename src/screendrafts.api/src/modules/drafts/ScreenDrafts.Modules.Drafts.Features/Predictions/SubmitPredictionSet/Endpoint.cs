namespace ScreenDrafts.Modules.Drafts.Features.Predictions.SubmitPredictionSet;

internal sealed class Endpoint : ScreenDraftsEndpoint<SubmitPredictionSetRequest>
{
  public override void Configure()
  {
    Post(PredictionRoutes.SubmitSet);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Predictions)
        .WithName(DraftsOpenApi.Names.Predictions_SubmitSet)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    });
    Policies(DraftsAuth.Permissions.PredictionSubmit);
  }

  public override async Task HandleAsync(SubmitPredictionSetRequest req, CancellationToken ct)
  {
    var command = new SubmitPredictionSetCommand
    {
      DraftPartPublicId = req.DraftPartPublicId,
      SeasonPublicId = req.SeasonPublicId,
      ContestantPublicId = req.ContestantPublicId,
      SubmittedByPersonPublicId = req.SubmittedByPersonPublicId,
      SourceKind = req.SourceKind,
      Entries = [.. req.Entries.Select(e => new PredictionEntryDto
            {
              MediaPublicId = e.MediaPublicId,
              MediaTitle = e.MediaTitle,
              OrderIndex = e.OrderIndex,
              Notes = e.Notes
            })]
    };

    var result = await Sender.Send(command, ct);
    await this.SendNoContentAsync(result, ct);
  }
}

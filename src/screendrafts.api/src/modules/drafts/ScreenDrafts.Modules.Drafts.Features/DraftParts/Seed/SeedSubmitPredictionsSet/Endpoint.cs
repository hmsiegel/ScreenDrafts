namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Seed.SeedSubmitPredictionsSet;

internal sealed class Endpoint : ScreenDraftsEndpoint<SeedSubmitPredictionSetRequest>
{
  public override void Configure()
  {
    Post(PredictionRoutes.SeedSubmitSet);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Predictions)
        .WithName(DraftsOpenApi.Names.Predictions_SeedSubmitSet)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    });
    Policies(DraftsAuth.Permissions.DraftSeed);
  }

  public override async Task HandleAsync(SeedSubmitPredictionSetRequest req, CancellationToken ct)
  {
    var command = new SeedSubmitPredictionSetCommand
    {
      DraftPartPublicId = req.DraftPartId,
      SeasonPublicId = req.SeasonPublicId,
      ContestantPublicId = req.ContestantPublicId,
      SubmittedByPersonPublicId = req.SubmittedByPersonPublicId,
      SourceKind = req.SourceKind,
      Entries =
      [
        .. req.Entries.Select(e => new PredictionEntryDto
        {
          TmdbId = e.TmdbId,
          MediaTitle = e.MediaTitle,
          OrderIndex = e.OrderIndex,
          Notes = e.Notes,
        }),
      ],
    };

    var result = await Sender.Send(command, ct);
    await this.SendNoContentAsync(result, ct);
  }
}

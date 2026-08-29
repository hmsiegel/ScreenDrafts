namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetTvSeriesRestriction;

internal sealed class Endpoint : ScreenDraftsEndpoint<SetTvSeriesRestrictionRequest>
{
  public override void Configure()
  {
    Put(DraftRoutes.TvSeriesRestriction);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Drafts)
        .WithName(DraftsOpenApi.Names.Drafts_SetTvSeriesRestriction)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftUpdate);
  }

  public override async Task HandleAsync(SetTvSeriesRestrictionRequest req, CancellationToken ct)
  {
    var command = new SetTvSeriesRestrictionCommand
    {
      PublicId = req.PublicId,
      TvSeriesTmdbId = req.TvSeriesTmdbId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

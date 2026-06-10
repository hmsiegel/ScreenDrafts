namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.RemoveMovie;

internal sealed class Endpoint : ScreenDraftsEndpoint<RemoveMovieFromDraftPoolRequest>
{
  public override void Configure()
  {
    Delete(DraftRoutes.PoolItemById);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftPools)
        .WithName(DraftsOpenApi.Names.DraftPools_RemoveMovie)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPoolUpdate);
  }

  public override async Task HandleAsync(RemoveMovieFromDraftPoolRequest req, CancellationToken ct)
  {
    var command = new RemoveMovieFromDraftPoolCommand
    {
      PublicId = req.PublicId,
      TmdbId = req.TmdbId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

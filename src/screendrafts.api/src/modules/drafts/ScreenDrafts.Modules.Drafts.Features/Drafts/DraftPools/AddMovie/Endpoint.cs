namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.AddMovie;

internal sealed class Endpoint : ScreenDraftsEndpoint<AddMovieToDraftPoolRequest>
{
  public override void Configure()
  {
    Post(DraftRoutes.PoolItem);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftPools)
      .WithName(DraftsOpenApi.Names.DraftPools_AddItem)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPoolUpdate);
  }

  public override async Task HandleAsync(AddMovieToDraftPoolRequest req, CancellationToken ct)
  {
    var command = new AddMovieToDraftPoolCommand
    {
      PublicId = req.PublicId,
      TmdbId = req.TmdbId
    };
    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

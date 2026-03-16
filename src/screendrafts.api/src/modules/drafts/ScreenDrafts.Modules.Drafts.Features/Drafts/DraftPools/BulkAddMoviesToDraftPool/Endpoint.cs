namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.BulkAddMoviesToDraftPool;

internal sealed class Endpoint : ScreenDraftsEndpoint<BulkAddMoviesToDraftPoolRequest, BulkAddMoviesResponse>
{
  public override void Configure()
  {
    Post(DraftRoutes.PoolBulk);
    AllowFileUploads();
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftPools)
      .WithName(DraftsOpenApi.Names.DraftPools_BulkAddItems)
      .Produces<BulkAddMoviesResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPoolCreate);
  }

  public override async Task HandleAsync(BulkAddMoviesToDraftPoolRequest req, CancellationToken ct)
  {
    using var stream = req.File.OpenReadStream();

    var command = new BulkAddMoviesToDraftPoolCommand
    {
      DraftId = req.DraftId,
      CsvStream = stream
    };

    var result = await Sender.Send(command, ct);

    await this.SendOkAsync(result, ct);
  }
}

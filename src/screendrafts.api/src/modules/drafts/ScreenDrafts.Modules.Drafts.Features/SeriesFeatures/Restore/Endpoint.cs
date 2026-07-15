namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Restore;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest
{
  public override void Configure()
  {
    Post(SeriesRoutes.Restore);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.Series_RestoreSeries)
        .WithTags(DraftsOpenApi.Tags.Series)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    });
    Policies(DraftsAuth.Permissions.SeriesRestore);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var publicId = Route<string>("publicId");

    if (string.IsNullOrWhiteSpace(publicId))
    {
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
      return;
    }

    var command = new RestoreSeriesCommand { PublicId = publicId };
    var result = await Sender.Send(command, ct);
    await this.SendNoContentAsync(result, ct);
  }
}

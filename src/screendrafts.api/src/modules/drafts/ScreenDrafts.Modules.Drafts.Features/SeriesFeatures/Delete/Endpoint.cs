namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Delete;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest
{
  public override void Configure()
  {
    Delete(SeriesRoutes.ById);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.Series_DeleteSeries)
        .WithTags(DraftsOpenApi.Tags.Series)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    });
    Policies(DraftsAuth.Permissions.SeriesDelete);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var publicId = Route<string>("publicId");

    if (string.IsNullOrWhiteSpace(publicId))
    {
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
      return;
    }

    var command = new DeleteSeriesCommand { PublicId = publicId };
    var result = await Sender.Send(command, ct);
    await this.SendNoContentAsync(result, ct);
  }
}

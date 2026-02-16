namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Edit;

internal sealed class Endpoint : ScreenDraftsEndpoint<EditSeriesRequest>
{
  public override void Configure()
  {
    Patch(SeriesRoutes.ById);
    Description(x =>
    {
      x.WithDescription("Edits an existing series.")
      .WithTags(DraftsOpenApi.Tags.Series)
      .WithName(DraftsOpenApi.Names.Series_EditSeries)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Permissions(DraftsAuth.Permissions.CampaignUpdate);
  }

  public override async Task HandleAsync(EditSeriesRequest req, CancellationToken ct)
  {
    var EditSeriesFeatureCommand = new EditSeriesCommand
    {
      Name = req.Name!,
      PublicId = req.PublicId,
      Kind = req.Kind,
      CanonicalPolicy = req.CanonicalPolicy,
      ContinuityScope = req.ContinuityScope,
      ContinuityDateRule = req.ContinuityDateRule,
      AllowedDraftTypes = (int)req.AllowedDraftTypes,
      DefaultDraftType = req.DefaultDraftType
    };

    var result = await Sender.Send(EditSeriesFeatureCommand, ct);

    await this.SendNoContentAsync(result, ct);
  }
}



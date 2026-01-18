namespace ScreenDrafts.Modules.Drafts.Features.Series.Edit;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request>
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
    Permissions(Features.Permissions.CampaignUpdate);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var command = new Command
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

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

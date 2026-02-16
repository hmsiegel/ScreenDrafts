namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Update;

internal sealed class Endpoint : ScreenDraftsEndpoint<UpdateDraftRequest>
{
  public override void Configure()
  {
    Put(DraftRoutes.ById);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Drafts)
      .WithName(DraftsOpenApi.Names.Drafts_UpdateDraft)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(DraftsAuth.Permissions.DraftUpdate);
  }

  public override async Task HandleAsync(UpdateDraftRequest req, CancellationToken ct)
  {
    var UpdateDraftCommand = new UpdateDraftCommand
    {
      PublicId = req.PublicId,
      Title = req.Title,
      Description = req.Description,
      SeriesPublicId = req.SeriesPublicId,
      CampaignPublicId = req.CampaignPublicId,
      PublicCategoryIds = req.PublicCategoryIds,
      DraftTypeValue = req.DraftTypeValue
    };

    var result = await Sender.Send(UpdateDraftCommand, ct);

    await this.SendNoContentAsync(result, ct);
  }
}



namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AdvanceSubDraft;

internal sealed class Endpoint : ScreenDraftsEndpoint<AdvanceSubDraftRequest>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.SubDraftAdvance);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
      .WithName(DraftsOpenApi.Names.SubDrafts_Advance)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.SubDraftUpdate);
  }

  public override async Task HandleAsync(AdvanceSubDraftRequest req, CancellationToken ct)
  {
    var command = new AdvanceSubDraftCommand
    {
      DraftPartPublicId = req.DraftPartPublicId,
      SubDraftPublicId = req.SubDraftPublicId
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}


namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.SetSubDraftSubject;

internal sealed class Endpoint : ScreenDraftsEndpoint<SetSubDraftSubjectRequest>
{
  public override void Configure()
  {
    Patch(DraftPartRoutes.SubDraftSubject);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
      .WithName(DraftsOpenApi.Names.SubDrafts_SetSubject)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.SubDraftUpdate);
  }

  public override async Task HandleAsync(SetSubDraftSubjectRequest req, CancellationToken ct)
  {
    var command = new SetSubDraftSubjectCommand
    {
      DraftPartPublicId = req.DraftPartPublicId,
      SubDraftPublicId = req.SubDraftPublicId,
      SubjectKind = req.SubjectKind,
      SubjectName = req.SubjectName
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

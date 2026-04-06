namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.ApplySubDraftVeto;

internal sealed class Endpoint : ScreenDraftsEndpoint<ApplySubDraftVetoRequest>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.SubDraftVeto);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
      .WithName(DraftsOpenApi.Names.SubDrafts_ApplyVeto)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.PickVeto);
  }

  public override async Task HandleAsync(ApplySubDraftVetoRequest req, CancellationToken ct)
  {
    var actedBy = User.GetPublicId();

    if(!ParticipantKind.TryFromValue(req.IssuerKind, out var issuerKind))
    {
      AddError(r => r.IssuerKind, "Invalid issuer kind.");
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
      return;
    }

    var command = new ApplySubDraftVetoCommand
    {
      DraftPartPublicId = req.DraftPartPublicId,
      SubDraftPublicId = req.SubDraftPublicId,
      PlayOrder = req.PlayOrder,
      IssuerPublicId = req.IssuerPublicId,
      IssuerKind = issuerKind,
      ActedbyPublicId = actedBy,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

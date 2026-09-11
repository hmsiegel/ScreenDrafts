namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.UpdateDraft;

internal sealed class Endpoint : ScreenDraftsEndpoint<UpdateGuestDraftRequest>
{
  public override void Configure()
  {
    Put(GuestDraftsRoutes.ById);
    Description(x =>
      x.WithTags(GuestDraftsOpenApi.Tags.GuestDrafts)
        .WithName(GuestDraftsOpenApi.Names.GuestDrafts_UpdateGuestDraft)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
    );
    Policies(GuestDraftsAuth.Permissions.GuestDraftUpdate);
  }

  public override async Task HandleAsync(UpdateGuestDraftRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var userPublicId = User.GetUserPublicId();

    if (userPublicId is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, cancellation: ct);
      return;
    }

    var command = new UpdateDraftCommand
    {
      GuestDraftPublicId = req.PublicId,
      CallerUserPublicId = userPublicId,
      Title = req.Title,
      DraftDate = req.DraftDate,
      Type = req.Type,
      NumberOfPicks = req.NumberOfPicks,
      Positions = req.Positions,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

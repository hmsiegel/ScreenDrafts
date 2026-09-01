namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.InviteParticipant;

internal sealed class Endpoint : ScreenDraftsEndpoint<InviteParticipantRequest>
{
  public override void Configure()
  {
    Post(GuestDraftsRoutes.GuestDraftParticipants);
    Description(x =>
    {
      x.WithTags(GuestDraftsOpenApi.Tags.GuestDrafts)
        .WithName(GuestDraftsOpenApi.Names.GuestDrafts_InviteParticipant)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(GuestDraftsAuth.Permissions.GuestDraftInviteParticipant);
  }

  public override async Task HandleAsync(InviteParticipantRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var command = new InviteParticipantCommand
    {
      GuestDraftPublicId = req.PublicId,
      UserPublicId = req.UserPublicId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

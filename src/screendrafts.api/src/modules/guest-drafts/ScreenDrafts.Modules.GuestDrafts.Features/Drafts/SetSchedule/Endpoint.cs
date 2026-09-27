namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.SetSchedule;

// ── Endpoint ──────────────────────────────────────────────────────────────────
internal sealed class Endpoint : ScreenDraftsEndpoint<SetDraftScheduleRequest>
{
  public override void Configure()
  {
    Put(GuestDraftsRoutes.Schedule);
    Description(x =>
      x.WithTags(GuestDraftsOpenApi.Tags.GuestDrafts)
        .WithName(GuestDraftsOpenApi.Names.GuestDrafts_SetSchedule)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict)
    );
    Policies(GuestDraftsAuth.Permissions.GuestDraftUpdate);
  }

  public override async Task HandleAsync(SetDraftScheduleRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var userPublicId = User.GetUserPublicId();

    if (userPublicId is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, cancellation: ct);
      return;
    }

    var command = new SetDraftScheduleCommand
    {
      GuestDraftPublicId = req.PublicId,
      CallerUserPublicId = userPublicId,
      ScheduledForUtc = req.ScheduledForUtc,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

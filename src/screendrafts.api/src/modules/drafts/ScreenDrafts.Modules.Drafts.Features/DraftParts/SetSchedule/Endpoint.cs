namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SetSchedule;

// ── Endpoint ──────────────────────────────────────────────────────────────────
internal sealed class Endpoint : ScreenDraftsEndpoint<SetDraftPartScheduleRequest>
{
  public override void Configure()
  {
    Put(DraftPartRoutes.Schedule);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
        .WithName(DraftsOpenApi.Names.DraftParts_SetSchedule)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(SetDraftPartScheduleRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var command = new SetDraftPartScheduleCommand
    {
      DraftPartId = req.DraftPartId,
      ScheduledForUtc = req.ScheduledForUtc,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

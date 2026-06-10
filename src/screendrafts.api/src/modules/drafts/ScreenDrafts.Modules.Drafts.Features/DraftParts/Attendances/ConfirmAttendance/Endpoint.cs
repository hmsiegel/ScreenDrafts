// ═══════════════════════════════════════════════════════════════════════════════
// ConfirmAttendance — PUT /draft-parts/{draftPartId}/attendances/{personPublicId}/confirm
// Admin confirms person is attending. Pending → Confirmed.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.ConfirmAttendance;

internal sealed class Endpoint : ScreenDraftsEndpoint<ConfirmAttendanceRequest>
{
  public override void Configure()
  {
    Put(DraftPartRoutes.AttendanceConfirm);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Attendances)
        .WithName(DraftsOpenApi.Names.Attendances_Confirm)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(ConfirmAttendanceRequest req, CancellationToken ct)
  {
    var command = new ConfirmAttendanceCommand
    {
      DraftPartId = req.DraftPartId,
      PersonPublicId = req.PersonPublicId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

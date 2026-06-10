// ═══════════════════════════════════════════════════════════════════════════════
// ReinstateAttendance — PUT /draft-parts/{draftPartId}/attendances/{personPublicId}/reinstate
// Admin reinstates a withdrawn record. Withdrawn → Pending.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.ReinstateAttendance;

internal sealed class Endpoint : ScreenDraftsEndpoint<ReinstateAttendanceRequest>
{
  public override void Configure()
  {
    Put(DraftPartRoutes.AttendanceReinstate);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Attendances)
        .WithName(DraftsOpenApi.Names.Attendances_Reinstate)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(ReinstateAttendanceRequest req, CancellationToken ct)
  {
    var command = new ReinstateAttendanceCommand
    {
      DraftPartId = req.DraftPartId,
      PersonPublicId = req.PersonPublicId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

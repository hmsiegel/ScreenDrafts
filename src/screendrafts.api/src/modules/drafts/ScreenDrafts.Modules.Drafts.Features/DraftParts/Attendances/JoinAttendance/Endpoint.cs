// ═══════════════════════════════════════════════════════════════════════════════
// JoinAttendance — PUT /draft-parts/{draftPartId}/attendances/{personPublicId}/join
// Person joins the draft part. Confirmed → Joined.
// Caller must be the person identified by personPublicId (or admin/commissioner).
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.JoinAttendance;

internal sealed class Endpoint : ScreenDraftsEndpoint<JoinAttendanceRequest>
{
  public override void Configure()
  {
    Put(DraftPartRoutes.AttendanceJoin);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Attendances)
        .WithName(DraftsOpenApi.Names.Attendances_Join)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.AttendanceJoin);
  }

  public override async Task HandleAsync(JoinAttendanceRequest req, CancellationToken ct)
  {
    var callerPersonPublicId = User.GetPublicId() ?? string.Empty;

    if (string.IsNullOrWhiteSpace(callerPersonPublicId))
    {
      await Send.UnauthorizedAsync(ct);
      return;
    }

    var command = new JoinAttendanceCommand
    {
      DraftPartId = req.DraftPartId,
      PersonPublicId = req.PersonPublicId,
      CallerPersonPublicId = callerPersonPublicId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

// ═══════════════════════════════════════════════════════════════════════════════
// JoinAttendance — PUT /draft-parts/{draftPartId}/attendances/{personPublicId}/join
// Person joins the draft part. Confirmed → Joined.
// Caller must be the person identified by personPublicId (or admin/commissioner).
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.JoinAttendance;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest
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

  public override async Task HandleAsync(CancellationToken ct)
  {
    var draftPartId = Route<string>("draftPartId");
    var userId = User.GetUserId();

    if (userId == Guid.Empty)
    {
      await Send.UnauthorizedAsync(ct);
      return;
    }

    if (string.IsNullOrWhiteSpace(draftPartId))
    {
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, cancellation: ct);
      return;
    }

    var command = new JoinAttendanceCommand { DraftPartId = draftPartId, UserId = userId };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

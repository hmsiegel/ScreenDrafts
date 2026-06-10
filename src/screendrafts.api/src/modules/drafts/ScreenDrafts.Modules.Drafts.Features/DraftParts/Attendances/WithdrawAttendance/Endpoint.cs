// ═══════════════════════════════════════════════════════════════════════════════
// WithdrawAttendance — PUT /draft-parts/{draftPartId}/attendances/{personPublicId}/withdraw
// Person or admin withdraws. Any non-Withdrawn status → Withdrawn.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.WithdrawAttendance;

internal sealed class Endpoint : ScreenDraftsEndpoint<WithdrawAttendanceRequest>
{
  public override void Configure()
  {
    Put(DraftPartRoutes.AttendanceWithdraw);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Attendances)
        .WithName(DraftsOpenApi.Names.Attendances_Withdraw)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.AttendanceWithdraw);
  }

  public override async Task HandleAsync(WithdrawAttendanceRequest req, CancellationToken ct)
  {
    var callerPersonPublicId = User.GetPublicId() ?? string.Empty;

    if (string.IsNullOrWhiteSpace(callerPersonPublicId))
    {
      await Send.UnauthorizedAsync(ct);
      return;
    }

    var command = new WithdrawAttendanceCommand
    {
      DraftPartId = req.DraftPartId,
      PersonPublicId = req.PersonPublicId,
      CallerPersonPublicId = callerPersonPublicId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

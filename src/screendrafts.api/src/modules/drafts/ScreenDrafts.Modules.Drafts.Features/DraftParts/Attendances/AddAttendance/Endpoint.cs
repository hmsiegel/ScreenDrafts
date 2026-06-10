// ═══════════════════════════════════════════════════════════════════════════════
// AddAttendance — POST /draft-parts/{draftPartId}/attendances
// Admin adds a person (drafter, host, or commissioner). Creates Pending record.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.AddAttendance;

internal sealed class Endpoint : ScreenDraftsEndpoint<AddAttendanceRequest, AddAttendanceResponse>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.Attendances);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Attendances)
        .WithName(DraftsOpenApi.Names.Attendances_Add)
        .Produces<AddAttendanceResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(AddAttendanceRequest req, CancellationToken ct)
  {
    var command = new AddAttendanceCommand
    {
      DraftPartId = req.DraftPartId,
      PersonPublicId = req.PersonPublicId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendOkAsync(result, ct);
  }
}

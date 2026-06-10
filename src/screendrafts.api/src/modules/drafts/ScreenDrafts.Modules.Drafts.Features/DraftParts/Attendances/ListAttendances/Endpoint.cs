// ═══════════════════════════════════════════════════════════════════════════════
// ListAttendances — GET /draft-parts/{draftPartId}/attendances
// Returns all attendance records for a draft part.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Attendances.ListAttendances;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<ListAttendancesRequest, ListAttendancesResponse>
{
  public override void Configure()
  {
    Get(DraftPartRoutes.Attendances);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Attendances)
        .WithName(DraftsOpenApi.Names.Attendances_List)
        .Produces<ListAttendancesResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartRead);
  }

  public override async Task HandleAsync(ListAttendancesRequest req, CancellationToken ct)
  {
    var query = new ListAttendancesQuery { DraftPartId = req.DraftPartId };
    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}

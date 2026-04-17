namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.GetHttpAuditLogs;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetHttpAuditLogsRequest, GetHttpAuditLogsResponse>
{
  public override void Configure()
  {
    Get(AuditRoutes.HttpLogs);
    Description(x =>
    {
      x.WithTags(AuditOpenApi.Tag)
      .WithName(AuditOpenApi.Names.Audit_GetHttpAuditLogs)
      .Produces<GetHttpAuditLogsResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(AuditAuth.Permission.AuditRead);
  }

  public override async Task HandleAsync(GetHttpAuditLogsRequest req, CancellationToken ct)
  {
    var query = new GetHttpAuditLogQuery
    {
      ActorId = req.ActorId,
      From = req.From,
      To = req.To,
      StatusCode = req.StatusCode,
      Endpoint = req.Endpoint,
      CursorId = req.CursorId,
      CursorTimestamp = req.CursorTimestamp,
      PageSize = req.PageSize,
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}


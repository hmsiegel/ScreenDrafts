namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.GetAuthAuditLogs;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetAuthAuditLogsRequest, GetAuthAuditLogsResponse>
{
  public override void Configure()
  {
    Get(AuditRoutes.AuthLogs);
    Description(x =>
    {
      x.WithTags(AuditOpenApi.Tag)
      .WithName(AuditOpenApi.Names.Audit_GetAuthAuditLogs)
      .Produces<GetAuthAuditLogsResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(AuditAuth.Permission.AuditRead);
  }

  public override async Task HandleAsync(GetAuthAuditLogsRequest req, CancellationToken ct)
  {
    var query = new GetAuthAuditLogsQuery
    {
      UserId = req.UserId,
      From = req.From,
      To = req.To,
      EventType = req.EventType,
      CursorId = req.CursorId,
      PageSize = req.PageSize,
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}


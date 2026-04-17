using ScreenDrafts.Modules.Audit.Features.AuditLogs.GetAuthAuditLogs;

namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.GetDomainEventAuditLogs;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetDomainEventAuditLogsRequest, GetDomainEventAuditLogsResponse>
{
  public override void Configure()
  {
    Get(AuditRoutes.DomainEventLogs);
    Description(x =>
    {
      x.WithTags(AuditOpenApi.Tag)
      .WithName(AuditOpenApi.Names.Audit_GetDomainEventAuditLogs)
      .Produces<GetDomainEventAuditLogsResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(AuditAuth.Permission.AuditRead);
  }

  public override async Task HandleAsync(GetDomainEventAuditLogsRequest req, CancellationToken ct)
  {
    var query = new GetDomainEventAuditLogsQuery
    {
      ActorId = req.ActorId,
      From = req.From,
      To = req.To,
      EventType = req.EventType,
      SourceModule = req.SourceModule,
      CursorId = req.CursorId,
      PageSize = req.PageSize,
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}


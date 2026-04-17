using FastEndpoints;
using ScreenDrafts.Modules.Audit.Features.AuditLogs.GetAuthAuditLogs;

namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.GetDomainEventAuditLogs;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get domain event audit logs.";
    Description = "Returns paginated domain event audit logs with optional filters.";
    Response<GetDomainEventAuditLogsResponse>(StatusCodes.Status200OK);
    Response(StatusCodes.Status401Unauthorized);
    Response(StatusCodes.Status403Forbidden);
  }
}

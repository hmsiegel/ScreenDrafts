using FastEndpoints;

namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.GetHttpAuditLogs;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get HTTP audit logs.";
    Description = "Returns paginated HTTP request/response audit logs with optional filters.";
    Response<GetHttpAuditLogsResponse>(StatusCodes.Status200OK);
    Response(StatusCodes.Status401Unauthorized);
    Response(StatusCodes.Status403Forbidden);
  }
}

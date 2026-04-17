using FastEndpoints;

namespace ScreenDrafts.Modules.Audit.Features.AuditLogs.GetAuthAuditLogs;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Get authentication audit logs.";
    Description = "Returns paginated authentication audit logs with optional filters.";
    Response<GetAuthAuditLogsResponse>(StatusCodes.Status200OK);
    Response(StatusCodes.Status401Unauthorized);
    Response(StatusCodes.Status403Forbidden);
  }
}

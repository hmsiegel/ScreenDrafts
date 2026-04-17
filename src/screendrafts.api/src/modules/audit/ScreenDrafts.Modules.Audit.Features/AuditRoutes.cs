namespace ScreenDrafts.Modules.Audit.Features;

internal static class AuditRoutes
{
  private const string Base = "/audit";

  internal const string HttpLogs = $"{Base}/http-logs";
  internal const string HttpLogsExport = $"{Base}/http-logs/export";
  internal const string DomainEventLogs = $"{Base}/domain-events";
  internal const string DomainEventLogsExport = $"{Base}/domain-events/export";
  internal const string AuthLogs = $"{Base}/auth-logs";
  internal const string AuthLogsExport = $"{Base}/auth-logs/export";
}

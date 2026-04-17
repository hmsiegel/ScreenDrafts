namespace ScreenDrafts.Modules.Audit.Features;

internal static class AuditOpenApi
{
  internal const string Tag = "Audit";

  internal static class Names
  {
    internal const string Audit_GetHttpAuditLogs = "Audit.GetHttpAuditLogs";
    internal const string Audit_ExportHttpAuditLogs = "Audit.ExportHttpAuditLogs";
    internal const string Audit_GetDomainEventAuditLogs = "Audit.GetDomainEventAuditLogs";
    internal const string Audit_ExportDomainEventAuditLogs = "Audit.ExportDomainEventAuditLogs";
    internal const string Audit_GetAuthAuditLogs = "Audit.GetAuthAuditLogs";
    internal const string Audit_ExportAuthAuditLogs = "Audit.ExportAuthAuditLogs";
  }
}

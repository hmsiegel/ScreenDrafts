using ScreenDrafts.Modules.Audit.Domain;

namespace ScreenDrafts.Modules.Audit.IntegrationTests.Abstractions;

[Collection(nameof(AuditIntegrationTestCollection))]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Reviewed")]
public abstract class AuditIntegrationTest(AuditIntegrationTestWebAppFactory factory)
  : BaseIntegrationTest<AuditDbContext>(factory)
{
  protected override async Task ClearDatabaseAsync()
  {
    await DbContext.Database.ExecuteSqlRawAsync(
      """
      TRUNCATE TABLE
        audit.outbox_messages,
        audit.inbox_messages,
        audit.http_audit_logs,
        audit.domain_event_audit_logs,
        audit.auth_audit_logs
      RESTART IDENTITY CASCADE;
      """);
  }

  protected IAuditWriteService AuditWriteService =>
    GetService<IAuditWriteService>();

  protected Task WriteHttpLogAsync(HttpAuditLog log) =>
    AuditWriteService.WriteHttpLogAsync(log);

  protected Task WriteDomainEventLogAsync(DomainEventAuditLog log) =>
    AuditWriteService.WriteDomainEventLogAsync(log);

  protected Task WriteAuthLogAsync(AuthAuditLog log) =>
    AuditWriteService.WriteAuthLogAsync(log);
}

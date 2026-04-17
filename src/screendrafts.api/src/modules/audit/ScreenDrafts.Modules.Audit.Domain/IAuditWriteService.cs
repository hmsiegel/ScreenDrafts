namespace ScreenDrafts.Modules.Audit.Domain;

public interface IAuditWriteService
{
  Task WriteHttpLogAsync(HttpAuditLog log, CancellationToken cancellationToken = default);
  Task WriteDomainEventLogAsync(DomainEventAuditLog log, CancellationToken cancellationToken = default);
  Task WriteAuthLogAsync(AuthAuditLog log, CancellationToken cancellationToken = default);
}

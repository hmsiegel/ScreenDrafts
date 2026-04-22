namespace ScreenDrafts.Modules.Audit.IntegrationTests.Abstractions;

public sealed class AuditIntegrationTestWebAppFactory : IntegrationTestWebAppFactory
{
  protected override IEnumerable<Type> GetDbContextTypes()
  {
    const string contextTypeName = "ScreenDrafts.Modules.Audit.Infrastructure.Database.AuditDbContext";

    return [.. AppDomain.CurrentDomain.GetAssemblies()
      .SelectMany(a => a.GetTypes())
      .Where(t => string.Equals(t.FullName, contextTypeName, StringComparison.Ordinal))];
  }

  protected override async Task ApplyMigrationsAsync()
  {
    await base.ApplyMigrationsAsync();

    // The three audit log tables are not EF-managed — they are created by the
    // DbMigrator SQL scripts in production. Run the DDL directly here so that
    // AuditWriteService and query handlers can read/write during tests.
    using var scope = Services.CreateScope();
    var connectionFactory = scope.ServiceProvider
      .GetRequiredService<ScreenDrafts.Common.Application.Data.IDbConnectionFactory>();

    await using var connection = await connectionFactory.OpenConnectionAsync(TestContext.Current.CancellationToken);

    await connection.ExecuteAsync(
      """
      CREATE TABLE IF NOT EXISTS audit.http_audit_logs
      (
          id               uuid        NOT NULL,
          correlation_id   uuid        NOT NULL,
          occurred_on_utc  timestamptz NOT NULL,
          actor_id         text,
          endpoint_name    text        NOT NULL,
          http_method      text        NOT NULL,
          route            text        NOT NULL,
          status_code      int,
          duration_ms      int,
          request_body     jsonb,
          response_body    jsonb,
          ip_address       text,
          CONSTRAINT pk_http_audit_logs PRIMARY KEY (id)
      );

      CREATE TABLE IF NOT EXISTS audit.domain_event_audit_logs
      (
          id               uuid        NOT NULL,
          occurred_on_utc  timestamptz NOT NULL,
          event_type       text        NOT NULL,
          source_module    text        NOT NULL,
          actor_id         text,
          entity_id        text,
          payload          jsonb       NOT NULL,
          CONSTRAINT pk_domain_event_audit_logs PRIMARY KEY (id)
      );

      CREATE TABLE IF NOT EXISTS audit.auth_audit_logs
      (
          id               uuid        NOT NULL,
          occurred_on_utc  timestamptz NOT NULL,
          event_type       text        NOT NULL,
          user_id          text,
          client_id        text,
          ip_address       text,
          details          jsonb,
          CONSTRAINT pk_auth_audit_logs PRIMARY KEY (id)
      );
      """);
  }
}

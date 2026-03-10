namespace ScreenDrafts.Modules.Integrations.IntegrationTests.Abstractions;

[Collection(nameof(IntegrationsIntegrationTestCollection))]
public abstract class IntegrationsIntegrationTest(IntegrationsIntegrationTestWebAppFactory factory)
  : BaseIntegrationTest<IntegrationsDbContext>(factory)
{
  protected FakeTmdbService FakeTmdbService => factory.FakeTmdbService;

  protected override async Task ClearDatabaseAsync()
  {
    await DbContext.Database.ExecuteSqlRawAsync(
      $"""
      TRUNCATE TABLE
        integrations.inbox_messages,
        integrations.inbox_message_consumers,
        integrations.outbox_messages,
        integrations.outbox_message_consumers
      RESTART IDENTITY CASCADE;
      """);

    FakeTmdbService.Reset();
  }
}

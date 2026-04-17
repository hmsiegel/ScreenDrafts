namespace ScreenDrafts.Modules.Audit.IntegrationTests.Abstractions;

[CollectionDefinition(nameof(AuditIntegrationTestCollection))]
public sealed class AuditIntegrationTestCollection : ICollectionFixture<AuditIntegrationTestWebAppFactory>;

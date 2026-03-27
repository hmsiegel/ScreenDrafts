namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Abstractions;

[CollectionDefinition(nameof(ReportingIntegrationTestCollection))]
public sealed class ReportingIntegrationTestCollection
  : ICollectionFixture<ReportingIntegrationTestWebAppFactory>;

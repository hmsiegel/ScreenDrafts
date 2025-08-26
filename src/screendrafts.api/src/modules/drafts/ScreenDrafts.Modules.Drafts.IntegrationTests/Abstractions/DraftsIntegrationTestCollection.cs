namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Abstractions;

[CollectionDefinition(nameof(DraftsIntegrationTestCollection))]
public sealed class DraftsIntegrationTestCollection : ICollectionFixture<DraftsIntegrationTestWebAppFactory>;

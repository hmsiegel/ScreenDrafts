namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Abstractions;

[CollectionDefinition(nameof(IntegrationTestCollection))]
public sealed class IntegrationTestCollection: ICollectionFixture<IntegrationTestWebAppFactory>;

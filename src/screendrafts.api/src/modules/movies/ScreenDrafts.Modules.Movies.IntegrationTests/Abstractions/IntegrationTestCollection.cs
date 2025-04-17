namespace ScreenDrafts.Modules.Movies.IntegrationTests.Abstractions;

[CollectionDefinition(nameof(IntegrationTestCollection))]
public sealed class IntegrationTestCollection: ICollectionFixture<IntegrationTestWebAppFactory>;

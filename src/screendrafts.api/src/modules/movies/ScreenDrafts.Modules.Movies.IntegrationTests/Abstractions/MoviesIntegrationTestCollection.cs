namespace ScreenDrafts.Modules.Movies.IntegrationTests.Abstractions;

[CollectionDefinition(nameof(MoviesIntegrationTestCollection))]
public sealed class MoviesIntegrationTestCollection : ICollectionFixture<MoviesIntegrationTestWebAppFactory>;
// This class has no code, and is never created. Its purpose is simply
// to be the place to apply [CollectionDefinition] and all the
// ICollectionFixture<> interfaces.

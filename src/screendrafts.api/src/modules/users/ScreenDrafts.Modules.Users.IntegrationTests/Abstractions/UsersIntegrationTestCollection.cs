namespace ScreenDrafts.Modules.Users.IntegrationTests.Abstractions;

[CollectionDefinition(nameof(UsersIntegrationTestCollection))]
public sealed class UsersIntegrationTestCollection : ICollectionFixture<UsersIntegrationTestWebAppFactory>;

namespace ScreenDrafts.Modules.Administration.IntegrationTests.Abstractions;

[Collection(nameof(AdministrationIntegrationTestCollection))]
public abstract class AdministrationIntegrationTest(AdministrationIntegrationTestWebAppFactory factory)
  : BaseIntegrationTest<AdministrationDbContext>(factory)
{
  protected override async Task ClearDatabaseAsync()
  {
    await DbContext.Database.ExecuteSqlRawAsync(
      """
      TRUNCATE TABLE
        administration.role_permissions,
        administration.user_roles,
        administration.permissions,
        administration.roles,
        administration.inbox_messages,
        administration.outbox_messages,
        administration.inbox_message_consumers,
        administration.outbox_message_consumers
      RESTART IDENTITY CASCADE;
      """);

    await DbContext.Database.ExecuteSqlRawAsync(
      """
      TRUNCATE TABLE users.users RESTART IDENTITY CASCADE;
      """);
  }

  protected async Task<(Guid UserId, string PublicId)> InsertUserAsync()
  {
    var userId = Guid.NewGuid();
    var publicId = $"u_{Faker.Random.AlphaNumeric(21)}";

    var connectionFactory = GetService<IDbConnectionFactory>();
    await using var connection = await connectionFactory.OpenConnectionAsync(TestContext.Current.CancellationToken);

    await connection.ExecuteAsync(
      """
      INSERT INTO users.users (id, public_id, email, first_name, last_name, identity_id)
      VALUES (@UserId, @PublicId, @Email, @FirstName, @LastName, @IdentityId)
      """,
      new
      {
        UserId = userId,
        PublicId = publicId,
        Email = Faker.Internet.Email(),
        FirstName = Faker.Name.FirstName(),
        LastName = Faker.Name.LastName(),
        IdentityId = Guid.NewGuid().ToString()
      });

    return (userId, publicId);
  }

  protected async Task InsertPermissionAsync(string code)
  {
    var connectionFactory = GetService<IDbConnectionFactory>();
    await using var connection = await connectionFactory.OpenConnectionAsync(TestContext.Current.CancellationToken);

    await connection.ExecuteAsync(
      "INSERT INTO administration.permissions (code) VALUES (@Code)",
      new { Code = code });
  }

  protected async Task InsertRoleAsync(string name)
  {
    var connectionFactory = GetService<IDbConnectionFactory>();
    await using var connection = await connectionFactory.OpenConnectionAsync(TestContext.Current.CancellationToken);

    await connection.ExecuteAsync(
      "INSERT INTO administration.roles (name) VALUES (@Name)",
      new { Name = name });
  }
}

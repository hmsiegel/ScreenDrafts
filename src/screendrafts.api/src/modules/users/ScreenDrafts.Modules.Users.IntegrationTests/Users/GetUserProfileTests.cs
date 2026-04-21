namespace ScreenDrafts.Modules.Users.IntegrationTests.Users;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2234:Pass system uri objects instead of strings", Justification = "Reviewed")]
public class GetUserProfileTests(UsersIntegrationTestWebAppFactory factory) : UsersIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnUnauthorized_WhenAccessTokenNotProvidedAsync()
  {
    // Act
    HttpResponseMessage response = await HttpClient.GetAsync("users/profile");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Should_ReturnOk_WhenUserExistsAsync()
  {
    // Arrange
    string email = "exists@test.com";
    string password = Faker.Internet.Password();
    string accessToken = await RegisterUserAndGetAccessTokenAsync(email, password);

    await DbContext.Database.ExecuteSqlRawAsync(
      """
      INSERT INTO users.user_permissions (user_id, permission_code)
      SELECT id, 'users:read'
      FROM users.users
      WHERE email = {0}
      """,
      email);

    HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
        JwtBearerDefaults.AuthenticationScheme,
        accessToken);

    // Act
    HttpResponseMessage response = await HttpClient.GetAsync("users/profile");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    UserResponse? user = await response.Content.ReadFromJsonAsync<UserResponse>();
    user.Should().NotBeNull();
  }

  private async Task<string> RegisterUserAndGetAccessTokenAsync(string email, string password)
  {
    var request = new RegisterUserRequest
    {
      Email = email,
      Password = password,
      FirstName = Faker.Name.FirstName(),
      LastName = Faker.Name.LastName()
    };

    var registerResponse = await HttpClient.PostAsJsonAsync("users/register", request);
    registerResponse.EnsureSuccessStatusCode();

    string accessToken = await GetAccessTokenAsync(request.Email, request.Password);

    return accessToken;
  }
}

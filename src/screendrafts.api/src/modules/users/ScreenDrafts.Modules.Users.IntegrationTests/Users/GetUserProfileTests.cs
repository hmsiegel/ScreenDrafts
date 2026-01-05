using Serilog;

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
    string accessToken = await RegisterUserAndGetAccessTokenAsync("exists@test.com", Faker.Internet.Password());
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
    Log.Information($"HttpClient BaseAddress: {HttpClient.BaseAddress}", HttpClient.BaseAddress);

    var request = new RegisterUserRequest(
      email,
      password,
      Faker.Name.FirstName(),
      Faker.Name.LastName());

    Log.Information($"Registering user with email: {email}", email);
    var registerResponse = await HttpClient.PostAsJsonAsync("users/register", request);

    Log.Information($"Registration response status code: {registerResponse.StatusCode}", registerResponse.StatusCode);
    var registerContent = await registerResponse.Content.ReadAsStringAsync();
    Log.Information($"Registration response content: {registerContent}", registerContent);

    registerResponse.EnsureSuccessStatusCode();

    Log.Information("Getting access token for {email}", email);
    string accessToken = await GetAccessTokenAsync(request.Email, request.Password);

    return accessToken;
  }
}

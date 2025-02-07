namespace ScreenDrafts.Modules.Users.IntegrationTests.Users;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2234:Pass system uri objects instead of strings", Justification = "Reviewed")]
public class GetUserProfileTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
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
    var request = new RegisterUserRequest(
      email,
      password,
      Faker.Name.FirstName(),
      Faker.Name.LastName());


    await HttpClient.PostAsJsonAsync("users/register", request);

    string accessToken = await GetAccessTokenAsync(request.Email, request.Password);

    return accessToken;
  }
}

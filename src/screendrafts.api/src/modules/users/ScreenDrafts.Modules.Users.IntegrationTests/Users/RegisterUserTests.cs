namespace ScreenDrafts.Modules.Users.IntegrationTests.Users;
public class RegisterUserTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
  public static readonly TheoryData<string, string, string, string> InvalidRequests = new()
    {
        { string.Empty, Faker.Internet.Password(), Faker.Name.FirstName(), Faker.Name.LastName() },
        { Faker.Internet.Email(), string.Empty, Faker.Name.FirstName(), Faker.Name.LastName() },
        { Faker.Internet.Email(), "12345", Faker.Name.FirstName(), Faker.Name.LastName() },
        { Faker.Internet.Email(), Faker.Internet.Password(), string.Empty, Faker.Name.LastName() },
        { Faker.Internet.Email(), Faker.Internet.Password(), Faker.Name.FirstName(), string.Empty }
    };


  [Theory]
  [MemberData(nameof(InvalidRequests))]
  public async Task Should_ReturnBadRequest_WhenRequestIsNotValidAsync(
      string email,
      string password,
      string firstName,
      string lastName)
  {
    // Arrange
    var request = new RegisterUserRequest(
      email,
      password,
      firstName,
      lastName);


    // Act
    HttpResponseMessage response = await HttpClient.PostAsJsonAsync("users/register", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Should_ReturnOk_WhenRequestIsValidAsync()
  {
    // Arrange
    var request = new RegisterUserRequest(
      "create@test.com",
      Faker.Internet.Password(),
      Faker.Name.FirstName(),
      Faker.Name.LastName());

    // Act
    HttpResponseMessage response = await HttpClient.PostAsJsonAsync("users/register", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
  }

  [Fact]
  public async Task Should_ReturnAccessToken_WhenUserIsRegisteredAsync()
  {
    // Arrange
    var request = new RegisterUserRequest(
      "token@test.com",
      Faker.Internet.Password(),
      Faker.Name.FirstName(),
      Faker.Name.LastName());

    await HttpClient.PostAsJsonAsync("users/register", request);

    // Act
    string accessToken = await GetAccessTokenAsync(request.Email, request.Password);

    // Assert
    accessToken.Should().NotBeEmpty();
  }
}

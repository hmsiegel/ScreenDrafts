namespace ScreenDrafts.Modules.Users.IntegrationTests.Users.EmailChange;

public class GenerateEmailBootstrapTokensTests(UsersIntegrationTestWebAppFactory factory)
  : UsersIntegrationTest(factory)
{
  [Fact]
  public async Task Handle_ShouldTargetAllUnclaimedUsers_WhenUserPublicIdsOmittedAsync()
  {
    // Arrange
    var userA = await RegisterUserAsync();
    var userB = await RegisterUserAsync();

    // Act
    var result = await Sender.Send(
      new GenerateEmailBootstrapTokensCommand(),
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result
      .Value.Select(r => r.UserPublicId)
      .Should()
      .BeEquivalentTo(userA.PublicId, userB.PublicId);
  }

  [Fact]
  public async Task Handle_ShouldTargetOnlyGivenList_WhenUserPublicIdsProvidedAsync()
  {
    // Arrange
    var userA = await RegisterUserAsync();
    await RegisterUserAsync();

    // Act
    var result = await Sender.Send(
      new GenerateEmailBootstrapTokensCommand { UserPublicIds = [userA.PublicId] },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().ContainSingle(r => r.UserPublicId == userA.PublicId);
  }

  [Fact]
  public async Task Handle_ShouldFail_WhenUserPublicIdIsUnknownAsync()
  {
    // Arrange
    const string unknownPublicId = "u_doesnotexist12345";

    // Act
    var result = await Sender.Send(
      new GenerateEmailBootstrapTokensCommand { UserPublicIds = [unknownPublicId] },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(UserErrors.PublicIdNotFound(unknownPublicId));
  }

  [Fact]
  public async Task Handle_ShouldSkipAlreadyClaimedUser_WithoutErroringBatchAsync()
  {
    // Arrange
    var claimedUser = await RegisterUserAsync();
    var unclaimedUser = await RegisterUserAsync();

    await ClaimBootstrapDirectlyAsync(claimedUser);

    // Act
    var result = await Sender.Send(
      new GenerateEmailBootstrapTokensCommand
      {
        UserPublicIds = [claimedUser.PublicId, unclaimedUser.PublicId],
      },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().ContainSingle(r => r.UserPublicId == unclaimedUser.PublicId);
    result.Value.Should().NotContain(r => r.UserPublicId == claimedUser.PublicId);
  }

  [Fact]
  public async Task Handle_ShouldReissueFreshTokenAndExpiry_ForUserWithPriorUnclaimedRowAsync()
  {
    // Arrange
    var user = await RegisterUserAsync();

    var firstResult = await Sender.Send(
      new GenerateEmailBootstrapTokensCommand { UserPublicIds = [user.PublicId], BatchLabel = "first" },
      TestContext.Current.CancellationToken
    );
    var firstToken = firstResult.Value.Single().Token;
    var firstExpiresAt = firstResult.Value.Single().ExpiresAt;

    // Act
    var secondResult = await Sender.Send(
      new GenerateEmailBootstrapTokensCommand
      {
        UserPublicIds = [user.PublicId],
        BatchLabel = "second",
        ExpiryHours = 200,
      },
      TestContext.Current.CancellationToken
    );

    // Assert
    secondResult.IsSuccess.Should().BeTrue();
    var secondToken = secondResult.Value.Single();
    secondToken.Token.Should().NotBe(firstToken);
    secondToken.ExpiresAt.Should().BeAfter(firstExpiresAt);

    var rowCount = await DbContext.EmailBootstrapClaims.CountAsync(
      c => c.UserId == UserId.Create(user.UserId),
      TestContext.Current.CancellationToken
    );
    rowCount.Should().Be(1);
  }

  [Fact]
  public async Task Handle_ShouldReflectIsPatreonTrue_WhenUserHasPatreonRoleAsync()
  {
    // Arrange
    var user = await RegisterUserAsync();
    await GrantRoleAsync(user.PublicId, "Patreon");

    // Act
    var result = await Sender.Send(
      new GenerateEmailBootstrapTokensCommand { UserPublicIds = [user.PublicId] },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Single().IsPatreon.Should().BeTrue();
  }

  [Fact]
  public async Task Handle_ShouldReflectIsPatreonFalse_WhenUserHasNoPatreonRoleAsync()
  {
    // Arrange
    var user = await RegisterUserAsync();
    await GrantRoleAsync(user.PublicId, "Member");

    // Act
    var result = await Sender.Send(
      new GenerateEmailBootstrapTokensCommand { UserPublicIds = [user.PublicId] },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Single().IsPatreon.Should().BeFalse();
  }

  [Fact]
  public async Task Handle_ShouldPersistOneClaimRowPerTargetedUser_WithGivenBatchLabelAsync()
  {
    // Arrange
    var user = await RegisterUserAsync();

    // Act
    var result = await Sender.Send(
      new GenerateEmailBootstrapTokensCommand
      {
        UserPublicIds = [user.PublicId],
        BatchLabel = "patreon-2026-09",
      },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();

    var claim = await DbContext.EmailBootstrapClaims.SingleAsync(
      c => c.UserId == UserId.Create(user.UserId),
      TestContext.Current.CancellationToken
    );
    claim.BatchLabel.Should().Be("patreon-2026-09");
  }

  [Fact]
  public async Task Generate_ShouldReturnUnauthorized_WhenAccessTokenNotProvidedAsync()
  {
    // Act
    var response = await HttpClient.PostAsJsonAsync(
      "users/email-change/bootstrap/generate",
      new { },
      TestContext.Current.CancellationToken
    );

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task Generate_ShouldReturnForbidden_WhenCallerLacksUsersTokensPermissionAsync()
  {
    // Arrange
    var email = Faker.Internet.Email();
    const string password = "Test@123456";
    var registerResponse = await HttpClient.PostAsJsonAsync(
      "users/register",
      new
      {
        Email = email,
        Password = password,
        FirstName = Faker.Name.FirstName(),
        LastName = Faker.Name.LastName(),
      },
      TestContext.Current.CancellationToken
    );
    registerResponse.EnsureSuccessStatusCode();

    var accessToken = await GetAccessTokenAsync(email, password);
    HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
      JwtBearerDefaults.AuthenticationScheme,
      accessToken
    );

    // Act
    var response = await HttpClient.PostAsJsonAsync(
      "users/email-change/bootstrap/generate",
      new { },
      TestContext.Current.CancellationToken
    );

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
  }

  private async Task ClaimBootstrapDirectlyAsync(GetByUserIdResponse user)
  {
    var claim = EmailBootstrapClaim.Issue(
      UserId.Create(user.UserId),
      DateTimeOffset.UtcNow,
      DateTimeOffset.UtcNow.AddHours(72),
      null
    );
    claim.Claim(Faker.Internet.Email(), DateTimeOffset.UtcNow);

    DbContext.EmailBootstrapClaims.Add(claim);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
  }
}

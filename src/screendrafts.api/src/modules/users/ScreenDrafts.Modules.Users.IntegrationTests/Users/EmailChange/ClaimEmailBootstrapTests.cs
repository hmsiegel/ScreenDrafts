namespace ScreenDrafts.Modules.Users.IntegrationTests.Users.EmailChange;

// NOTE: this module's integration tests run against a REAL Testcontainers Keycloak
// (see UsersIntegrationTestWebAppFactory) rather than a test double for
// IIdentityProviderService -- there is no seam to swap in a fake IdP here, matching
// how ChangePasswordTests/RegisterUserTests already exercise the real Keycloak
// pipeline. The test realm (screendrafts-realm-export.json) has no SMTP server
// configured, so Keycloak's "send actions email" call inside
// TriggerPasswordResetAsync cannot actually succeed in this environment -- which
// means the happy-path test below is, in practice, already exercising the
// "password-reset trigger fails but the claim still succeeds" branch that
// ClaimEmailBootstrapCommandHandler deliberately tolerates. There's no reliable way
// to additionally construct a "reset succeeds" variant without real SMTP
// infrastructure, so that half of the scenario isn't separately testable here.
public class ClaimEmailBootstrapTests(UsersIntegrationTestWebAppFactory factory)
  : UsersIntegrationTest(factory)
{
  private const string Password = "Test@123456";

  [Fact]
  public async Task Handle_ShouldSucceed_AndUpdateKeycloakAndLocalUser_EvenIfPasswordResetTriggerFailsAsync()
  {
    // Arrange
    var user = await RegisterUserAsync(password: Password);
    var token = await GenerateBootstrapTokenAsync(user.PublicId);
    var newEmail = Faker.Internet.Email();

    // Act
    var result = await Sender.Send(
      new ClaimEmailBootstrapCommand { Token = token, NewEmail = newEmail },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();

    var claim = await DbContext.EmailBootstrapClaims.SingleAsync(
      c => c.UserId == UserId.Create(user.UserId),
      TestContext.Current.CancellationToken
    );
    claim.IsClaimed.Should().BeTrue();
    claim.ClaimedEmail.Should().Be(newEmail);
    claim.ClaimedAt.Should().NotBeNull();

    var updatedUser = await DbContext.Users.SingleAsync(
      u => u.Id == UserId.Create(user.UserId),
      TestContext.Current.CancellationToken
    );
    updatedUser.Email.Value.Should().Be(newEmail);

    // Proves Keycloak's email was actually updated -- logging in by the new address works.
    var accessToken = await GetAccessTokenAsync(newEmail, Password);
    accessToken.Should().NotBeNullOrWhiteSpace();
  }

  [Fact]
  public async Task Handle_ShouldFail_WhenNewEmailAlreadyInUseByAnotherUser_AndMutateNothingAsync()
  {
    // Arrange
    var existingUser = await RegisterUserAsync(password: Password);
    var claimingUser = await RegisterUserAsync(password: Password);
    var token = await GenerateBootstrapTokenAsync(claimingUser.PublicId);

    // Act
    var result = await Sender.Send(
      new ClaimEmailBootstrapCommand { Token = token, NewEmail = existingUser.Email },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(UserErrors.EmailInUse);

    var claim = await DbContext.EmailBootstrapClaims.SingleAsync(
      c => c.UserId == UserId.Create(claimingUser.UserId),
      TestContext.Current.CancellationToken
    );
    claim.IsClaimed.Should().BeFalse();

    // Keycloak was never called -- the claiming user can still log in with their old,
    // fake bootstrap email/password (proving it wasn't touched).
    var accessToken = await GetAccessTokenAsync(claimingUser.Email, Password);
    accessToken.Should().NotBeNullOrWhiteSpace();
  }

  [Fact]
  public async Task Handle_ShouldFail_WhenTokenAlreadyClaimedAsync()
  {
    // Arrange
    var user = await RegisterUserAsync(password: Password);
    var token = await GenerateBootstrapTokenAsync(user.PublicId);

    var firstClaim = await Sender.Send(
      new ClaimEmailBootstrapCommand { Token = token, NewEmail = Faker.Internet.Email() },
      TestContext.Current.CancellationToken
    );
    firstClaim.IsSuccess.Should().BeTrue();

    // Act -- reusing the exact same (now-claimed) token
    var result = await Sender.Send(
      new ClaimEmailBootstrapCommand { Token = token, NewEmail = Faker.Internet.Email() },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(EmailBootstrapClaimErrors.AlreadyClaimed);
  }

  [Fact]
  public async Task Handle_ShouldFail_WhenTokenExpiredAsync()
  {
    // Arrange
    var user = await RegisterUserAsync(password: Password);
    var userId = UserId.Create(user.UserId);
    var tokenService = GetService<IEmailBootstrapTokenService>();
    var token = tokenService.GenerateToken(userId, DateTimeOffset.UtcNow.AddHours(72));

    DbContext.EmailBootstrapClaims.Add(
      EmailBootstrapClaim.Issue(
        userId,
        DateTimeOffset.UtcNow.AddHours(-100),
        DateTimeOffset.UtcNow.AddHours(-1),
        null
      )
    );
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Act
    var result = await Sender.Send(
      new ClaimEmailBootstrapCommand { Token = token, NewEmail = Faker.Internet.Email() },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(EmailBootstrapClaimErrors.Expired);
  }

  private async Task<string> GenerateBootstrapTokenAsync(string publicId)
  {
    var result = await Sender.Send(
      new GenerateEmailBootstrapTokensCommand { UserPublicIds = [publicId] },
      TestContext.Current.CancellationToken
    );

    return result.Value.Single().Token;
  }
}

namespace ScreenDrafts.Modules.Users.IntegrationTests.Users.EmailChange;

public class ValidateEmailBootstrapTokenTests(UsersIntegrationTestWebAppFactory factory)
  : UsersIntegrationTest(factory)
{
  [Fact]
  public async Task Handle_ShouldSucceed_WhenTokenIsValidUnclaimedAndUnexpiredAsync()
  {
    // Arrange
    var user = await RegisterUserAsync();
    var token = await GenerateBootstrapTokenAsync(user.PublicId);

    // Act
    var result = await Sender.Send(
      new ValidateEmailBootstrapTokenQuery { Token = token },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.IsValid.Should().BeTrue();
    result.Value.UserPublicId.Should().Be(user.PublicId);
  }

  [Fact]
  public async Task Handle_ShouldFail_WhenNoClaimRowExistsForUserAsync()
  {
    // Arrange -- a signed token for a user who was never issued a claim row
    var tokenService = GetService<IEmailBootstrapTokenService>();
    var userId = UserId.CreateUnique();
    var token = tokenService.GenerateToken(userId, DateTimeOffset.UtcNow.AddHours(72));

    // Act
    var result = await Sender.Send(
      new ValidateEmailBootstrapTokenQuery { Token = token },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(EmailBootstrapClaimErrors.NotFound(userId.Value));
  }

  [Fact]
  public async Task Handle_ShouldFail_WhenAlreadyClaimedAsync()
  {
    // Arrange
    var user = await RegisterUserAsync();
    var token = await GenerateBootstrapTokenAsync(user.PublicId);

    var claim = await DbContext.EmailBootstrapClaims.SingleAsync(
      c => c.UserId == UserId.Create(user.UserId),
      TestContext.Current.CancellationToken
    );
    claim.Claim(Faker.Internet.Email(), DateTimeOffset.UtcNow);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Act
    var result = await Sender.Send(
      new ValidateEmailBootstrapTokenQuery { Token = token },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(EmailBootstrapClaimErrors.AlreadyClaimed);
  }

  [Fact]
  public async Task Handle_ShouldFail_WhenClaimRowIsPastExpiresAtAsync()
  {
    // Arrange -- the token's own embedded expiry is still valid, but the DB row has expired
    // (e.g. the claim table's ExpiresAt was set independently of the token payload).
    var user = await RegisterUserAsync();
    var tokenService = GetService<IEmailBootstrapTokenService>();
    var userId = UserId.Create(user.UserId);
    var futureExpiry = DateTimeOffset.UtcNow.AddHours(72);
    var token = tokenService.GenerateToken(userId, futureExpiry);

    var claim = EmailBootstrapClaim.Issue(
      userId,
      DateTimeOffset.UtcNow.AddHours(-100),
      DateTimeOffset.UtcNow.AddHours(-1),
      null
    );
    DbContext.EmailBootstrapClaims.Add(claim);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Act
    var result = await Sender.Send(
      new ValidateEmailBootstrapTokenQuery { Token = token },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(EmailBootstrapClaimErrors.Expired);
  }

  [Fact]
  public async Task Handle_ShouldFail_WhenTokenItselfHasExpiredAsync()
  {
    // Arrange -- the token payload's own embedded expiry is in the past; the signature
    // check happens inside IEmailBootstrapTokenService.ValidateToken before any DB lookup.
    var user = await RegisterUserAsync();
    var tokenService = GetService<IEmailBootstrapTokenService>();
    var userId = UserId.Create(user.UserId);
    var token = tokenService.GenerateToken(userId, DateTimeOffset.UtcNow.AddHours(-1));

    DbContext.EmailBootstrapClaims.Add(
      EmailBootstrapClaim.Issue(userId, DateTimeOffset.UtcNow.AddHours(-2), DateTimeOffset.UtcNow.AddHours(-1), null)
    );
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Act
    var result = await Sender.Send(
      new ValidateEmailBootstrapTokenQuery { Token = token },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(EmailBootstrapClaimErrors.Expired);
  }

  [Theory]
  [InlineData("")]
  [InlineData("not-a-real-token")]
  [InlineData("tampered.tampered")]
  public async Task Handle_ShouldFail_WhenTokenIsMalformedOrTamperedAsync(string malformedToken)
  {
    // Act
    var result = await Sender.Send(
      new ValidateEmailBootstrapTokenQuery { Token = malformedToken },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(EmailBootstrapClaimErrors.InvalidToken);
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

namespace ScreenDrafts.Modules.Users.IntegrationTests.Users.EmailChange;

// NOTE: RequestEmailChangeCommandHandler only ever persists the SHA-256 HASH of the
// raw confirmation token -- the raw value lives solely in the confirmation-link
// query string embedded in UserEmailChangeRequestedDomainEvent, which this test host
// can't observe: ALL integration tests in this repo run with IEventBus replaced by a
// NoOpEventBus (see the shared IntegrationTestWebAppFactory), so nothing captures
// domain-event payloads on the way out. There is no seam to recover a real raw token
// produced by the Request handler, so these tests issue EmailChangeToken rows
// directly (mirroring the handler's own hashing: SHA-256 hex of the raw value) to
// exercise Confirm in isolation.
public class ConfirmEmailChangeTests(UsersIntegrationTestWebAppFactory factory)
  : UsersIntegrationTest(factory)
{
  private const string Password = "Test@123456";

  [Fact]
  public async Task Handle_ShouldSucceed_AndUpdateKeycloakAndLocalUserAsync()
  {
    // Arrange
    var user = await RegisterUserAsync(password: Password);
    var newEmail = Faker.Internet.Email();
    var rawToken = await IssueTokenAsync(user, newEmail);

    // Act
    var result = await Sender.Send(
      new ConfirmEmailChangeCommand { Token = rawToken },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();

    var token = await DbContext.EmailChangeTokens.SingleAsync(
      t => t.UserId == UserId.Create(user.UserId),
      TestContext.Current.CancellationToken
    );
    token.IsUsed.Should().BeTrue();

    var updatedUser = await DbContext.Users.SingleAsync(
      u => u.Id == UserId.Create(user.UserId),
      TestContext.Current.CancellationToken
    );
    updatedUser.Email.Value.Should().Be(newEmail);

    var accessToken = await GetAccessTokenAsync(newEmail, Password);
    accessToken.Should().NotBeNullOrWhiteSpace();
  }

  [Fact]
  public async Task Handle_ShouldFail_WhenTokenIsUnknownAsync()
  {
    // Act
    var result = await Sender.Send(
      new ConfirmEmailChangeCommand { Token = "unknown-token-value" },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(EmailChangeErrors.InvalidToken);
  }

  [Fact]
  public async Task Handle_ShouldFail_WhenTokenAlreadyUsedAsync()
  {
    // Arrange
    var user = await RegisterUserAsync(password: Password);
    var rawToken = await IssueTokenAsync(user, Faker.Internet.Email());

    var firstConfirm = await Sender.Send(
      new ConfirmEmailChangeCommand { Token = rawToken },
      TestContext.Current.CancellationToken
    );
    firstConfirm.IsSuccess.Should().BeTrue();

    // Act
    var result = await Sender.Send(
      new ConfirmEmailChangeCommand { Token = rawToken },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(EmailChangeErrors.AlreadyUsed);
  }

  [Fact]
  public async Task Handle_ShouldFail_WhenTokenExpiredAsync()
  {
    // Arrange
    var user = await RegisterUserAsync(password: Password);
    var rawToken = await IssueTokenAsync(
      user,
      Faker.Internet.Email(),
      issuedAt: DateTimeOffset.UtcNow.AddHours(-2),
      expiresAt: DateTimeOffset.UtcNow.AddHours(-1)
    );

    // Act
    var result = await Sender.Send(
      new ConfirmEmailChangeCommand { Token = rawToken },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(EmailChangeErrors.Expired);
  }

  [Fact]
  public async Task Handle_ShouldFail_AndNotMarkTokenUsed_WhenEmailClaimedByAnotherUserBetweenRequestAndConfirmAsync()
  {
    // Arrange
    var user = await RegisterUserAsync(password: Password);
    var contestedEmail = Faker.Internet.Email();
    var rawToken = await IssueTokenAsync(user, contestedEmail);

    // Someone else takes the contested email in the meantime.
    await RegisterUserAsync(email: contestedEmail);

    // Act
    var result = await Sender.Send(
      new ConfirmEmailChangeCommand { Token = rawToken },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(UserErrors.EmailInUse);

    var token = await DbContext.EmailChangeTokens.SingleAsync(
      t => t.UserId == UserId.Create(user.UserId),
      TestContext.Current.CancellationToken
    );
    token.IsUsed.Should().BeFalse();
  }

  private async Task<string> IssueTokenAsync(
    GetByUserIdResponse user,
    string newEmail,
    DateTimeOffset? issuedAt = null,
    DateTimeOffset? expiresAt = null
  )
  {
    var rawToken = Convert
      .ToBase64String(RandomNumberGenerator.GetBytes(32))
      .TrimEnd('=')
      .Replace('+', '-')
      .Replace('/', '_');
    var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

    var token = EmailChangeToken.Issue(
      UserId.Create(user.UserId),
      newEmail,
      tokenHash,
      issuedAt ?? DateTimeOffset.UtcNow,
      expiresAt ?? DateTimeOffset.UtcNow.AddHours(1)
    );

    DbContext.EmailChangeTokens.Add(token);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    return rawToken;
  }
}

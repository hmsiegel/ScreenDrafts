namespace ScreenDrafts.Modules.Users.IntegrationTests.Users.EmailChange;

public class RequestEmailChangeTests(UsersIntegrationTestWebAppFactory factory)
  : UsersIntegrationTest(factory)
{
  [Fact]
  public async Task Handle_ShouldCreateActiveToken_ForNewEmailAsync()
  {
    // Arrange
    var user = await RegisterUserAsync();
    var newEmail = Faker.Internet.Email();

    // Act
    var result = await Sender.Send(
      new RequestEmailChangeCommand { PublicId = user.PublicId, NewEmail = newEmail },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();

    var token = await DbContext.EmailChangeTokens.SingleAsync(
      t => t.UserId == UserId.Create(user.UserId),
      TestContext.Current.CancellationToken
    );
    token.NewEmail.Should().Be(newEmail);
    token.IsUsed.Should().BeFalse();
    token.ExpiresAt.Should().BeAfter(token.IssuedAt);
  }

  [Fact]
  public async Task Handle_ShouldFail_WhenRequestingSameEmailAsCurrentAsync()
  {
    // Arrange
    var user = await RegisterUserAsync();

    // Act
    var result = await Sender.Send(
      new RequestEmailChangeCommand { PublicId = user.PublicId, NewEmail = user.Email },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(EmailChangeErrors.SameAsCurrentEmail);
  }

  [Fact]
  public async Task Handle_ShouldFail_WhenRequestedEmailAlreadyInUseByAnotherUserAsync()
  {
    // Arrange
    var otherUser = await RegisterUserAsync();
    var requestingUser = await RegisterUserAsync();

    // Act
    var result = await Sender.Send(
      new RequestEmailChangeCommand { PublicId = requestingUser.PublicId, NewEmail = otherUser.Email },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(UserErrors.EmailInUse);
  }

  [Fact]
  public async Task Handle_ShouldInvalidateOldToken_AndOnlyNewOneValidates_OnSecondRequestAsync()
  {
    // Arrange
    var user = await RegisterUserAsync();
    var firstEmail = Faker.Internet.Email();
    var secondEmail = Faker.Internet.Email();

    var firstRequest = await Sender.Send(
      new RequestEmailChangeCommand { PublicId = user.PublicId, NewEmail = firstEmail },
      TestContext.Current.CancellationToken
    );
    firstRequest.IsSuccess.Should().BeTrue();

    var firstToken = await DbContext.EmailChangeTokens.SingleAsync(
      t => t.NewEmail == firstEmail,
      TestContext.Current.CancellationToken
    );

    // Act
    var secondRequest = await Sender.Send(
      new RequestEmailChangeCommand { PublicId = user.PublicId, NewEmail = secondEmail },
      TestContext.Current.CancellationToken
    );

    // Assert
    secondRequest.IsSuccess.Should().BeTrue();

    var reloadedFirstToken = await DbContext.EmailChangeTokens.SingleAsync(
      t => t.Id == firstToken.Id,
      TestContext.Current.CancellationToken
    );
    reloadedFirstToken.IsUsed.Should().BeTrue();

    var activeToken = await DbContext.EmailChangeTokens.SingleAsync(
      t => t.NewEmail == secondEmail,
      TestContext.Current.CancellationToken
    );
    activeToken.IsUsed.Should().BeFalse();
  }

  [Fact]
  public async Task RequestEmailChange_ShouldReturnUnauthorized_WhenAccessTokenNotProvidedAsync()
  {
    // Act
    var response = await HttpClient.PostAsJsonAsync(
      "users/email-change/request",
      new { NewEmail = Faker.Internet.Email() },
      TestContext.Current.CancellationToken
    );

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }
}

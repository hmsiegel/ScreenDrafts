namespace ScreenDrafts.Modules.Users.IntegrationTests.Users;

public class GetUsersByIdsTests(UsersIntegrationTestWebAppFactory factory)
  : UsersIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnAllMatchingUsers_WhenUserIdsExistAsync()
  {
    // Arrange
    var userId1 = await RegisterUserAsync();
    var userId2 = await RegisterUserAsync();

    // Act
    var result = await Sender.Send(
      new GetUsersByIdsQuery([userId1, userId2]),
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Users.Should().HaveCount(2);
    result.Value.Users.Select(u => u.UserId).Should().BeEquivalentTo([userId1, userId2]);
  }

  [Fact]
  public async Task Should_OmitNonExistentIds_WithoutErrorAsync()
  {
    // Arrange
    var userId = await RegisterUserAsync();
    var nonExistentUserId = Guid.NewGuid();

    // Act
    var result = await Sender.Send(
      new GetUsersByIdsQuery([userId, nonExistentUserId]),
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Users.Should().ContainSingle();
    result.Value.Users[0].UserId.Should().Be(userId);
  }

  [Fact]
  public async Task Should_ReturnEmptyResult_WhenInputListIsEmptyAsync()
  {
    // Act
    var result = await Sender.Send(
      new GetUsersByIdsQuery([]),
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Users.Should().BeEmpty();
  }

  [Fact]
  public async Task Should_NotReturnDuplicateRows_WhenInputContainsDuplicateIdsAsync()
  {
    // Arrange
    var userId = await RegisterUserAsync();

    // Act
    var result = await Sender.Send(
      new GetUsersByIdsQuery([userId, userId]),
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Users.Should().ContainSingle();
  }

  private async Task<Guid> RegisterUserAsync()
  {
    var result = await Sender.Send(
      new RegisterUserCommand
      {
        Email = Faker.Internet.Email(),
        Password = Faker.Internet.Password(),
        FirstName = Faker.Name.FirstName(),
        LastName = Faker.Name.LastName(),
      },
      TestContext.Current.CancellationToken
    );

    return result.Value;
  }
}

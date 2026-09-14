namespace ScreenDrafts.Modules.Users.UnitTests.EmailChange;

public class EmailChangeTokenTests : BaseTest
{
  private static readonly DateTimeOffset IssuedAt = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
  private static readonly DateTimeOffset ExpiresAt = IssuedAt.AddHours(1);

  [Fact]
  public void Issue_ShouldCreateToken_WithGivenValues()
  {
    // Arrange
    var userId = UserId.CreateUnique();
    var newEmail = Faker.Internet.Email();

    // Act
    var token = EmailChangeToken.Issue(userId, newEmail, "token-hash", IssuedAt, ExpiresAt);

    // Assert
    token.UserId.Should().Be(userId);
    token.NewEmail.Should().Be(newEmail);
    token.TokenHash.Should().Be("token-hash");
    token.IssuedAt.Should().Be(IssuedAt);
    token.ExpiresAt.Should().Be(ExpiresAt);
    token.IsUsed.Should().BeFalse();
  }

  [Fact]
  public void MarkUsed_WhenNotUsed_ShouldSucceedAndSetUsedAt()
  {
    // Arrange
    var token = EmailChangeToken.Issue(
      UserId.CreateUnique(),
      Faker.Internet.Email(),
      "token-hash",
      IssuedAt,
      ExpiresAt
    );
    var usedAt = IssuedAt.AddMinutes(5);

    // Act
    var result = token.MarkUsed(usedAt);

    // Assert
    result.IsSuccess.Should().BeTrue();
    token.IsUsed.Should().BeTrue();
    token.UsedAt.Should().Be(usedAt);
  }

  [Fact]
  public void MarkUsed_WhenAlreadyUsed_ShouldFail()
  {
    // Arrange
    var token = EmailChangeToken.Issue(
      UserId.CreateUnique(),
      Faker.Internet.Email(),
      "token-hash",
      IssuedAt,
      ExpiresAt
    );
    token.MarkUsed(IssuedAt.AddMinutes(5));

    // Act
    var result = token.MarkUsed(IssuedAt.AddMinutes(10));

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(EmailChangeErrors.AlreadyUsed);
  }

  [Fact]
  public void Invalidate_WhenNotUsed_ShouldSetUsedAt()
  {
    // Arrange
    var token = EmailChangeToken.Issue(
      UserId.CreateUnique(),
      Faker.Internet.Email(),
      "token-hash",
      IssuedAt,
      ExpiresAt
    );
    var invalidatedAt = IssuedAt.AddMinutes(5);

    // Act
    token.Invalidate(invalidatedAt);

    // Assert
    token.IsUsed.Should().BeTrue();
    token.UsedAt.Should().Be(invalidatedAt);
  }

  [Fact]
  public void Invalidate_WhenAlreadyUsed_ShouldBeNoOp()
  {
    // Arrange
    var token = EmailChangeToken.Issue(
      UserId.CreateUnique(),
      Faker.Internet.Email(),
      "token-hash",
      IssuedAt,
      ExpiresAt
    );
    var originalUsedAt = IssuedAt.AddMinutes(5);
    token.MarkUsed(originalUsedAt);

    // Act -- a later request superseding this one shouldn't overwrite the original used timestamp
    token.Invalidate(IssuedAt.AddMinutes(10));

    // Assert
    token.UsedAt.Should().Be(originalUsedAt);
  }
}

namespace ScreenDrafts.Modules.Users.UnitTests.Bootstraps;

public class EmailBootstrapClaimTests : BaseTest
{
  private static readonly DateTimeOffset IssuedAt = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
  private static readonly DateTimeOffset ExpiresAt = IssuedAt.AddHours(72);

  [Fact]
  public void Issue_ShouldCreateClaim_WithGivenValues()
  {
    // Arrange
    var userId = UserId.CreateUnique();

    // Act
    var claim = EmailBootstrapClaim.Issue(userId, IssuedAt, ExpiresAt, "patreon-2026-09");

    // Assert
    claim.UserId.Should().Be(userId);
    claim.IssuedAt.Should().Be(IssuedAt);
    claim.ExpiresAt.Should().Be(ExpiresAt);
    claim.BatchLabel.Should().Be("patreon-2026-09");
    claim.IsClaimed.Should().BeFalse();
  }

  [Fact]
  public void Reissue_WhenNotClaimed_ShouldUpdateFieldsAndSucceed()
  {
    // Arrange
    var claim = EmailBootstrapClaim.Issue(UserId.CreateUnique(), IssuedAt, ExpiresAt, "patreon-2026-09");
    var newIssuedAt = IssuedAt.AddDays(30);
    var newExpiresAt = newIssuedAt.AddHours(72);

    // Act
    var result = claim.Reissue(newIssuedAt, newExpiresAt, "discord-2026-10");

    // Assert
    result.IsSuccess.Should().BeTrue();
    claim.IssuedAt.Should().Be(newIssuedAt);
    claim.ExpiresAt.Should().Be(newExpiresAt);
    claim.BatchLabel.Should().Be("discord-2026-10");
  }

  [Fact]
  public void Reissue_WhenAlreadyClaimed_ShouldFail()
  {
    // Arrange
    var claim = EmailBootstrapClaim.Issue(UserId.CreateUnique(), IssuedAt, ExpiresAt, null);
    claim.Claim(Faker.Internet.Email(), IssuedAt.AddHours(1));

    // Act
    var result = claim.Reissue(IssuedAt.AddDays(30), IssuedAt.AddDays(30).AddHours(72), null);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(EmailBootstrapClaimErrors.CannotReissueClaimed);
  }

  [Fact]
  public void Claim_WhenNotClaimedAndNotExpired_ShouldSucceedAndSetFields()
  {
    // Arrange
    var claim = EmailBootstrapClaim.Issue(UserId.CreateUnique(), IssuedAt, ExpiresAt, null);
    var claimedEmail = Faker.Internet.Email();
    var claimedAt = IssuedAt.AddHours(1);

    // Act
    var result = claim.Claim(claimedEmail, claimedAt);

    // Assert
    result.IsSuccess.Should().BeTrue();
    claim.IsClaimed.Should().BeTrue();
    claim.ClaimedEmail.Should().Be(claimedEmail);
    claim.ClaimedAt.Should().Be(claimedAt);
  }

  [Fact]
  public void Claim_WhenAlreadyClaimed_ShouldFail()
  {
    // Arrange
    var claim = EmailBootstrapClaim.Issue(UserId.CreateUnique(), IssuedAt, ExpiresAt, null);
    claim.Claim(Faker.Internet.Email(), IssuedAt.AddHours(1));

    // Act
    var result = claim.Claim(Faker.Internet.Email(), IssuedAt.AddHours(2));

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(EmailBootstrapClaimErrors.AlreadyClaimed);
  }

  [Fact]
  public void Claim_WhenPastExpiresAt_ShouldFail()
  {
    // Arrange
    var claim = EmailBootstrapClaim.Issue(UserId.CreateUnique(), IssuedAt, ExpiresAt, null);

    // Act
    var result = claim.Claim(Faker.Internet.Email(), ExpiresAt.AddSeconds(1));

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(EmailBootstrapClaimErrors.Expired);
    claim.IsClaimed.Should().BeFalse();
  }
}

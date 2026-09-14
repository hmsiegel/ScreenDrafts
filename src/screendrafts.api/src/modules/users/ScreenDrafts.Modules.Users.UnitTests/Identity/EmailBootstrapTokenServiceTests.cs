namespace ScreenDrafts.Modules.Users.UnitTests.Identity;

public class EmailBootstrapTokenServiceTests : BaseTest
{
  private static EmailBootstrapTokenService CreateService(string secret = "unit-test-secret-please-change") =>
    new(Options.Create(new EmailBootstrapOptions { Secret = secret, DefaultExpiryHours = 72 }));

  [Fact]
  public void GenerateToken_ThenValidateToken_ShouldRoundTrip_ReturningUserIdAndExpiresAt()
  {
    // Arrange
    var service = CreateService();
    var userId = UserId.CreateUnique();
    var expiresAt = DateTimeOffset.UtcNow.AddHours(72);

    // Act
    var token = service.GenerateToken(userId, expiresAt);
    var result = service.ValidateToken(token);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.UserId.Should().Be(userId);
    result.Value.ExpiresAt.Should().Be(DateTimeOffset.FromUnixTimeSeconds(expiresAt.ToUnixTimeSeconds()));
  }

  [Fact]
  public void ValidateToken_WithTamperedPayload_ShouldFail()
  {
    // Arrange
    var service = CreateService();
    var token = service.GenerateToken(UserId.CreateUnique(), DateTimeOffset.UtcNow.AddHours(72));
    var parts = token.Split('.');
    var tamperedPayload = FlipLastChar(parts[0]);
    var tamperedToken = $"{tamperedPayload}.{parts[1]}";

    // Act
    var result = service.ValidateToken(tamperedToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(EmailBootstrapClaimErrors.InvalidToken);
  }

  [Fact]
  public void ValidateToken_WithTamperedSignature_ShouldFail()
  {
    // Arrange
    var service = CreateService();
    var token = service.GenerateToken(UserId.CreateUnique(), DateTimeOffset.UtcNow.AddHours(72));
    var parts = token.Split('.');
    var tamperedSignature = FlipLastChar(parts[1]);
    var tamperedToken = $"{parts[0]}.{tamperedSignature}";

    // Act
    var result = service.ValidateToken(tamperedToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(EmailBootstrapClaimErrors.InvalidToken);
  }

  [Fact]
  public void ValidateToken_WithExpiredToken_ShouldFail_WithExpiredError()
  {
    // Arrange
    var service = CreateService();
    var token = service.GenerateToken(UserId.CreateUnique(), DateTimeOffset.UtcNow.AddHours(-1));

    // Act
    var result = service.ValidateToken(token);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(EmailBootstrapClaimErrors.Expired);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData("no-dot-separator")]
  [InlineData("too.many.dots")]
  [InlineData("!!!invalid-base64!!!.!!!invalid-base64!!!")]
  public void ValidateToken_WithMalformedInput_ShouldFailCleanlyWithoutThrowing(string malformedToken)
  {
    // Arrange
    var service = CreateService();

    // Act
    var act = () => service.ValidateToken(malformedToken);

    // Assert
    act.Should().NotThrow();
    var result = act();
    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(EmailBootstrapClaimErrors.InvalidToken);
  }

  private static string FlipLastChar(string base64UrlSegment)
  {
    const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
    var lastChar = base64UrlSegment[^1];
    var replacement = alphabet.First(c => c != lastChar);

    return base64UrlSegment[..^1] + replacement;
  }
}

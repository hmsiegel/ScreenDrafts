namespace ScreenDrafts.Modules.Reporting.UnitTests.Drafts;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
  "Design",
  "CA1054:URI parameters should not be strings",
  Justification = "UpdateSpotlightCommand.SpotifyUrl is a string by contract; the validator is what parses it."
)]
public sealed class UpdateSpotlightValidatorTests
{
  private const string ValidPublicId = "spl_abcdefgh12345678";

  private static readonly Validator _validator = new();

  private static UpdateSpotlightCommand BuildCommand(
    string publicId = ValidPublicId,
    string description = "A great episode",
    string? spotifyUrl = null
  ) =>
    new()
    {
      PublicId = publicId,
      SpotlightDescription = description,
      SpotifyUrl = spotifyUrl,
    };

  // -------------------------------------------------------------------------
  // Valid
  // -------------------------------------------------------------------------

  [Fact]
  public void Validate_ShouldPass_WhenCommandIsValid()
  {
    var result = _validator.Validate(
      BuildCommand(spotifyUrl: "https://open.spotify.com/episode/abc123")
    );

    result.IsValid.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // PublicId
  // -------------------------------------------------------------------------

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void Validate_ShouldFail_WhenPublicIdIsEmpty(string publicId)
  {
    var result = _validator.Validate(BuildCommand(publicId: publicId));

    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateSpotlightCommand.PublicId));
  }

  [Theory]
  [InlineData("d_abcdefgh12345678")]
  [InlineData("dr_abcdefgh12345678")]
  [InlineData("abcdefgh12345678")]
  public void Validate_ShouldFail_WhenPublicIdHasWrongPrefix(string publicId)
  {
    var result = _validator.Validate(BuildCommand(publicId: publicId));

    result.IsValid.Should().BeFalse();
    result
      .Errors.Should()
      .Contain(e =>
        e.PropertyName == nameof(UpdateSpotlightCommand.PublicId)
        && e.ErrorMessage == "Spotlight public ID is invalid."
      );
  }

  [Fact]
  public void Validate_ShouldUseTheSpotlightPrefix()
  {
    var result = _validator.Validate(
      BuildCommand(publicId: $"{PublicIdPrefixes.Spotlight}_abcdefgh12345678")
    );

    result.IsValid.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // SpotlightDescription
  // -------------------------------------------------------------------------

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  [InlineData("   \t ")]
  public void Validate_ShouldFail_WhenDescriptionIsEmptyOrWhitespace(string description)
  {
    var result = _validator.Validate(BuildCommand(description: description));

    result.IsValid.Should().BeFalse();
    result
      .Errors.Should()
      .Contain(e => e.PropertyName == nameof(UpdateSpotlightCommand.SpotlightDescription));
  }

  [Fact]
  public void Validate_ShouldPass_WhenDescriptionIsExactlyAtTheLimit()
  {
    var result = _validator.Validate(BuildCommand(description: new string('a', 1000)));

    result.IsValid.Should().BeTrue();
  }

  [Fact]
  public void Validate_ShouldFail_WhenDescriptionIsOneOverTheLimit()
  {
    var result = _validator.Validate(BuildCommand(description: new string('a', 1001)));

    result.IsValid.Should().BeFalse();
    result
      .Errors.Should()
      .Contain(e => e.PropertyName == nameof(UpdateSpotlightCommand.SpotlightDescription));
  }

  // -------------------------------------------------------------------------
  // SpotifyUrl
  // -------------------------------------------------------------------------

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public void Validate_ShouldPass_WhenSpotifyUrlIsNullEmptyOrWhitespace(string? spotifyUrl)
  {
    var result = _validator.Validate(BuildCommand(spotifyUrl: spotifyUrl));

    result.IsValid.Should().BeTrue();
  }

  [Theory]
  [InlineData("not a url")]
  [InlineData("open.spotify.com/episode/abc")]
  [InlineData("/episode/abc")]
  [InlineData("://broken")]
  public void Validate_ShouldFail_WhenSpotifyUrlIsNotAnAbsoluteUrl(string spotifyUrl)
  {
    var result = _validator.Validate(BuildCommand(spotifyUrl: spotifyUrl));

    result.IsValid.Should().BeFalse();
    result
      .Errors.Should()
      .Contain(e => e.PropertyName == nameof(UpdateSpotlightCommand.SpotifyUrl));
  }

  [Theory]
  [InlineData("https://open.spotify.com/episode/abc123")]
  [InlineData("http://example.com/path?x=1")]
  public void Validate_ShouldPass_WhenSpotifyUrlIsAValidAbsoluteUrl(string spotifyUrl)
  {
    var result = _validator.Validate(BuildCommand(spotifyUrl: spotifyUrl));

    result.IsValid.Should().BeTrue();
  }
}

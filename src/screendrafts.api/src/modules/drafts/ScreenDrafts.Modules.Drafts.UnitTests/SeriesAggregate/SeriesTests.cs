namespace ScreenDrafts.Modules.Drafts.UnitTests.SeriesAggregate;

public class SeriesTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var name = Faker.Lorem.Word();
    var publicId = Faker.Random.AlphaNumeric(10);
    var canonicalPolicy = CanonicalPolicy.Always;
    var continuityScope = ContinuityScope.Global;
    var continuityDateRule = ContinuityDateRule.AnyChannelFirstRelease;
    var kind = SeriesKind.Regular;
    var defaultDraftType = DraftType.Standard;
    var allowedDraftTypes = DraftTypeMask.All;

    // Act
    var result = Series.Create(
      name,
      publicId,
      canonicalPolicy,
      continuityScope,
      continuityDateRule,
      kind,
      defaultDraftType,
      allowedDraftTypes);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Name.Should().Be(name);
    result.Value.PublicId.Should().Be(publicId);
    result.Value.CanonicalPolicy.Should().Be(canonicalPolicy);
    result.Value.ContinuityScope.Should().Be(continuityScope);
    result.Value.ContinuityDateRule.Should().Be(continuityDateRule);
    result.Value.Kind.Should().Be(kind);
    result.Value.DefaultDraftType.Should().Be(defaultDraftType);
    result.Value.AllowedDraftTypes.Should().Be(allowedDraftTypes);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenNameIsNullOrEmpty()
  {
    // Arrange
    var publicId = Faker.Random.AlphaNumeric(10);

    // Act
    var result = Series.Create(
      string.Empty,
      publicId,
      CanonicalPolicy.Always,
      ContinuityScope.Global,
      ContinuityDateRule.AnyChannelFirstRelease,
      SeriesKind.Regular,
      DraftType.Standard,
      DraftTypeMask.All);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(SeriesErrors.SeriesNameIsRequired);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenAllowedDraftTypesIsNone()
  {
    // Arrange
    var name = Faker.Lorem.Word();
    var publicId = Faker.Random.AlphaNumeric(10);

    // Act
    var result = Series.Create(
      name,
      publicId,
      CanonicalPolicy.Always,
      ContinuityScope.Global,
      ContinuityDateRule.AnyChannelFirstRelease,
      SeriesKind.Regular,
      DraftType.Standard,
      DraftTypeMask.None);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(SeriesErrors.AllowedDraftTypesCannotBeNone);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenDefaultDraftTypeNotInAllowedTypes()
  {
    // Arrange
    var name = Faker.Lorem.Word();
    var publicId = Faker.Random.AlphaNumeric(10);

    // Act
    var result = Series.Create(
      name,
      publicId,
      CanonicalPolicy.Always,
      ContinuityScope.Global,
      ContinuityDateRule.AnyChannelFirstRelease,
      SeriesKind.Regular,
      DraftType.Mega,
      DraftTypeMask.Standard);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(SeriesErrors.DefaultDraftTypeMustBeIncludedInAllowedDraftTypes);
  }

  [Fact]
  public void Rename_ShouldUpdateName_WhenValidNameProvided()
  {
    // Arrange
    var series = CreateSeries();
    var newName = Faker.Lorem.Word();

    // Act
    var result = series.Rename(newName);

    // Assert
    result.IsSuccess.Should().BeTrue();
    series.Name.Should().Be(newName);
  }

  [Fact]
  public void Rename_ShouldReturnFailure_WhenNameIsNullOrEmpty()
  {
    // Arrange
    var series = CreateSeries();

    // Act
    var result = series.Rename(string.Empty);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(SeriesErrors.SeriesNameIsRequired);
  }

  private static Series CreateSeries()
  {
    return Series.Create(
      name: Faker.Lorem.Word(),
      publicId: Faker.Random.AlphaNumeric(10),
      canonicalPolicy: CanonicalPolicy.Always,
      continuityScope: ContinuityScope.Global,
      continuityDateRule: ContinuityDateRule.AnyChannelFirstRelease,
      kind: SeriesKind.Regular,
      defaultDraftType: DraftType.Standard,
      allowedDraftTypes: DraftTypeMask.All).Value;
  }
}

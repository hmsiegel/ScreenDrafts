namespace ScreenDrafts.Modules.Reporting.UnitTests.Titles;

public sealed class TitleHonorificValidatorTests
{
  private static GetTitleHonorificsQuery Query(
    string level = TitleHonorificLevels.MarqueeOfFame,
    string? search = null,
    string? sort = null,
    int? page = null,
    int? pageSize = null) =>
    new()
    {
      Level = level,
      Search = search,
      Sort = sort,
      Page = page,
      PageSize = pageSize,
      IncludeAll = false,
    };

  [Fact]
  public void Validate_ShouldApplyDefaults_WhenOnlyALevelIsSupplied()
  {
    var result = TitleHonorificValidator.Validate(Query());

    result.Error.Should().BeNull();
    result.Spec!.Sort.Should().Be(TitleHonorificSorts.Newest);
    result.Spec.Page.Should().Be(1);
    result.Spec.PageSize.Should().Be(50);
    result.Spec.Search.Should().BeNull();
  }

  [Theory]
  [InlineData("marquee-of-fame", 2)]
  [InlineData("hat-trick", 3)]
  [InlineData("grand-slam", 4)]
  [InlineData("high-five", 5)]
  public void Validate_ShouldResolveEachLevelToItsMinimumAppearances(string level, int minimum)
  {
    TitleHonorificValidator.Validate(Query(level)).Spec!.Level.MinAppearances.Should().Be(minimum);
  }

  [Fact]
  public void Validate_ShouldResolveLevelCaseInsensitively()
  {
    TitleHonorificValidator.Validate(Query("HAT-TRICK")).Spec!.Level.Code.Should().Be(TitleHonorificLevels.HatTrick);
  }

  [Fact]
  public void Validate_ShouldReturnNotFound_WhenLevelIsUnknown()
  {
    var result = TitleHonorificValidator.Validate(Query("six-pack"));

    result.Spec.Should().BeNull();
    result.Error!.Type.Should().Be(ErrorType.NotFound);
    result.Error.Code.Should().Be("TitleHonorifics.UnknownLevel");
  }

  [Fact]
  public void Validate_ShouldReturnProblem_WhenSortIsUnknown()
  {
    var result = TitleHonorificValidator.Validate(Query(sort: "random"));

    result.Error!.Type.Should().Be(ErrorType.Problem);
    result.Error.Code.Should().Be("TitleHonorifics.UnknownSort");
  }

  [Theory]
  [InlineData("newest")]
  [InlineData("oldest")]
  [InlineData("alphabetical")]
  [InlineData("appearances")]
  public void Validate_ShouldAcceptEveryKnownSort(string sort)
  {
    TitleHonorificValidator.Validate(Query(sort: sort)).Spec!.Sort.Should().Be(sort);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void Validate_ShouldFallBackToNewest_WhenSortIsBlank(string sort)
  {
    TitleHonorificValidator.Validate(Query(sort: sort)).Spec!.Sort.Should().Be(TitleHonorificSorts.Newest);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-3)]
  public void Validate_ShouldReturnProblem_WhenPageIsBelowOne(int page)
  {
    TitleHonorificValidator.Validate(Query(page: page)).Error!.Code.Should().Be("TitleHonorifics.InvalidPage");
  }

  [Theory]
  [InlineData(0)]
  [InlineData(101)]
  public void Validate_ShouldReturnProblem_WhenPageSizeIsOutOfRange(int pageSize)
  {
    TitleHonorificValidator.Validate(Query(pageSize: pageSize)).Error!.Code.Should().Be("TitleHonorifics.InvalidPageSize");
  }

  [Theory]
  [InlineData(1)]
  [InlineData(100)]
  public void Validate_ShouldAcceptPageSizeBoundaries(int pageSize)
  {
    TitleHonorificValidator.Validate(Query(pageSize: pageSize)).Spec!.PageSize.Should().Be(pageSize);
  }

  [Fact]
  public void Validate_ShouldReturnProblem_WhenSearchExceedsOneHundredCharacters()
  {
    var result = TitleHonorificValidator.Validate(Query(search: new string('x', 101)));

    result.Error!.Type.Should().Be(ErrorType.Problem);
    result.Error.Code.Should().Be("TitleHonorifics.SearchTooLong");
  }

  [Fact]
  public void Validate_ShouldAcceptSearchOfExactlyOneHundredCharacters()
  {
    TitleHonorificValidator.Validate(Query(search: new string('x', 100))).Spec.Should().NotBeNull();
  }

  [Fact]
  public void Validate_ShouldThrow_WhenQueryIsNull()
  {
    FluentActions.Invoking(() => TitleHonorificValidator.Validate(null!)).Should().Throw<ArgumentNullException>();
  }
}

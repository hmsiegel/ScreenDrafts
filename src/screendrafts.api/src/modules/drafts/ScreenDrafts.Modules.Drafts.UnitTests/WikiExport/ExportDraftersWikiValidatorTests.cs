using DraftersValidator = ScreenDrafts.Modules.Drafts.Features.WikiExport.ExportDrafters.Validator;
using ExportDraftersWikiQuery = ScreenDrafts.Modules.Drafts.Features.WikiExport.ExportDrafters.ExportDraftersWikiQuery;

namespace ScreenDrafts.Modules.Drafts.UnitTests.WikiExport;

public sealed class ExportDraftersWikiValidatorTests
{
  private static readonly DraftersValidator _validator = new();

  private static string DrafterId(int n) =>
    $"{PublicIdPrefixes.Drafter}_{n.ToString("D15", System.Globalization.CultureInfo.InvariantCulture)}";

  private static ExportDraftersWikiQuery Query(IReadOnlyList<string> ids) =>
    new() { DrafterPublicIds = ids };

  private static IReadOnlyList<string> Ids(int count) =>
    [.. Enumerable.Range(1, count).Select(DrafterId)];

  [Fact]
  public void Validate_ShouldPass_ForASingleValidId() =>
    _validator.Validate(Query(Ids(1))).IsValid.Should().BeTrue();

  [Fact]
  public void Validate_ShouldPass_AtTheLimitOf50Ids() =>
    _validator.Validate(Query(Ids(50))).IsValid.Should().BeTrue();

  [Fact]
  public void Validate_ShouldFail_WhenTheListIsEmpty()
  {
    var result = _validator.Validate(Query([]));

    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.ErrorMessage == "Select at least one drafter.");
  }

  [Fact]
  public void Validate_ShouldFail_When51IdsAreSelected()
  {
    var result = _validator.Validate(Query(Ids(51)));

    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.ErrorMessage == "Select at most 50 drafters per export.");
  }

  [Theory]
  [InlineData("d_000000000000001")]
  [InlineData("dt_000000000000001")]
  [InlineData("000000000000001")]
  [InlineData("dr_short")]
  [InlineData("dr_")]
  [InlineData("dr_has space 12345")]
  public void Validate_ShouldFail_WhenAnIdIsMalformedOrHasTheWrongPrefix(string badId)
  {
    var result = _validator.Validate(Query([DrafterId(1), badId]));

    result.IsValid.Should().BeFalse();
    result
      .Errors.Should()
      .ContainSingle(e => e.ErrorMessage == "Drafter public ID is invalid.")
      .Which.PropertyName.Should()
      .Be("DrafterPublicIds[1]");
  }

  [Fact]
  public void Validate_ShouldFailWithoutThrowing_WhenTheListIsNull()
  {
    // An explicit JSON null deserializes into a null list; that must be a validation
    // failure, not a NullReferenceException (which would surface as a 500).
    var result = _validator.Validate(Query(null!));

    result.IsValid.Should().BeFalse();
  }
}

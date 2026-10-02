using DraftsValidator = ScreenDrafts.Modules.Drafts.Features.WikiExport.ExportDrafts.Validator;
using ExportDraftsWikiQuery = ScreenDrafts.Modules.Drafts.Features.WikiExport.ExportDrafts.ExportDraftsWikiQuery;

namespace ScreenDrafts.Modules.Drafts.UnitTests.WikiExport;

public sealed class ExportDraftsWikiValidatorTests
{
  private static readonly DraftsValidator _validator = new();

  private static string DraftId(int n) =>
    $"{PublicIdPrefixes.Draft}_{n.ToString("D15", System.Globalization.CultureInfo.InvariantCulture)}";

  private static ExportDraftsWikiQuery Query(IReadOnlyList<string> ids) =>
    new() { DraftPublicIds = ids };

  private static IReadOnlyList<string> Ids(int count) =>
    [.. Enumerable.Range(1, count).Select(DraftId)];

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
    result.Errors.Should().Contain(e => e.ErrorMessage == "Select at least one draft.");
  }

  [Fact]
  public void Validate_ShouldFail_When51IdsAreSelected()
  {
    var result = _validator.Validate(Query(Ids(51)));

    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.ErrorMessage == "Select at most 50 drafts per export.");
  }

  [Theory]
  [InlineData("dr_000000000000001")]
  [InlineData("p_000000000000001")]
  [InlineData("000000000000001")]
  [InlineData("d_short")]
  [InlineData("d_")]
  [InlineData("d_has space 123456")]
  public void Validate_ShouldFail_WhenAnIdIsMalformedOrHasTheWrongPrefix(string badId)
  {
    var result = _validator.Validate(Query([DraftId(1), badId]));

    result.IsValid.Should().BeFalse();
    result
      .Errors.Should()
      .ContainSingle(e => e.ErrorMessage == "Draft public ID is invalid.")
      .Which.PropertyName.Should()
      .Be("DraftPublicIds[1]");
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

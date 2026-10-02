using System.Globalization;
using System.Text.RegularExpressions;

namespace ScreenDrafts.Modules.Drafts.UnitTests.WikiExport;

public sealed partial class WikiTextToResponseTests
{
  [GeneratedRegex(@"^screendrafts-wiki-(?<kind>[a-z]+)-(?<date>\d{8})\.txt$")]
  private static partial Regex FileNamePattern();

  [Fact]
  public void ToResponse_ShouldStartEachPageWithThePageMarker()
  {
    var response = WikiText.ToResponse([new WikiPage("Alpha", "body")], "drafts");

    response.Content.Should().StartWith("=== Page: Alpha ===\n\nbody");
  }

  [Fact]
  public void ToResponse_ShouldSeparatePagesWithTwoBlankLines_AndEndWithASingleNewline()
  {
    var response = WikiText.ToResponse(
      [new WikiPage("Alpha", "AAA"), new WikiPage("Beta", "BBB")],
      "drafts"
    );

    response.Content.Should().Be("=== Page: Alpha ===\n\nAAA\n\n\n=== Page: Beta ===\n\nBBB\n");
  }

  [Fact]
  public void ToResponse_ShouldTrimTrailingWhitespaceFromEachPage()
  {
    var response = WikiText.ToResponse([new WikiPage("Alpha", "AAA  \n\n\n")], "drafts");

    response.Content.Should().Be("=== Page: Alpha ===\n\nAAA\n");
  }

  [Fact]
  public void ToResponse_ShouldEndWithExactlyOneTrailingNewline()
  {
    var response = WikiText.ToResponse([new WikiPage("Alpha", "AAA")], "drafts");

    response.Content.Should().EndWith("AAA\n");
    response.Content.Should().NotEndWith("\n\n");
  }

  [Theory]
  [InlineData(0)]
  [InlineData(1)]
  [InlineData(3)]
  public void ToResponse_ShouldReportThePageCount(int count)
  {
    var pages = Enumerable.Range(1, count).Select(i => new WikiPage($"P{i}", $"text {i}")).ToList();

    WikiText.ToResponse(pages, "drafts").PageCount.Should().Be(count);
  }

  [Fact]
  public void ToResponse_ShouldReturnJustANewline_WhenThereAreNoPages()
  {
    var response = WikiText.ToResponse([], "drafts");

    response.Content.Should().Be("\n");
    response.PageCount.Should().Be(0);
  }

  [Theory]
  [InlineData("drafts")]
  [InlineData("drafters")]
  public void ToResponse_ShouldNameTheFileAfterTheKindAndTheUtcDate(string kind)
  {
    // The date comes from the clock, so accept either side of a midnight rollover
    // rather than depending on when the test happens to run.
    var before = DateTime.UtcNow;
    var response = WikiText.ToResponse([new WikiPage("Alpha", "AAA")], kind);
    var after = DateTime.UtcNow;

    var match = FileNamePattern().Match(response.FileName);
    match.Success.Should().BeTrue($"'{response.FileName}' should match the file name pattern");
    match.Groups["kind"].Value.Should().Be(kind);
    match
      .Groups["date"]
      .Value.Should()
      .BeOneOf(
        before.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
        after.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
      );
  }

  [Fact]
  public void ToResponse_ShouldThrow_WhenPagesIsNull()
  {
    var act = () => WikiText.ToResponse(null!, "drafts");

    act.Should().Throw<ArgumentNullException>();
  }
}

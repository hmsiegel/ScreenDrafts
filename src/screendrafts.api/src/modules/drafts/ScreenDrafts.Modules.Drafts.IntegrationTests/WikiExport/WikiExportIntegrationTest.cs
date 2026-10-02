namespace ScreenDrafts.Modules.Drafts.IntegrationTests.WikiExport;

/// <summary>
/// Shared plumbing for the wiki-export golden-output tests: a seeder bound to this test's
/// DbContext, the stubbed Reporting API, a clean <c>CommissionerPersonPublicIds</c>, and helpers
/// for slicing a page out of the combined export file.
/// </summary>
public abstract class WikiExportIntegrationTest(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  private WikiSeeder? _seed;

  protected WikiSeeder Seed => _seed ??= new WikiSeeder(DbContext);

  protected FakeReportingApi ReportingApi { get; } =
    factory.Services.GetRequiredService<FakeReportingApi>();

  protected static DateOnly Date(int year, int month, int day) => new(year, month, day);

  protected override Task OnInitializeAsync()
  {
    ReportingApi.Reset();
    SetCommissioners();
    return base.OnInitializeAsync();
  }

  protected override Task OnDisposeAsync()
  {
    ReportingApi.Reset();
    SetCommissioners();
    return base.OnDisposeAsync();
  }

  /// <summary>
  /// DraftsOptions is a singleton shared by the whole collection; whatever a test sets here is
  /// reset before and after every test so no test inherits another's commissioners.
  /// </summary>
  protected void SetCommissioners(params string[] personPublicIds) =>
    GetService<IOptions<DraftsOptions>>().Value.CommissionerPersonPublicIds = personPublicIds;

  protected Task<Result<ExportWikiResponse>> ExportDraftsAsync(params string[] draftPublicIds) =>
    Sender.Send(
      new ExportDraftsWikiQuery { DraftPublicIds = draftPublicIds },
      TestContext.Current.CancellationToken
    );

  protected Task<Result<ExportWikiResponse>> ExportDraftersAsync(
    params string[] drafterPublicIds
  ) =>
    Sender.Send(
      new ExportDraftersWikiQuery { DrafterPublicIds = drafterPublicIds },
      TestContext.Current.CancellationToken
    );

  protected async Task<string> ExportDraftPageAsync(DraftSeed draft)
  {
    ArgumentNullException.ThrowIfNull(draft);

    var result = await ExportDraftsAsync(draft.PublicId);
    result.IsSuccess.Should().BeTrue();
    return PageText(result.Value, draft.Title);
  }

  protected async Task<string> ExportDrafterPageAsync(DrafterSeed drafter)
  {
    ArgumentNullException.ThrowIfNull(drafter);

    var result = await ExportDraftersAsync(drafter.PublicId);
    result.IsSuccess.Should().BeTrue();
    return PageText(result.Value, drafter.Person.DisplayName);
  }

  /// <summary>
  /// Returns the text of one page from the combined export, without its page marker and without
  /// the trailing blank lines that separate it from the next page.
  /// </summary>
  protected static string PageText(ExportWikiResponse response, string title)
  {
    ArgumentNullException.ThrowIfNull(response);

    var marker = $"=== Page: {title} ===\n\n";
    var start = response.Content.IndexOf(marker, StringComparison.Ordinal);
    start.Should().BeGreaterThanOrEqualTo(0, $"the export should contain a page titled '{title}'");

    var bodyStart = start + marker.Length;
    var next = response.Content.IndexOf("\n\n\n=== Page: ", bodyStart, StringComparison.Ordinal);

    return (
      next < 0 ? response.Content[bodyStart..] : response.Content[bodyStart..next]
    ).TrimEnd('\n');
  }

  protected static IReadOnlyList<string> PageTitles(ExportWikiResponse response)
  {
    ArgumentNullException.ThrowIfNull(response);

    return
    [
      .. System
        .Text.RegularExpressions.Regex.Matches(
          response.Content,
          "^=== Page: (.*) ===$",
          System.Text.RegularExpressions.RegexOptions.Multiline
        )
        .Select(m => m.Groups[1].Value),
    ];
  }

  /// <summary>The <c>{{Episodes ... }}</c> / <c>{{Drafter ... }}</c> template block.</summary>
  protected static string TemplateBlock(string pageText)
  {
    ArgumentNullException.ThrowIfNull(pageText);

    return pageText[..(pageText.IndexOf("}}", StringComparison.Ordinal) + 2)];
  }

  /// <summary>Everything from <paramref name="from"/> up to (not including) <paramref name="to"/>.</summary>
  protected static string Between(string text, string from, string to)
  {
    ArgumentNullException.ThrowIfNull(text);
    ArgumentNullException.ThrowIfNull(from);
    ArgumentNullException.ThrowIfNull(to);

    var start = text.IndexOf(from, StringComparison.Ordinal);
    start.Should().BeGreaterThanOrEqualTo(0, $"'{from}' should be present");

    var end = text.IndexOf(to, start + from.Length, StringComparison.Ordinal);
    end.Should().BeGreaterThan(start, $"'{to}' should follow '{from}'");

    return text[start..end];
  }

  /// <summary>One <c> | name = value</c> line of the episode template, padded to 17 characters.</summary>
  protected static string P(string name, string value = "")
  {
    ArgumentNullException.ThrowIfNull(name);

    return $" | {name.PadRight(17)} = {value}";
  }
}

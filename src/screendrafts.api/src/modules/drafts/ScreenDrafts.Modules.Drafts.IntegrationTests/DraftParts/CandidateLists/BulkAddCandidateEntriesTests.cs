using System.Text;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts.CandidateLists;

public sealed class BulkAddCandidateEntriesTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task BulkAddCandidateEntries_WithValidCsv_ShouldSucceedAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartAsync();
    var tmdbId1 = Faker.Random.Int(1, 500_000);
    var tmdbId2 = Faker.Random.Int(500_001, 1_000_000);

    using var csvStream = BuildCsvStream(
      ("Movie One", tmdbId1),
      ("Movie Two", tmdbId2));

    var command = new BulkAddCandidateEntriesCommand
    {
      DraftPartId = draftPartPublicId,
      CsvStream = csvStream,
      AddedByPublicId = "u_" + Faker.Random.AlphaNumeric(17)
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task BulkAddCandidateEntries_ShouldCountAddedEntriesCorrectlyAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartAsync();
    var tmdbId1 = Faker.Random.Int(1, 500_000);
    var tmdbId2 = Faker.Random.Int(500_001, 1_000_000);

    using var csvStream = BuildCsvStream(
      ("Movie One", tmdbId1),
      ("Movie Two", tmdbId2));

    var command = new BulkAddCandidateEntriesCommand
    {
      DraftPartId = draftPartPublicId,
      CsvStream = csvStream,
      AddedByPublicId = "u_" + Faker.Random.AlphaNumeric(17)
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Value.TotalRows.Should().Be(2);
    result.Value.AddedEntries.Should().Be(2);
    result.Value.SkipedEntries.Should().Be(0);
    result.Value.FailedEntries.Should().Be(0);
  }

  [Fact]
  public async Task BulkAddCandidateEntries_ShouldPersistEntriesInDatabaseAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartAsync();
    var tmdbId1 = Faker.Random.Int(1, 500_000);
    var tmdbId2 = Faker.Random.Int(500_001, 1_000_000);

    using var csvStream = BuildCsvStream(
      ("Movie One", tmdbId1),
      ("Movie Two", tmdbId2));

    var command = new BulkAddCandidateEntriesCommand
    {
      DraftPartId = draftPartPublicId,
      CsvStream = csvStream,
      AddedByPublicId = "u_" + Faker.Random.AlphaNumeric(17)
    };

    // Act
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    var entries = await DbContext.CandidateListEntries
      .Where(e => e.TmdbId == tmdbId1 || e.TmdbId == tmdbId2)
      .ToListAsync(TestContext.Current.CancellationToken);

    entries.Should().HaveCount(2);
    entries.Should().Contain(e => e.TmdbId == tmdbId1);
    entries.Should().Contain(e => e.TmdbId == tmdbId2);
  }

  // -------------------------------------------------------------------------
  // Guard — skip existing entries
  // -------------------------------------------------------------------------

  [Fact]
  public async Task BulkAddCandidateEntries_ShouldSkipExistingEntriesAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartAsync();
    var existingTmdbId = Faker.Random.Int(1, 500_000);
    var newTmdbId = Faker.Random.Int(500_001, 1_000_000);

    // Add one entry first
    await Sender.Send(new AddCandidateEntryCommand
    {
      DraftPartId = draftPartPublicId,
      TmdbId = existingTmdbId,
      AddedByPublicId = "u_" + Faker.Random.AlphaNumeric(17)
    }, TestContext.Current.CancellationToken);

    using var csvStream = BuildCsvStream(
      ("Existing Movie", existingTmdbId),
      ("New Movie", newTmdbId));

    var command = new BulkAddCandidateEntriesCommand
    {
      DraftPartId = draftPartPublicId,
      CsvStream = csvStream,
      AddedByPublicId = "u_" + Faker.Random.AlphaNumeric(17)
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Value.AddedEntries.Should().Be(1);
    result.Value.SkipedEntries.Should().Be(1);
  }

  // -------------------------------------------------------------------------
  // Guard — failed rows for missing TmdbId
  // -------------------------------------------------------------------------

  [Fact]
  public async Task BulkAddCandidateEntries_ShouldCountFailedRows_WhenTmdbIdIsMissingAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartAsync();
    var validTmdbId = Faker.Random.Int(1, 1_000_000);

    var csvContent = $"Title,TmdbId\nGood Movie,{validTmdbId}\nBad Movie,\n";
    using var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

    var command = new BulkAddCandidateEntriesCommand
    {
      DraftPartId = draftPartPublicId,
      CsvStream = csvStream,
      AddedByPublicId = "u_" + Faker.Random.AlphaNumeric(17)
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Value.AddedEntries.Should().Be(1);
    result.Value.FailedEntries.Should().Be(1);
    result.Value.Failures.Should().ContainSingle(f => f.Title == "Bad Movie");
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task BulkAddCandidateEntries_ShouldFail_WhenDraftPartNotFoundAsync()
  {
    // Arrange
    using var csvStream = BuildCsvStream(("Movie One", Faker.Random.Int(1, 1_000_000)));

    var command = new BulkAddCandidateEntriesCommand
    {
      DraftPartId = "dp_nonexistent",
      CsvStream = csvStream,
      AddedByPublicId = "u_" + Faker.Random.AlphaNumeric(17)
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Code.Should().Be(CandidateListErrors.DraftPartNotFound("dp_nonexistent").Code);
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<string> SetupDraftPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftAndPartAsync(seriesId);
    var draftPartInternalId = await GetFirstDraftPartIdAsync(draftPublicId);

    var draftPart = await DbContext.DraftParts
      .FirstAsync(dp => dp.Id == DraftPartId.Create(draftPartInternalId), TestContext.Current.CancellationToken);

    return draftPart.PublicId;
  }

  private async Task<Guid> CreateSeriesAsync()
  {
    var result = await Sender.Send(new CreateSeriesCommand
    {
      Name = Faker.Company.CompanyName() + Faker.Random.AlphaNumeric(6),
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = ContinuityScope.None.Value,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = (int)DraftTypeMask.All,
      DefaultDraftType = DraftType.Standard.Value
    }, TestContext.Current.CancellationToken);

    return result.Value;
  }

  private async Task<string> CreateDraftAndPartAsync(Guid seriesId)
  {
    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId
    }, TestContext.Current.CancellationToken);

    var draftPublicId = draftResult.Value;

    await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7
    }, TestContext.Current.CancellationToken);

    return draftPublicId;
  }

  private static MemoryStream BuildCsvStream(params (string Title, int TmdbId)[] rows)
  {
    var sb = new System.Text.StringBuilder();
    sb.AppendLine("Title,TmdbId");
    foreach (var (title, tmdbId) in rows)
    {
      sb.Append(title);
      sb.Append(',');
      sb.Append(tmdbId.ToString(System.Globalization.CultureInfo.InvariantCulture));
      sb.AppendLine();
    }

    return new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
  }
}

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Predictions;

/// <summary>
/// Shared setup helpers for Prediction integration tests.
/// </summary>
public abstract class PredictionIntegrationTestBase(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  protected async Task<string> CreateDraftPartPublicIdAsync(int partIndex = 1)
  {
    var seriesId = await CreateSeriesIdAsync();
    var draftPublicId = await CreateDraftPublicIdAsync(seriesId);
    var result = await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = partIndex,
      MinimumPosition = 1,
      MaximumPosition = 7
    }, TestContext.Current.CancellationToken);
    return result.Value;
  }

  protected async Task<string> CreateSeasonPublicIdAsync(int? number = null)
  {
    var result = await Sender.Send(new CreatePredictionSeasonCommand
    {
      Number = number ?? Faker.Random.Int(1, 100),
      StartsOn = DateOnly.FromDateTime(Faker.Date.Past())
    }, TestContext.Current.CancellationToken);
    return result.Value;
  }

  protected async Task<string> CreateContestantPublicIdAsync()
  {
    var people = new PeopleFactory(Sender, Faker);
    var personPublicId = await people.CreateAndSavePersonAsync();
    var result = await Sender.Send(new CreatePredictionContestantCommand
    {
      PersonPublicId = personPublicId
    }, TestContext.Current.CancellationToken);
    return result.Value;
  }

  protected async Task SetRulesAsync(
    string draftPartPublicId,
    int requiredCount = 3,
    int predictionMode = 0 /* UnorderedAll */,
    int? topN = null)
  {
    var result = await Sender.Send(new SetDraftPartPredictionRulesCommand
    {
      DraftPartPublicId = draftPartPublicId,
      PredictionMode = predictionMode,
      RequiredCount = requiredCount,
      TopN = topN
    }, TestContext.Current.CancellationToken);
    result.IsSuccess.Should().BeTrue("rules must be created before submitting sets");
  }

  /// <summary>
  /// Submits a prediction set and returns its public ID from the database.
  /// </summary>
  protected async Task<string> SubmitSetAsync(
    string draftPartPublicId,
    string seasonPublicId,
    string contestantPublicId,
    params string[] mediaPublicIds)
  {
    var entries = mediaPublicIds.Select((id, idx) => new PredictionEntryDto
    {
      MediaPublicId = id,
      MediaTitle = $"Movie {idx + 1}"
    }).ToList();

    var command = new SubmitPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SeasonPublicId = seasonPublicId,
      ContestantPublicId = contestantPublicId,
      SourceKind = PredictionSourceKind.UI.Value,
      Entries = entries
    };

    var result = await Sender.Send(command, TestContext.Current.CancellationToken);
    result.IsSuccess.Should().BeTrue("set submission must succeed");

    var draftPart = await DbContext.DraftParts.FirstAsync(dp => dp.PublicId == draftPartPublicId, TestContext.Current.CancellationToken);
    var contestant = await DbContext.PredictionContestants.FirstAsync(c => c.PublicId == contestantPublicId, TestContext.Current.CancellationToken);
    var set = await DbContext.DraftPredictionSets
      .FirstAsync(s => s.DraftPartId == draftPart.Id && s.ContestantId == contestant.Id, TestContext.Current.CancellationToken);
    return set.PublicId;
  }

  // ──────────────────────────────────────────────────────────────────────
  // Private infrastructure helpers
  // ──────────────────────────────────────────────────────────────────────

  private async Task<Guid> CreateSeriesIdAsync()
  {
    var result = await Sender.Send(new CreateSeriesCommand
    {
      Name = Faker.Company.CompanyName(),
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = ContinuityScope.None.Value,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = (int)DraftTypeMask.All,
      DefaultDraftType = DraftType.Standard.Value
    }, TestContext.Current.CancellationToken);
    return result.Value;
  }

  private async Task<string> CreateDraftPublicIdAsync(Guid seriesId)
  {
    var result = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId
    }, TestContext.Current.CancellationToken);
    return result.Value;
  }
}

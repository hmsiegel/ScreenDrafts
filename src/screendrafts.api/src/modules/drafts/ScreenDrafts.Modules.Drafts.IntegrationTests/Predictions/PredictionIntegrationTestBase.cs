namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Predictions;

/// <summary>
/// Shared setup helpers for Prediction integration tests.
/// </summary>
public abstract class PredictionIntegrationTestBase(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  private readonly Dictionary<string, string> _actorUserPublicIdByContestant = new();
  private readonly Dictionary<string, List<string>> _configuredContestantsByDraftPart = new();

  protected async Task<string> CreateDraftPartPublicIdAsync(int partIndex = 1)
  {
    var seriesId = await CreateSeriesIdAsync();
    var draftPublicId = await CreateDraftPublicIdAsync(seriesId);

    // CreateDraft auto-creates part index 1 (min=1, max=7) when no Parts are supplied,
    // which matches the values requested here — reuse it instead of colliding with it.
    var existingPart = await DbContext.Drafts
      .Where(d => d.PublicId == draftPublicId)
      .SelectMany(d => d.Parts)
      .FirstOrDefaultAsync(p => p.PartIndex == partIndex, TestContext.Current.CancellationToken);

    if (existingPart is not null)
    {
      return existingPart.PublicId;
    }

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
    var contestantPublicId = result.Value;

    // The contestant's Person is linked to a fake User so SubmitSetAsync (and
    // any test building a SubmitPredictionSetCommand directly) can supply a
    // caller identity that resolves back to this contestant.
    var userId = Guid.NewGuid();
    var actorUserPublicId = FakeUsersApi.RegisterUser(userId, $"u_{Faker.Random.AlphaNumeric(16)}");
    var linkResult = await Sender.Send(new LinkUserPersonCommand
    {
      PublicId = personPublicId,
      UserId = userId
    }, TestContext.Current.CancellationToken);
    linkResult.IsSuccess.Should().BeTrue("test setup must be able to link the contestant's Person to a fake User");

    _actorUserPublicIdByContestant[contestantPublicId] = actorUserPublicId;

    return contestantPublicId;
  }

  /// <summary>
  /// Returns the fake User public ID linked to the contestant's Person by
  /// CreateContestantPublicIdAsync — i.e. the identity that is authorized to
  /// submit predictions on that contestant's behalf.
  /// </summary>
  protected string GetActorUserPublicIdForContestant(string contestantPublicId) =>
    _actorUserPublicIdByContestant[contestantPublicId];

  /// <summary>
  /// Ensures a DraftPartPredictor exists for the contestant on the given draft
  /// part, without clobbering predictors already configured for other
  /// contestants on the same draft part.
  /// </summary>
  protected async Task EnsurePredictorConfiguredAsync(string draftPartPublicId, string contestantPublicId)
  {
    if (!_configuredContestantsByDraftPart.TryGetValue(draftPartPublicId, out var contestants))
    {
      contestants = [];
      _configuredContestantsByDraftPart[draftPartPublicId] = contestants;
    }

    if (contestants.Contains(contestantPublicId))
    {
      return;
    }

    contestants.Add(contestantPublicId);

    var result = await Sender.Send(new SetDraftPartPredictorsCommand
    {
      DraftPartPublicId = draftPartPublicId,
      Predictors = [.. contestants.Select(id => new PredictorEntryDto { ContestantPublicId = id })]
    }, TestContext.Current.CancellationToken);

    result.IsSuccess.Should().BeTrue("predictor setup must succeed before submitting a prediction set");
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
    params int[] tmdbIds)
  {
    await EnsurePredictorConfiguredAsync(draftPartPublicId, contestantPublicId);

    var entries = tmdbIds.Select((id, idx) => new PredictionEntryDto
    {
      TmdbId = id,
      MediaTitle = $"Movie {idx + 1}"
    }).ToList();

    var command = new SubmitPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SeasonPublicId = seasonPublicId,
      ContestantPublicId = contestantPublicId,
      SourceKind = PredictionSourceKind.UI.Value,
      Entries = entries,
      ActorUserPublicId = GetActorUserPublicIdForContestant(contestantPublicId)
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

  private async Task<string> CreateSeriesIdAsync()
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

  private async Task<string> CreateDraftPublicIdAsync(string seriesId)
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

namespace ScreenDrafts.Modules.Drafts.UnitTests.TestUtils;

public static class PredictionFactory
{
  private static readonly Faker _faker = new();

  public static PredictionSeason CreateSeason(int? number = null) =>
    PredictionSeason.Create(
      number: number ?? _faker.Random.Int(1, 50),
      startsOn: DateOnly.FromDateTime(_faker.Date.Past()),
      publicId: _faker.Random.AlphaNumeric(10));

  public static PredictionContestant CreateContestant(Domain.People.Person? person = null)
  {
    person ??= Domain.People.Person.Create(
      _faker.Random.AlphaNumeric(10),
      _faker.Name.FirstName(),
      _faker.Name.LastName()).Value;

    return PredictionContestant.Create(person, _faker.Random.AlphaNumeric(10));
  }

  public static DraftPartPredictionRule CreateUnorderedAllRule(DraftPart draftPart, int requiredCount = 3) =>
    DraftPartPredictionRule.Create(
      publicId: _faker.Random.AlphaNumeric(10),
      draftPart: draftPart,
      predictionMode: PredictionMode.UnorderedAll,
      requiredCount: requiredCount).Value;

  public static DraftPartPredictionRule CreateUnorderedTopNRule(DraftPart draftPart, int requiredCount = 3, int topN = 5) =>
    DraftPartPredictionRule.Create(
      publicId: _faker.Random.AlphaNumeric(10),
      draftPart: draftPart,
      predictionMode: PredictionMode.UnorderedTopN,
      requiredCount: requiredCount,
      topN: topN).Value;

  public static DraftPredictionSet CreateSet(
    DraftPart draftPart,
    PredictionSeason season,
    PredictionContestant contestant) =>
    DraftPredictionSet.Create(
      publicId: _faker.Random.AlphaNumeric(10),
      season: season,
      draftPart: draftPart,
      contestant: contestant,
      submittedByPerson: null,
      sourceKind: PredictionSourceKind.UI).Value;

  public static DraftPredictionSet CreateLockedSet(
    DraftPart draftPart,
    PredictionSeason season,
    PredictionContestant contestant,
    DraftPartPredictionRule rules,
    IEnumerable<string>? mediaPublicIds = null)
  {
    ArgumentNullException.ThrowIfNull(rules);

    var set = CreateSet(draftPart, season, contestant);

    var ids = (mediaPublicIds ?? Enumerable.Range(1, rules.RequiredCount).Select(i => $"m_{i:D3}")).ToList();
    var entries = ids.Select((id, idx) => PredictionEntry.Create(set, id, $"Movie {idx + 1}")).ToList();
    set.ReplaceEntries(entries);

    var snapshot = rules.ToSnapshot();
    set.Lock(snapshot, DateTime.UtcNow);

    return set;
  }
}

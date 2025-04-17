namespace ScreenDrafts.Modules.Drafts.IntegrationTests.TestUtils;
internal sealed class ValidDraftPositionData : IEnumerable<object[]>
{
  private readonly Faker _faker = new();
  public IEnumerator<object[]> GetEnumerator()
  {
    yield return new object[]
    {
      _faker.Company.CompanyName(),
      DraftType.MiniMega.Name,
      13,
      3,
      0,
      2,
      EpisodeType.MainFeed.Name,
      DraftStatus.Created.Name,
      new List<DraftPosition>
      {
        DraftPosition.Create("Drafter C", [13, 12, 9, 6, 3], false, true).Value,
        DraftPosition.Create("Drafter B", [11, 8, 5, 2]).Value,
        DraftPosition.Create("Drafter A", [10, 7, 4, 1], true).Value
      }
    };

    yield return new object[]
    {
      _faker.Company.CompanyName(),
      DraftType.Mega.Name,
      20,
      4,
      0,
      2,
      EpisodeType.MainFeed.Name,
      DraftStatus.Created.Name,
      new List<DraftPosition>
      {
        DraftPosition.Create("Drafter A", [14, 10, 6, 1], true).Value,
        DraftPosition.Create("Drafter B", [19, 18, 15, 11, 7, 2]).Value,
        DraftPosition.Create("Drafter C", [20, 16, 12, 8, 3], false, true).Value,
        DraftPosition.Create("Drafter D", [17, 13, 9, 5, 4], true).Value
      }
    };

    yield return new object[]
    {
      _faker.Company.CompanyName(),
      DraftType.Super.Name,
      30,
      9,
      0,
      2,
      EpisodeType.MainFeed.Name,
      DraftStatus.Created.Name,
      new List<DraftPosition>
      {
        DraftPosition.Create("Drafter A", [27, 24, 21], false, true).Value,
        DraftPosition.Create("Drafter B", [28, 25, 22], true).Value,
        DraftPosition.Create("Drafter C", [30, 29, 26, 23]).Value,
        DraftPosition.Create("Drafter D", [17, 14, 11], false, true).Value,
        DraftPosition.Create("Drafter E", [18, 15, 22], true).Value,
        DraftPosition.Create("Drafter F", [20, 19, 16, 13]).Value,
        DraftPosition.Create("Drafter G", [7, 4, 1], false, true).Value,
        DraftPosition.Create("Drafter H", [8, 5, 2], false).Value,
        DraftPosition.Create("Drafter I", [10, 9, 6, 3]).Value
      }
    };
  }

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

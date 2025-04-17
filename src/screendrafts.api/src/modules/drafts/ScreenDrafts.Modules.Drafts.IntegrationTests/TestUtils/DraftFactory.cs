namespace ScreenDrafts.Modules.Drafts.IntegrationTests.TestUtils;

public static class DraftFactory 
{
  private static readonly Faker _faker = new();

  public static Result<Draft> CreateStandardDraft() => Draft.Create(
      Title.Create(_faker.Lorem.Word()),
      DraftType.Standard,
      7,
      2,
      0,
      2,
      DraftStatus.Created,
      EpisodeType.MainFeed);

  public static Result<Draft> CreateMiniMegaDraft() => Draft.Create(
      Title.Create(_faker.Lorem.Word()),
      DraftType.MiniMega,
      11,
      3,
      0,
      2,
      DraftStatus.Created,
      EpisodeType.MainFeed);

  public static Result<Draft> CreateMegaDraft() => Draft.Create(
      Title.Create(_faker.Lorem.Word()),
      DraftType.Mega,
      20,
      4,
      0,
      2,
      DraftStatus.Created,
      EpisodeType.MainFeed);

  public static Collection<DraftPosition> CreateStandardDraftPositions() =>
  [
      DraftPosition.Create("Drafter A", [7,6,4,2]).Value,
      DraftPosition.Create("Drafter B", [5,3,1]).Value
  ];
  public static Collection<DraftPosition> CreateMegaDraftPositions() =>
  [
      DraftPosition.Create("Drafter A", [14,10,6,1], true).Value,
      DraftPosition.Create("Drafter B", [19,18,15,11,7,2]).Value,
      DraftPosition.Create("Drafter C", [20,16,12,8,3], false, true).Value,
      DraftPosition.Create("Drafter D", [17,13,9,5,4]).Value
  ];
  public static Collection<DraftPosition> CreateMiniMegaDraftPositions() =>
  [
      DraftPosition.Create("Drafter A", [11, 10, 6, 3], false, true).Value,
      DraftPosition.Create("Drafter B", [9, 8, 5, 2]).Value,
      DraftPosition.Create("Drafter C", [7, 4, 1], true, false).Value,
  ];
}

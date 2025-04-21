namespace ScreenDrafts.Modules.Drafts.UnitTests.TestUtils;

public static class PickFactory
{
  private static readonly Faker _faker = new();

  public static Result<Pick> CreatePick() =>
    Pick.Create(
      _faker.Random.Int(1, 7),
      MovieFactory.CreateMovie().Value,
      DrafterFactory.CreateDrafter(),
      null,
      DraftFactory.CreateStandardDraft().Value,
      _faker.Random.Int(1, 7));

}

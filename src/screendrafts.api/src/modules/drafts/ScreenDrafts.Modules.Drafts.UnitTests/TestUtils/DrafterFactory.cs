namespace ScreenDrafts.Modules.Drafts.UnitTests.TestUtils;

public static class DrafterFactory
{
  private static readonly Faker _faker = new();

  public static Drafter CreateDrafter() =>
    Drafter.Create(
      PersonFactory.CreatePerson().Value).Value;

  public static DrafterTeam CreateDrafterTeam() =>
    DrafterTeam.Create(
      _faker.Company.CompanyName()).Value;
}

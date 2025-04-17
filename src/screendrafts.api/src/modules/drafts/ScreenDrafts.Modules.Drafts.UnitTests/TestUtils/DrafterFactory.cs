namespace ScreenDrafts.Modules.Drafts.UnitTests.TestUtils;

public static class DrafterFactory
{
  private static readonly Faker _faker = new();

  public static Result<Drafter> CreateDrafterWithUserId() =>
    Drafter.Create(
      _faker.Name.FullName(),
      TestConstants.Constants.User.Id);

  public static Drafter CreateDrafter() =>
    Drafter.Create(
      _faker.Name.FullName()).Value;

  public static DrafterTeam CreateDrafterTeam() =>
    DrafterTeam.Create(
      _faker.Company.CompanyName()).Value;
}

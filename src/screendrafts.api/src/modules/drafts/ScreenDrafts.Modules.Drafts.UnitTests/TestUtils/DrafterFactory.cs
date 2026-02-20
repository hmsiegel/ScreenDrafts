using ScreenDrafts.Modules.Drafts.Domain.DrafterTeams;

namespace ScreenDrafts.Modules.Drafts.UnitTests.TestUtils;

public static class DrafterFactory
{
  private static readonly Faker _faker = new();

  public static Drafter CreateDrafter() =>
    Drafter.Create(
      person: PersonFactory.CreatePerson().Value,
      publicId: _faker.Random.AlphaNumeric(10)).Value;

  public static DrafterTeam CreateDrafterTeam() =>
    DrafterTeam.Create(
      name: _faker.Company.CompanyName()).Value;
}

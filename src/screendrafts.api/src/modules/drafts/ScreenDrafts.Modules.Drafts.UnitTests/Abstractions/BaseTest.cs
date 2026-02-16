using ScreenDrafts.Common.Abstractions.Exceptions;

namespace ScreenDrafts.Modules.Drafts.UnitTests.Abstractions;

public abstract class DraftsBaseTest : BaseTest
{
  protected static Series CreateSeries()
  {
    var publicId = Faker.Random.AlphaNumeric(10);

    return Series.Create(
      Faker.Lorem.Word(),
      publicId,
      CanonicalPolicy.Always,
      ContinuityScope.Global,
      ContinuityDateRule.AnyChannelFirstRelease,
      SeriesKind.Regular,
      DraftType.Standard,
      DraftTypeMask.Standard).Value;
  }

  protected static Draft CreateDraft()
  {
    var title = Title.Create(Faker.Lorem.Word());
    var publicId = Faker.Random.AlphaNumeric(10);
    var draftType = DraftType.Standard;
    var series = CreateSeries();

    return Draft.Create(title, publicId, draftType, series).Value;
  }

  protected static Category CreateCategory()
  {
    return Category.Create(
      Faker.Random.AlphaNumeric(10),
      Faker.Lorem.Word(),
      Faker.Lorem.Paragraph()).Value;
  }

  protected static Campaign CreateCampaign()
  {
    return Campaign.Create(
      Faker.Lorem.Word(),
      Faker.Company.CompanyName(),
      Faker.Random.AlphaNumeric(10)).Value;
  }

  protected static Domain.People.Person CreatePerson()
  {
    return Domain.People.Person.Create(
      Faker.Random.AlphaNumeric(10),
      Faker.Name.FirstName(),
      Faker.Name.LastName()).Value;
  }

  protected static Drafter CreateDrafter(Domain.People.Person? person = null)
  {
    person ??= CreatePerson();
    return Drafter.Create(person, Faker.Random.AlphaNumeric(10)).Value;
  }

  protected static DrafterTeam CreateDrafterTeam(Domain.People.Person? person = null, Collection<Drafter>? drafters = null)
  {
    var team = DrafterTeam.Create(Faker.Commerce.ProductName()).Value;

    if (drafters != null)
    {
      foreach (var drafter in drafters)
      {
        team.AddDrafter(drafter);
      }
    }

    return team;
  }

  protected static Host CreateHost(Domain.People.Person? person = null)
  {
    person ??= CreatePerson();
    return Host.Create(person, Faker.Random.AlphaNumeric(10)).Value;
  }

  protected static Movie CreateMovie()
  {
    return Movie.Create(
      Faker.Random.Word(),
      Faker.Random.AlphaNumeric(5),
      Guid.NewGuid()).Value;
  }

  protected static DraftPart CreateDraftPart(Draft? draft = null)
  {
    draft ??= CreateDraft();

    return DraftPart.Create(
      draft.Id,
      Faker.Random.Int(1, 10),
      DraftPartGamePlaySnapshot.Create(
        1,
        7,
        DraftType.Standard,
        CreateSeries().Id).Value).Value;
  }

  protected static ParticipantId CreateParticipantId(Drafter drafter)
  {
    return TestParticipantFactory.CreateParticipantId(drafter);
  }

  protected static ParticipantId CreateParticipantId(DrafterTeam drafterTeam)
  {
    return TestParticipantFactory.CreateParticipantId(drafterTeam);
  }

  protected static DraftPartParticipant CreateDraftPartParticipant(
    DraftPart draftPart,
    Drafter drafter)
  {
    return TestParticipantFactory.CreateDraftPartParticipant(draftPart, drafter);
  }

  protected static DraftPartParticipant CreateDraftPartParticipant(
    DraftPart draftPart,
    DrafterTeam drafterTeam)
  {
    return TestParticipantFactory.CreateDraftPartParticipant(draftPart, drafterTeam);
  }
}

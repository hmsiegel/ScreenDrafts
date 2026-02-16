namespace ScreenDrafts.Modules.Drafts.UnitTests.TestUtils;

/// <summary>
/// Factory for creating test participants (Drafters/DrafterTeams) and DraftPartParticipants.
/// </summary>
public static class TestParticipantFactory
{

  /// <summary>
  /// Creates a ParticipantId from a Drafter.
  /// </summary>
  public static ParticipantId CreateParticipantId(Drafter drafter)
  {
    ArgumentNullException.ThrowIfNull(drafter);
    return new ParticipantId(drafter.Id.Value, ParticipantKind.Drafter);
  }

  /// <summary>
  /// Creates a ParticipantId from a DrafterTeam.
  /// </summary>
  public static ParticipantId CreateParticipantId(DrafterTeam drafterTeam)
  {
    ArgumentNullException.ThrowIfNull(drafterTeam);
    return new ParticipantId(drafterTeam.Id.Value, ParticipantKind.Team);
  }

  /// <summary>
  /// Creates a DraftPartParticipant for a Drafter.
  /// </summary>
  public static DraftPartParticipant CreateDraftPartParticipant(
    DraftPart draftPart,
    Drafter drafter)
  {
    var participantId = CreateParticipantId(drafter);
    return DraftPartParticipant.Create(draftPart, participantId);
  }

  /// <summary>
  /// Creates a DraftPartParticipant for a DrafterTeam.
  /// </summary>
  public static DraftPartParticipant CreateDraftPartParticipant(
    DraftPart draftPart,
    DrafterTeam drafterTeam)
  {
    var participantId = CreateParticipantId(drafterTeam);
    return DraftPartParticipant.Create(draftPart, participantId);
  }

  /// <summary>
  /// Creates a test Drafter with a random person.
  /// </summary>
  public static Drafter CreateTestDrafter(Faker faker, PersonId? personId = null)
  {
    ArgumentNullException.ThrowIfNull(faker);

    var person = Domain.People.Person.Create(
      faker.Random.AlphaNumeric(10),
      faker.Name.FirstName(),
      faker.Name.LastName()).Value;

    return Drafter.Create(person, faker.Random.AlphaNumeric(10)).Value;
  }

  /// <summary>
  /// Creates a test DrafterTeam with drafters.
  /// </summary>
  internal static DrafterTeam CreateTestDrafterTeam(
    Faker faker,
    List<Drafter>? drafters = null)
  {

    var team = DrafterTeam.Create(faker.Commerce.ProductName()).Value;

    if (drafters != null)
    {
      foreach (var drafter in drafters)
      {
        team.AddDrafter(drafter);
      }
    }

    return team;
  }
}

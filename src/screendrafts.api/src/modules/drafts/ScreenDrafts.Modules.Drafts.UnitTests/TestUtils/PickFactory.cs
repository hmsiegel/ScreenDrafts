namespace ScreenDrafts.Modules.Drafts.UnitTests.TestUtils;

public static class PickFactory
{
  private static readonly Faker _faker = new();

  public static Result<Pick> CreatePick()
  {
    var draft = DraftFactory.CreateStandardDraft();
    draft.AddPart(1, 1, 7);
    var draftPart = draft.Parts.First();
    var drafter = DrafterFactory.CreateDrafter();
    var participantId = ParticipantId.From(drafter.Id);
    draftPart.AddParticipant(participantId);

    var movie = MovieFactory.CreateMovie().Value;
    var position = _faker.Random.Int(1, 10);
    var playOrder = _faker.Random.Int(1, 10);

    // Use reflection to call internal Pick.Create method
    var pickType = typeof(Pick);
    var createMethod = pickType.GetMethod("Create", 
      BindingFlags.NonPublic | BindingFlags.Static,
      null,
      [typeof(int), typeof(Movie), typeof(ParticipantId), typeof(DraftPart), typeof(int), typeof(PickId)],
      null) ?? throw new InvalidOperationException("Could not find Pick.Create method");
    var result = createMethod.Invoke(null, [position, movie, participantId, draftPart, playOrder, null]);

    return (Result<Pick>)result!;
  }
}

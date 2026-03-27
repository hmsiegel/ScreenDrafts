namespace ScreenDrafts.Modules.Drafts.UnitTests.TestUtils;

public static class PickFactory
{
  private static readonly Faker _faker = new();

  public static Result<Pick> CreatePick()
  {
    var draft = DraftFactory.CreateStandardDraft();
    draft.AddPart(1, 1, 7, _faker.Random.AlphaNumeric(10));
    var draftPart = draft.Parts.First();
    var drafter = DrafterFactory.CreateDrafter();
    var participantId = Participant.From(drafter.Id);
    draftPart.AddParticipant(participantId);
    draftPart.SetDraftStatus(DraftPartStatus.InProgress);

    var movie = MovieFactory.CreateMovie().Value;
    var position = _faker.Random.Int(1, 7);
    var playOrder = _faker.Random.Int(1, 10);
    var canonicalPolicyValue = _faker.Random.Int(0, 2);

    var pickIdResult = draftPart.PlayPick(
      movie: movie,
      draftPosition: position,
      playOrder: playOrder,
      participantId: participantId,
      canonicalPolicyValue: canonicalPolicyValue);

    if (pickIdResult.IsFailure)
    {
      return Result.Failure<Pick>(pickIdResult.Errors);
    }

    var pick = draftPart.Picks.First(p => p.Id == pickIdResult.Value);
    return Result.Success(pick);
  }
}

using ScreenDrafts.Modules.Drafts.Domain.Participants;

namespace ScreenDrafts.Modules.Drafts.UnitTests.TestUtils;

public static class PickFactory
{
  private static readonly Faker _faker = new();
  private static readonly ISeriesPolicyProvider _seriesPolicyProvider = new TestSeriesPolicyProvider();

  public static Result<Pick> CreatePick()
  {
    var draft = DraftFactory.CreateStandardDraft();
    draft.AddPart(1, 1, 7);
    var draftPart = draft.Parts.First();
    var drafter = DrafterFactory.CreateDrafter();
    var participantId = Participant.From(drafter.Id);
    draftPart.AddParticipant(participantId);
    draftPart.SetDraftStatus(DraftPartStatus.InProgress);

    var movie = MovieFactory.CreateMovie().Value;
    var position = _faker.Random.Int(1, 7);
    var playOrder = _faker.Random.Int(1, 10);

    var pickIdResult = draftPart.PlayPick(
      _seriesPolicyProvider,
      draftPart.SeriesId,
      draftPart.DraftType,
      movie,
      position,
      playOrder,
      participantId);

    if (pickIdResult.IsFailure)
    {
      return Result.Failure<Pick>(pickIdResult.Errors);
    }

    var pick = draftPart.Picks.First(p => p.Id == pickIdResult.Value);
    return Result.Success(pick);
  }

  private sealed class TestSeriesPolicyProvider : ISeriesPolicyProvider
  {
    public ContinuityScope GetContinuityScope(SeriesId seriesId) => ContinuityScope.Global;

    public CanonicalPolicy GetCanonicalPolicy(SeriesId seriesId) => CanonicalPolicy.Always;

    public PartBudget GetPartBudget(SeriesId seriesId, DraftType draftType, int partNumber, int totalParticipants)
    {
      return new PartBudget(
        MaxVetoes: 2,
        MaxVetoOverrides: 2,
        MaxCommunityPicks: 1);
    }
  }
}

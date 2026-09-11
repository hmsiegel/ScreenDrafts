using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Entities;

namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.TestUtils;

public static class GuestDraftPickFactory
{
  private static readonly Faker _faker = new();

  public static Pick CreatePick(
    Draft guestDraft,
    DraftParticipant playedBy,
    int position = 1,
    int playOrder = 1,
    string? moviePublicId = null,
    Guid? movieId = null
  )
  {
    ArgumentNullException.ThrowIfNull(guestDraft);
    ArgumentNullException.ThrowIfNull(playedBy);

    var result = guestDraft.PlayPick(
      moviePublicId: moviePublicId ?? _faker.Random.AlphaNumeric(10),
      movieId: movieId ?? Guid.NewGuid(),
      position: position,
      playOrder: playOrder,
      participantId: playedBy.Id.Value
    );

    return guestDraft.Picks.First(p => p.Id == result.Value);
  }
}

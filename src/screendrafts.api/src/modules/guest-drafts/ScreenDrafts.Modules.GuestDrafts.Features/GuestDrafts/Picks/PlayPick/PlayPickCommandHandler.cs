using ScreenDrafts.Modules.GuestDrafts.Domain.Drafters;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Repositories;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.ValueObjects;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.PlayPick;

internal sealed class PlayPickCommandHandler(
  IDraftRepository guestDraftRepository,
  IDrafterRepository guestDrafterRepository,
  IUsersApi usersApi,
  IMovieRepository guestDraftMovieRepository
) : ICommandHandler<PlayPickCommand>
{
  private readonly IDraftRepository _guestDraftRepository = guestDraftRepository;
  private readonly IDrafterRepository _guestDrafterRepository = guestDrafterRepository;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly IMovieRepository _guestDraftMovieRepository = guestDraftMovieRepository;

  public async Task<Result> Handle(PlayPickCommand request, CancellationToken cancellationToken)
  {
    var guestDraft = await _guestDraftRepository.GetByPublicIdForGameplayAsync(
      request.GuestDraftPublicId,
      cancellationToken
    );

    if (guestDraft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.GuestDraftPublicId));
    }

    var caller = await _usersApi.GetUserByPublicId(request.CallerUserPublicId, cancellationToken);

    if (caller is null)
    {
      return Result.Failure(UserPublicApiErrors.PublicIdNotFound(request.CallerUserPublicId));
    }

    var callerDrafter = await _guestDrafterRepository.GetByUserIdAsync(
      caller.UserId,
      cancellationToken
    );

    if (callerDrafter is null)
    {
      return Result.Failure(DrafterErrors.NotFoundForUser(caller.UserId));
    }

    var participant = guestDraft.FindByParticipantRef(Participant.From(callerDrafter.Id));

    if (participant is null)
    {
      return Result.Failure(DraftErrors.CallerNotAParticipant);
    }

    var movie = await _guestDraftMovieRepository.GetByPublicIdAsync(
      request.MoviePublicId,
      cancellationToken
    );

    if (movie is null)
    {
      return Result.Failure(DraftErrors.MovieNotFound(request.MoviePublicId));
    }

    // Every guest draft is hostless -- GuestDraft.PlayPick can derive "the other
    // participant" itself when there's exactly one candidate, but with more than
    // one it needs a true random draw, which doesn't belong in a deterministic
    // domain method -- same reasoning as canonical PlayPickCommandHandler's
    // RandomNumberGenerator usage.
    DraftParticipantId? explicitRevealRecipientId = null;

    var others = guestDraft.Participants.Where(p => p.Id != participant.Id).ToList();

    if (others.Count > 1)
    {
      explicitRevealRecipientId = others[RandomNumberGenerator.GetInt32(others.Count)].Id;
    }

    var result = guestDraft.PlayPick(
      moviePublicId: request.MoviePublicId,
      movieId: movie.Id,
      position: request.Position,
      playOrder: request.PlayOrder,
      participantId: participant.Id.Value,
      actedByPublicId: callerDrafter.PublicId,
      explicitRevealRecipientId: explicitRevealRecipientId
    );

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _guestDraftRepository.Update(guestDraft);
    return Result.Success();
  }
}

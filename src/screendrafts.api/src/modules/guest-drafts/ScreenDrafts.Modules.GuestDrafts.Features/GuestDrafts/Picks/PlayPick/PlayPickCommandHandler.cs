namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.PlayPick;

internal sealed class PlayPickCommandHandler(
  IGuestDraftRepository guestDraftRepository,
  IGuestDrafterRepository guestDrafterRepository,
  IUsersApi usersApi,
  IGuestDraftMovieRepository guestDraftMovieRepository
) : ICommandHandler<PlayPickCommand>
{
  private readonly IGuestDraftRepository _guestDraftRepository = guestDraftRepository;
  private readonly IGuestDrafterRepository _guestDrafterRepository = guestDrafterRepository;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly IGuestDraftMovieRepository _guestDraftMovieRepository =
    guestDraftMovieRepository;

  public async Task<Result> Handle(PlayPickCommand request, CancellationToken cancellationToken)
  {
    var guestDraft = await _guestDraftRepository.GetByPublicIdForGameplayAsync(
      request.GuestDraftPublicId,
      cancellationToken
    );

    if (guestDraft is null)
    {
      return Result.Failure(GuestDraftErrors.NotFound(request.GuestDraftPublicId));
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
      return Result.Failure(GuestDrafterErrors.NotFoundForUser(caller.UserId));
    }

    var participant = guestDraft.FindByParticipantRef(GuestParticipant.From(callerDrafter.Id));

    if (participant is null)
    {
      return Result.Failure(GuestDraftErrors.CallerNotAParticipant);
    }

    var movie = await _guestDraftMovieRepository.GetByPublicIdAsync(
      request.MoviePublicId,
      cancellationToken
    );

    if (movie is null)
    {
      return Result.Failure(GuestDraftErrors.MovieNotFound(request.MoviePublicId));
    }

    // Every guest draft is hostless -- GuestDraft.PlayPick can derive "the other
    // participant" itself when there's exactly one candidate, but with more than
    // one it needs a true random draw, which doesn't belong in a deterministic
    // domain method -- same reasoning as canonical PlayPickCommandHandler's
    // RandomNumberGenerator usage.
    GuestDraftParticipantId? explicitRevealRecipientId = null;

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

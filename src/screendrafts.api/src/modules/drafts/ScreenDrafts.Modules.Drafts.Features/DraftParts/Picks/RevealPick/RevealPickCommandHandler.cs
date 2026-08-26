namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.RevealPick;

internal sealed class RevealPickCommandHandler(
  IDraftPartRepository draftPartRepository,
  IPickRepository pickRepository,
  IHostRepository hostRepository,
  IPersonRepository personRepository,
  IUsersApi usersApi,
  ISeriesPolicyProvider seriesPolicyProvider,
  IDrafterRepository drafterRepository
) : ICommandHandler<RevealPickCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IPickRepository _pickRepository = pickRepository;
  private readonly IHostRepository _hostRepository = hostRepository;
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly ISeriesPolicyProvider _seriesPolicyProvider = seriesPolicyProvider;
  private readonly IDrafterRepository _drafterRepository = drafterRepository;

  public async Task<Result> Handle(RevealPickCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdWithHostsAsync(
      request.DraftPartId,
      cancellationToken
    );

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));
    }

    var pick = await _pickRepository.GetByDraftPartIdAndPlayOrderAsync(
      id: draftPart.Id,
      playOrder: request.PlayOrder,
      cancellationToken: cancellationToken
    );

    if (pick is null)
    {
      return Result.Failure(DraftPartErrors.PickNotFound(request.PlayOrder));
    }

    var user = await _usersApi.GetUserByPublicId(request.UserPublicId, cancellationToken);

    if (user is null)
    {
      return Result.Failure(UserPublicApiErrors.PublicIdNotFound(request.UserPublicId));
    }

    var person = await _personRepository.GetByUserIdAsync(user.UserId, cancellationToken);

    if (person is null)
    {
      return Result.Failure(PersonErrors.NotFoundForUser(request.UserPublicId));
    }

    // actedByPublicId records who physically performed the reveal — a Host's public id
    // for a hosted draft part, a Drafter's for a hostless one. Resolved by whichever
    // branch below actually applies, since exactly one of them runs.
    string actedByPublicId;

    if (draftPart.IsHostless)
    {
      var drafter = await _drafterRepository.GetByPersonPublicIdAsync(
        person.PublicId,
        cancellationToken
      );

      if (drafter is null)
      {
        return Result.Failure(DrafterErrors.NotFoundForPerson(person.PublicId));
      }

      var callerParticipant = new Participant(drafter.Id.Value, ParticipantKind.Drafter);

      if (!pick.IsRevealAuthorized(callerParticipant))
      {
        return Result.Failure(DraftPartErrors.OnlyDesignatedRecipientCanRevealPick);
      }

      actedByPublicId = drafter.PublicId;
    }
    else
    {
      var host = await _hostRepository.GetByPersonPublicIdAsync(person.PublicId, cancellationToken);

      if (host is null)
      {
        return Result.Failure(HostErrors.NotFoundForPerson(person.PublicId));
      }

      if (!draftPart.IsPrimaryHost(host.PublicId))
      {
        return Result.Failure(DraftPartErrors.OnlyPrimaryHostCanRevealPicks);
      }

      actedByPublicId = host.PublicId;
    }
    var series = await _seriesPolicyProvider.GetSeriesAsyc(draftPart.SeriesId, cancellationToken);

    if (series is null)
    {
      return Result.Failure(SeriesErrors.SeriesNotFound(draftPart.SeriesId.Value));
    }

    var result = draftPart.RevealPick(
      playOrder: request.PlayOrder,
      actedByPublicId: actedByPublicId,
      canonicalPolicyValue: CanonicalPolicy.FromValue(series.CanonicalPolicy.Value)
    );

    if (result.IsFailure)
    {
      return result;
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}

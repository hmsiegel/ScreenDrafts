namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.RevealPick;

internal sealed class RevealPickCommandHandler(
  IDraftPartRepository draftPartRepository,
  IPickRepository pickRepository,
  IHostRepository hostRepository,
  IPersonRepository personRepository,
  IUsersApi usersApi,
  ISeriesPolicyProvider seriesPolicyProvider
) : ICommandHandler<RevealPickCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IPickRepository _pickRepository = pickRepository;
  private readonly IHostRepository _hostRepository = hostRepository;
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly ISeriesPolicyProvider _seriesPolicyProvider = seriesPolicyProvider;

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

    var host = await _hostRepository.GetByPersonPublicIdAsync(person.PublicId, cancellationToken);

    if (host is null)
    {
      return Result.Failure(HostErrors.NotFoundForPerson(person.PublicId));
    }

    if (!draftPart.IsPrimaryHost(host.PublicId))
    {
      return Result.Failure(DraftPartErrors.OnlyPrimaryHostCanRevealPicks);
    }

    var series = await _seriesPolicyProvider.GetSeriesAsyc(draftPart.SeriesId, cancellationToken);

    if (series is null)
    {
      return Result.Failure(SeriesErrors.SeriesNotFound(draftPart.SeriesId.Value));
    }

    var result = draftPart.RevealPick(
      playOrder: request.PlayOrder,
      actedByPublicId: host.PublicId,
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

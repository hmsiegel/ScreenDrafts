using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Repositories;
using ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.ApplyCommissionerOverride;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.ApplyCommissionerOverride;

internal sealed class ApplyCommissionerOverrideCommandHandler(
  IDraftRepository guestDraftRepository,
  IUsersApi usersApi
) : ICommandHandler<ApplyCommissionerOverrideCommand>
{
  private readonly IDraftRepository _guestDraftRepository = guestDraftRepository;
  private readonly IUsersApi _usersApi = usersApi;

  public async Task<Result> Handle(
    ApplyCommissionerOverrideCommand request,
    CancellationToken cancellationToken
  )
  {
    var guestDraft = await _guestDraftRepository.GetByPublicIdForGameplayAsync(
      request.GuestDraftPublicId,
      cancellationToken
    );

    if (guestDraft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.GuestDraftPublicId));
    }

    var pick = guestDraft.Picks.FirstOrDefault(p => p.PlayOrder == request.PlayOrder);

    if (pick is null)
    {
      return Result.Failure(DraftErrors.PickNotFoundByPlayOrder(request.PlayOrder));
    }

    var caller = await _usersApi.GetUserByPublicId(request.CallerUserPublicId, cancellationToken);

    if (caller is null)
    {
      return Result.Failure(UserPublicApiErrors.PublicIdNotFound(request.CallerUserPublicId));
    }

    if (caller.UserId != guestDraft.OwnerUserId)
    {
      return Result.Failure(DraftErrors.OnlyOwnerCanPerformThisAction);
    }

    var result = guestDraft.ApplyCommissionerOverride(pick.Id);

    if (result.IsFailure)
    {
      return result;
    }

    _guestDraftRepository.Update(guestDraft);
    return Result.Success();
  }
}

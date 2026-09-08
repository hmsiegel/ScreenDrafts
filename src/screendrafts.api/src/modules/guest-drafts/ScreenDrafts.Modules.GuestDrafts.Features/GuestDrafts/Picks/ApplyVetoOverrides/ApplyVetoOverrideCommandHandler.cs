using ScreenDrafts.Modules.GuestDrafts.Domain.Drafters;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Enums;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Repositories;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Picks.ApplyVetoOverrides;

internal sealed class ApplyVetoOverrideCommandHandler(
  IDraftRepository guestDraftRepository,
  IDrafterRepository guestDrafterRepository,
  IUsersApi usersApi
) : ICommandHandler<ApplyVetoOverrideCommand>
{
  private readonly IDraftRepository _guestDraftRepository = guestDraftRepository;
  private readonly IDrafterRepository _guestDrafterRepository = guestDrafterRepository;
  private readonly IUsersApi _usersApi = usersApi;

  public async Task<Result> Handle(
    ApplyVetoOverrideCommand request,
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

    var by = guestDraft.FindByParticipantRef(Participant.From(callerDrafter.Id));

    if (by is null)
    {
      return Result.Failure(DraftErrors.CallerNotAParticipant);
    }

    // Type/Status checks happen before the pick lookup, mirroring
    // GuestDraft.ApplyVetoOverride's own precedence -- same reasoning as the
    // handler-ordering fix applied to UndoVeto/RevealPick.
    if (guestDraft.GuestDraftType == DraftType.Standard)
    {
      return Result.Failure(DraftErrors.VetoOverridesNotAllowedForThisDraftType);
    }

    if (guestDraft.GuestDraftStatus != DraftStatus.InProgress)
    {
      return Result.Failure(DraftErrors.DraftNotStarted);
    }

    var pick = guestDraft.Picks.FirstOrDefault(p => p.PlayOrder == request.PlayOrder);

    if (pick is null)
    {
      return Result.Failure(DraftErrors.PickNotFoundByPlayOrder(request.PlayOrder));
    }

    var result = guestDraft.ApplyVetoOverride(
      pick.Id,
      by.Id.Value,
      callerDrafter.PublicId,
      request.Note
    );

    if (result.IsFailure)
    {
      return result;
    }

    _guestDraftRepository.Update(guestDraft);
    return Result.Success();
  }
}

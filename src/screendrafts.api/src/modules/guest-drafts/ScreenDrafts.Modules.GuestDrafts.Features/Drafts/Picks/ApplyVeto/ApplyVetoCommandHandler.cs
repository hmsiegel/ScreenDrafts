using ScreenDrafts.Modules.GuestDrafts.Domain.Drafters;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Repositories;
using ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.ApplyVeto;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Picks.ApplyVeto;

internal sealed class ApplyVetoCommandHandler(
  IDraftRepository guestDraftRepository,
  IDrafterRepository guestDrafterRepository,
  IUsersApi usersApi
) : ICommandHandler<ApplyVetoCommand>
{
  private readonly IDraftRepository _guestDraftRepository = guestDraftRepository;
  private readonly IDrafterRepository _guestDrafterRepository = guestDrafterRepository;
  private readonly IUsersApi _usersApi = usersApi;

  public async Task<Result> Handle(ApplyVetoCommand request, CancellationToken cancellationToken)
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

    var callerDrafter = await _guestDrafterRepository.GetByUserIdAsync(
      caller.UserId,
      cancellationToken
    );

    if (callerDrafter is null)
    {
      return Result.Failure(DrafterErrors.NotFoundForUser(caller.UserId));
    }

    var issuer = guestDraft.FindByParticipantRef(Participant.From(callerDrafter.Id));

    if (issuer is null)
    {
      return Result.Failure(DraftErrors.CallerNotAParticipant);
    }

    var result = guestDraft.ApplyVeto(
      pick.Id,
      issuer.Id.Value,
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

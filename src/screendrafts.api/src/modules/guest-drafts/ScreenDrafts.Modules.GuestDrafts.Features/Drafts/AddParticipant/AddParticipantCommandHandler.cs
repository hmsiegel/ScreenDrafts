using ScreenDrafts.Modules.GuestDrafts.Domain.Drafters;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Repositories;
using ScreenDrafts.Modules.GuestDrafts.Features.Drafts.AddParticipant;

namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.AddParticipant;

internal sealed class AddParticipantCommandHandler(
  IDraftRepository guestDraftRepository,
  IDrafterRepository guestDrafterRepository,
  IUsersApi usersApi
) : ICommandHandler<AddParticipantCommand>
{
  private readonly IDraftRepository _guestDraftRepository = guestDraftRepository;
  private readonly IDrafterRepository _guestDrafterRepository = guestDrafterRepository;
  private readonly IUsersApi _usersApi = usersApi;

  public async Task<Result> Handle(
    AddParticipantCommand request,
    CancellationToken cancellationToken
  )
  {
    var guestDraft = await _guestDraftRepository.GetByPublicIdWithParticipantsAsync(
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

    if (caller.UserId != guestDraft.OwnerUserId)
    {
      return Result.Failure(DraftErrors.OnlyOwnerCanPerformThisAction);
    }

    var guestDrafter = await _guestDrafterRepository.GetByPublicIdAsync(
      request.GuestDrafterPublicId,
      cancellationToken
    );

    if (guestDrafter is null)
    {
      return Result.Failure(DrafterErrors.NotFound(request.GuestDrafterPublicId));
    }

    var participant = Participant.From(guestDrafter.Id);

    // Computed here, not assumed: this tells us whether the GuestDrafter being
    // added IS the owner (adding themselves), not just whether the caller
    // happens to be the owner (already confirmed above -- AddParticipant is
    // owner-only regardless of who's being added).
    var isOwner = guestDrafter.UserId == guestDraft.OwnerUserId;

    var result = guestDraft.AddParticipant(participant, isOwner);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _guestDraftRepository.Update(guestDraft);
    return Result.Success();
  }
}

using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Enums;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Helpers;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Repositories;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.UpdateGuestDraft;

internal sealed class UpdateGuestDraftCommandHandler(
  IDraftRepository guestDraftRepository,
  IUsersApi usersApi,
  IPublicIdGenerator publicIdGenerator
) : ICommandHandler<UpdateGuestDraftCommand>
{
  private readonly IDraftRepository _guestDraftRepository = guestDraftRepository;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result> Handle(
    UpdateGuestDraftCommand request,
    CancellationToken cancellationToken
  )
  {
    // Switched from the cheap GetByPublicIdAsync to the full gameplay-graph
    // load -- changing Type needs GameBoard/Positions/Participants loaded to
    // correctly clear/revoke/rebuild the board.
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

    if (caller.UserId != guestDraft.OwnerUserId)
    {
      return Result.Failure(DraftErrors.OnlyOwnerCanPerformThisAction);
    }

    if (!string.IsNullOrWhiteSpace(request.Title))
    {
      var titleResult = guestDraft.SetTitle(request.Title);

      if (titleResult.IsFailure)
      {
        return titleResult;
      }
    }

    if (request.DraftDate.HasValue)
    {
      guestDraft.SetDraftDate(request.DraftDate);
    }

    if (!string.IsNullOrWhiteSpace(request.Type))
    {
      if (!DraftType.TryFromName(request.Type, ignoreCase: true, out var newType))
      {
        return Result.Failure(DraftErrors.InvalidType(request.Type));
      }

      if (newType != guestDraft.GuestDraftType)
      {
        var changeTypeResult = guestDraft.ChangeType(newType);

        if (changeTypeResult.IsFailure)
        {
          return changeTypeResult;
        }

        // Board was just cleared by ChangeType -- must be redone for the new
        // type. Same logic CreateGuestDraftCommandHandler uses; duplicated
        // here rather than extracted, matching this module's existing
        // preference for a little repetition over a shared utility class
        // that doesn't otherwise exist in this feature set.
        Result boardResult;

        if (GameBoardTemplates.IsFixed(newType))
        {
          boardResult = guestDraft.UseFixedBoardLayout(_ =>
            _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.GuestDraftPosition)
          );
        }
        else
        {
          if (request.NumberOfPicks is not { } numberOfPicks || numberOfPicks < 1)
          {
            return Result.Failure(DraftErrors.NumberOfPicksMustBeGreaterThanZero);
          }

          var coverageResult = GuestDraftPositionCoverage.Validate(
            [.. request.Positions.Select(p => p.Picks)],
            numberOfPicks
          );

          if (coverageResult.IsFailure)
          {
            return Result.Failure(coverageResult.Errors);
          }

          var positions = request
            .Positions.Select(p =>
              (p.Name, p.Picks, p.HasBonusVeto, p.HasBonusVetoOverride, p.HasBonusFungibleToken)
            )
            .ToList();

          boardResult = guestDraft.SetCustomPositions(
            positions,
            _ => _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.GuestDraftPosition)
          );
        }

        if (boardResult.IsFailure)
        {
          return boardResult;
        }
      }
    }

    _guestDraftRepository.Update(guestDraft);
    return Result.Success();
  }
}

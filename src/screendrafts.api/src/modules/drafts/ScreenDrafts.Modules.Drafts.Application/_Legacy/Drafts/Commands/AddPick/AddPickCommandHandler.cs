using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;
using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Repositories;
using ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;

namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.AddPick;

internal sealed class AddPickCommandHandler(
  IDraftRepository draftsRepository,
  IPicksRepository picksRepository)
  : ICommandHandler<AddPickCommand, Guid>
{
  private readonly IDraftRepository _draftsRepository = draftsRepository;
  private readonly IPicksRepository _picksRepository = picksRepository;

  public async Task<Result<Guid>> Handle(AddPickCommand request, CancellationToken cancellationToken)
  {
    var draftPartId = DraftPartId.Create(request.DraftPartId);

    var draftPart = await _draftsRepository.GetDraftPartByIdAsync(draftPartId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure<Guid>(DraftErrors.DraftPartNotFound(request.DraftPartId));
    }

    var draft = await _draftsRepository.GetDraftByDraftPartId(draftPartId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<Guid>(DraftErrors.NotFound(draft!.Id.Value));
    }

    var drafter = draftPart.Drafters.FirstOrDefault(d => d.Id.Value == request.DrafterId);
    var drafterTeam = draftPart.DrafterTeams.FirstOrDefault(dt => dt.Id.Value == request.DrafterId);

    if (drafter is null && drafterTeam is null)
    {
      return Result.Failure<Guid>(DrafterErrors.NotFound(request.DrafterId, request.DrafterTeamId));
    }

    var movie = await _draftsRepository.GetMovieByIdAsync(request.MovieId, cancellationToken);

    if (movie is null)
    {
      return Result.Failure<Guid>(DraftErrors.MovieNotFound(request.MovieId));
    }

    var pick = Pick.Create(request.Position, movie, drafter, drafterTeam, draftPart, request.PlayOrder);

    if (pick.IsFailure)
    {
      return Result.Failure<Guid>(pick.Errors);
    }

    var result = draftPart.AddPick(pick.Value);

    if (result.IsFailure)
    {
      return Result.Failure<Guid>(result.Errors);
    }

    _picksRepository.Add(pick.Value);

    return Result.Success(pick.Value.Id.Value);
  }
}

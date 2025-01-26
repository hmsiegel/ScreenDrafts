using ScreenDrafts.Modules.Drafts.Domain.Drafters.Errors;
using ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;

namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddPick;

internal sealed class AddPickCommandHandler(
  IDraftsRepository draftsRepository,
  IUnitOfWork unitOfWork) 
  : ICommandHandler<AddPickCommand>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  public async Task<Result> Handle(AddPickCommand request, CancellationToken cancellationToken)
  {
    var draft = await _draftsRepository.GetByIdAsync(request.DraftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<Guid>(DraftErrors.NotFound(request.DraftId));
    }

    var drafter = draft.Drafters.FirstOrDefault(d => d.Id.Value == request.DrafterId);

    if (drafter is null)
    {
      return Result.Failure<Guid>(DrafterErrors.NotFound(drafter!.Id.Value));
    }

    var movie = await _draftsRepository.GetMovieByIdAsync(request.MovieId, cancellationToken);

    if (movie is null)
    {
      return Result.Failure<Guid>(DraftErrors.MovieNotFound(movie!.Id.Value));
    }

    draft.AddPick(request.Position, movie, drafter);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}

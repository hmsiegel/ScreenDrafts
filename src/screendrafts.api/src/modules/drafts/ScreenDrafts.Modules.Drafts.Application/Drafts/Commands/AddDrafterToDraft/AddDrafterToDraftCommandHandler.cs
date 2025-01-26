using ScreenDrafts.Modules.Drafts.Domain.Drafters.Errors;
using ScreenDrafts.Modules.Drafts.Domain.Drafters.Repositories;
using ScreenDrafts.Modules.Drafts.Domain.Drafts.Repositories;

namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddDrafterToDraft;

internal sealed class AddDrafterToDraftCommandHandler(
  IDraftsRepository draftRepository,
  IDraftersRepository drafterRepository,
  IUnitOfWork unitOfWork)
  : ICommandHandler<AddDrafterToDraftCommand>
{
  private readonly IDraftsRepository _draftRepository = draftRepository;
  private readonly IDraftersRepository _drafterRepository = drafterRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public async Task<Result> Handle(AddDrafterToDraftCommand request, CancellationToken cancellationToken)
  {
    var draft = await _draftRepository.GetByIdAsync(request.DraftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<Draft>(DraftErrors.NotFound(request.DraftId));
    }

    var drafter = await _drafterRepository.GetByIdAsync(request.DrafterId, cancellationToken);

    if (drafter is null)
    {
      return Result.Failure<Drafter>(DrafterErrors.NotFound(request.DrafterId));
    }

    draft.AddDrafter(drafter);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
    return Result.Success();
  }
}

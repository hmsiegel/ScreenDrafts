using FluentResults;

namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreateDraft;

internal sealed class CreateDraftCommandHandler(IDraftsRepository draftsRepository, IUnitOfWork unitOfWork)
  : ICommandHandler<CreateDraftCommand, Guid>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public async Task<Result<Guid>> Handle(CreateDraftCommand request, CancellationToken cancellationToken)
  {
    var draft = Draft.Create(
      request.Title,
      request.DraftType,
      request.NumberOfDrafters,
      request.NumberOfCommissioners,
      request.NumberOfMovies);

    _draftsRepository.Add(draft);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
    return draft.Id;
  }
}

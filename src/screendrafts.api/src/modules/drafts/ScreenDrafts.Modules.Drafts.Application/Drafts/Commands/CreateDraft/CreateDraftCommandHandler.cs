namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreateDraft;

internal sealed class CreateDraftCommandHandler(IDraftsRepository draftsRepository, IUnitOfWork unitOfWork)
  : ICommandHandler<CreateDraftCommand, Guid>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public async Task<Result<Guid>> Handle(CreateDraftCommand request, CancellationToken cancellationToken)
  {
    var result = Draft.Create(
      new Title(request.Title),
      request.DraftType,
      request.TotalPicks,
      request.TotalDrafters,
      request.TotalHosts,
      request.DraftStatus);

    if (result.IsFailure)
    {
      return Result.Failure<Guid>(result.Error!);
    }

    var draft = result.Value;

    _draftsRepository.Add(draft);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
    return draft.Id.Value;
  }
}

namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.UpdateScreamDrafts;

internal sealed class UpdateScreamDraftsCommandHandler(
    IDraftsRepository draftRepository) : ICommandHandler<UpdateScreamDraftsCommand>
{
  private readonly IDraftsRepository _draftsRepository = draftRepository;

  public async Task<Result> Handle(UpdateScreamDraftsCommand request, CancellationToken cancellationToken)
  {
    var draftId = DraftId.Create(request.DraftId);
    var draft = await _draftsRepository.GetByIdAsync(draftId, cancellationToken);
    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.DraftId));
    }
    draft.SetScreamDrafts(request.IsScreamDrafts);
    _draftsRepository.Update(draft);
    return Result.Success();
  }
}

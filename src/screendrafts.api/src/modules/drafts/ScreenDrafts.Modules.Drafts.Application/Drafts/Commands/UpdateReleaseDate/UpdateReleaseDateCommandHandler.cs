namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.UpdateReleaseDate;

internal sealed class UpdateReleaseDateCommandHandler(
  IDraftsRepository draftsRepository)
  : ICommandHandler<UpdateReleaseDateCommand>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;

  public async Task<Result> Handle(UpdateReleaseDateCommand request, CancellationToken cancellationToken)
  {
    var draftId = DraftId.Create(request.DraftId);
    var draft = await _draftsRepository.GetByIdAsync(draftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DrafterErrors.NotFound(request.DraftId));
    }

    var releaseDate = DraftReleaseDate.Create(draftId, request.ReleaseDate);

    draft.AddReleaseDate(releaseDate);

    _draftsRepository.Update(draft);

    return Result.Success();
  }
}

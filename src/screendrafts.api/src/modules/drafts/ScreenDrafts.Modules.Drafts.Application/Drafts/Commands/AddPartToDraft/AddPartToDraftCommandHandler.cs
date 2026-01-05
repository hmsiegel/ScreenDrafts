namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddPartToDraft;

internal sealed class AddPartToDraftCommandHandler(IDraftsRepository draftsRepository) : ICommandHandler<AddPartToDraftCommand>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;

  public async Task<Result> Handle(AddPartToDraftCommand request, CancellationToken cancellationToken)
  {
    var draftId = DraftId.Create(request.DraftId);

    var draft = await _draftsRepository.GetByIdAsync(draftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.DraftId));
    }

    draft.AddPart(
      partIndex: request.PartIndex,
      totalPicks: request.TotalPicks,
      totalDrafters: request.TotalDrafters,
      totalDrafterTeams: request.TotalDrafterTeams,
      totalHosts: request.TotalHosts);

    _draftsRepository.Update(draft);

    return await Task.FromResult(Result.Success());
  }
}

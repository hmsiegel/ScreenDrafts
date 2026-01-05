namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddDrafterTeamToDraft;

internal sealed class AddDrafterTeamToDraftCommandHandler(
    IDraftsRepository draftRepository,
    IDraftersRepository draftersRepository)
    : ICommandHandler<AddDrafterTeamToDraftCommand>
{
  private readonly IDraftsRepository _draftsRepository = draftRepository;
  private readonly IDraftersRepository _draftersRepository = draftersRepository;
  public async Task<Result> Handle(AddDrafterTeamToDraftCommand request, CancellationToken cancellationToken)
  {
    var draftPartId = DraftPartId.Create(request.DraftPartId);

    var draft = await _draftsRepository.GetDraftByDraftPartId(draftPartId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<Guid>(DraftErrors.NotFound(draft!.Id.Value));
    }

    var draftPart = await _draftsRepository.GetDraftPartByIdAsync(draftPartId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure<Guid>(DraftErrors.DraftPartNotFound(request.DraftPartId));
    }

    var drafterTeamId = DrafterTeamId.Create(request.DrafterTeamId);

    var drafterTeam = await _draftersRepository.GetByIdAsync(drafterTeamId, cancellationToken);

    if (drafterTeam is null)
    {
      return Result.Failure<Guid>(DrafterTeamErrors.NotFound(request.DrafterTeamId));
    }

    draftPart.AddDrafterTeam(drafterTeam);

    _draftsRepository.Update(draft);

    return Result.Success(drafterTeam.Id.Value);
  }
}

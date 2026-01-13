namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ClearCampaign;

internal sealed class CommandHandler(
  IDraftsRepository draftsRepository)
  : Common.Features.Abstractions.Messaging.ICommandHandler<Command>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;

  public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
  {
    var draft = await _draftsRepository.GetDraftByPublicId(request.DraftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.DraftId));
    }

    draft.ClearCampaign();

    _draftsRepository.Update(draft);

    return Result.Success();
  }
}

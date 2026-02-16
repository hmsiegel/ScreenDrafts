namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetDraft;

internal sealed class GetDraftQueryHandler(IDraftRepository draftsRepository)
    : IQueryHandler<GetDraftQuery, Guid>
{
  private readonly IDraftRepository _draftsRepository = draftsRepository;

  public async Task<Result<Guid>> Handle(GetDraftQuery request, CancellationToken cancellationToken)
  {
    var draft = await _draftsRepository.GetDraftWithDetailsAsync(DraftId.Create(request.DraftId), cancellationToken);

    if (draft is null)
    {
      return Result.Failure<Guid>(DraftErrors.NotFound(request.DraftId));
    }

    return Result.Success(draft.Id.Value);
  }
}

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Delete;

internal sealed class DeleteDraftCommandHandler(
  IDraftRepository draftsRepository,
  IDateTimeProvider dateTimeProvider
) : ICommandHandler<DeleteDraftCommand>
{
  private readonly IDraftRepository _draftsRepository = draftsRepository;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result> Handle(DeleteDraftCommand request, CancellationToken cancellationToken)
  {
    var draft = await _draftsRepository.GetDraftByPublicId(request.PublicId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.PublicId));
    }

    var result = draft.SoftDelete(_dateTimeProvider.UtcNow);

    if (result.IsFailure)
    {
      return result;
    }

    _draftsRepository.Update(draft);

    return Result.Success();
  }
}

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetEpisodeNumber;

internal sealed class SetEpisodeNumberCommandHandler : ICommandHandler<SetEpisodeNumberCommand>
{
  private readonly IDraftRepository _draftRepository;

  public SetEpisodeNumberCommandHandler(IDraftRepository draftRepository)
  {
    _draftRepository = draftRepository;
  }

  public async Task<Result> Handle(SetEpisodeNumberCommand request, CancellationToken cancellationToken)
  {
    var draft = await _draftRepository.GetByPublicIdAsync(request.DraftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.DraftId));
    }

    var result = draft.UpsertChannelRelease(request.ReleaseChannel, request.EpisodeNumber);

    if (result.IsFailure)
    {
      return Result.Failure(result.Error!);
    }

    return Result.Success();
  }
}

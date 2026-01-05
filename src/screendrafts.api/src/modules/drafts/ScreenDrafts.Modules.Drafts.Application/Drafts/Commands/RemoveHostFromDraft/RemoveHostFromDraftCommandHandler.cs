namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.RemoveHostFromDraft;

internal sealed class RemoveHostFromDraftCommandHandler(
  IDraftsRepository draftsRepository,
  IHostsRepository hostsRepository)
  : ICommandHandler<RemoveHostFromDraftCommand>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly IHostsRepository _hostsRepository = hostsRepository;

  public async Task<Result> Handle(RemoveHostFromDraftCommand request, CancellationToken cancellationToken)
  {
    var draftId = DraftId.Create(request.DraftId);
    var hostId = HostId.Create(request.HostId);

    var draft = await _draftsRepository.GetByIdAsync(draftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<Guid>(DraftErrors.NotFound(request.DraftId));
    }

    var host = await _hostsRepository.GetHostByIdAsync(hostId, cancellationToken);

    if (host is null)
    {
      return Result.Failure<Guid>(HostErrors.NotFound(request.HostId));
    }

    var result = draft.RemoveHost(host);

    if (result.IsFailure)
    {
      return Result.Failure<Guid>(result.Errors);
    }

    _draftsRepository.Update(draft);

    return Result.Success(hostId.Value);
  }
}

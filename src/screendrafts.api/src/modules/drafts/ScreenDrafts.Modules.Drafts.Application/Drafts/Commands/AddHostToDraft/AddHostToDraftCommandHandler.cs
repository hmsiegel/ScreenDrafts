namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddHostToDraft;

internal sealed class AddHostToDraftCommandHandler(
  IDraftsRepository draftsRepository,
  IUnitOfWork unitOfWork,
  IHostsRepository hostsRepository)
  : ICommandHandler<AddHostToDraftCommand, Guid>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  private readonly IHostsRepository _hostsRepository = hostsRepository;

  public async Task<Result<Guid>> Handle(AddHostToDraftCommand request, CancellationToken cancellationToken)
  {
    var draftId = DraftId.Create(request.DraftId);

    var draft = await _draftsRepository.GetByIdAsync(draftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<Guid>(DraftErrors.NotFound(request.DraftId));
    }

    var hostId = HostId.Create(request.HostId);

    var host = await _hostsRepository.GetHostByIdAsync(hostId, cancellationToken);

    if (host is null)
    {
      return Result.Failure<Guid>(HostErrors.NotFound(request.HostId));
    }

    draft.AddHost(host);

    _draftsRepository.Update(draft);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(host.Id.Value);
  }
}

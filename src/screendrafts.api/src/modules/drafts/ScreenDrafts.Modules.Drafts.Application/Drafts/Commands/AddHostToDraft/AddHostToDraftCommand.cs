namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddHostToDraft;
public sealed record AddHostToDraftCommand(Guid DraftId, Guid HostId) : ICommand;

internal sealed class AddHostToDraftCommandHandler(
  IDraftsRepository draftsRepository,
  IUnitOfWork unitOfWork,
  IHostsRepository hostsRepository)
  : ICommandHandler<AddHostToDraftCommand>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  private readonly IHostsRepository _hostsRepository = hostsRepository;

  public async Task<Result> Handle(AddHostToDraftCommand request, CancellationToken cancellationToken)
  {
    var draftId = DraftId.Create(request.DraftId);

    var draft = await _draftsRepository.GetByIdAsync(draftId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<Draft>(DraftErrors.NotFound(request.DraftId));
    }

    var host = await _hostsRepository.GetHostByIdAsync(request.HostId, cancellationToken);

    if (host is null)
    {
      return Result.Failure<Host>(HostErrors.NotFound(request.HostId));
    }

    draft.AddHost(host);

    _draftsRepository.Update(draft);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}

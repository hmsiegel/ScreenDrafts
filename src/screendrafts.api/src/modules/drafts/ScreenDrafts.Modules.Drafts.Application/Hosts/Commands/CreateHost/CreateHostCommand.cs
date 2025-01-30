namespace ScreenDrafts.Modules.Drafts.Application.Hosts.Commands.CreateHost;
public sealed record CreateHostCommand(Guid UserId, string Name) : ICommand;

internal sealed class CreateHostCommandHandler(IHostsRepository hostRepository, IUnitOfWork unitOfWork) : ICommandHandler<CreateHostCommand>
{
  private readonly IHostsRepository _hostRepository = hostRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public async Task<Result> Handle(CreateHostCommand request, CancellationToken cancellationToken)
  {
    var host = Host.Create(request.UserId, request.Name);

    _hostRepository.AddHost(host.Value);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
    return Result.Success();
  }
}

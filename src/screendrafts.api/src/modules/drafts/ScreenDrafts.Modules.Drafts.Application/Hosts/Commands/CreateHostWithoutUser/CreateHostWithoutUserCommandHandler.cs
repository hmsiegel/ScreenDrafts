namespace ScreenDrafts.Modules.Drafts.Application.Hosts.Commands.CreateHostWithoutUser;

internal sealed class CreateHostWithoutUserCommandHandler(
  IHostsRepository hostRepository,
  IUnitOfWork unitOfWork)
  : ICommandHandler<CreateHostWithoutUserCommand, Guid>
{
  private readonly IHostsRepository _hostRepository = hostRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public async Task<Result<Guid>> Handle(CreateHostWithoutUserCommand request, CancellationToken cancellationToken)
  {
    var host = Host.Create(request.Name);

    if (host.IsFailure)
    {
      return Result.Failure<Guid>(HostErrors.CannotCreateHost);
    }

    _hostRepository.AddHost(host.Value);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return host.Value.Id.Value;
  }
}

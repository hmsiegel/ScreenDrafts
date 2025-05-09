namespace ScreenDrafts.Modules.Drafts.Application.Hosts.Commands.CreateHostWithoutUser;

internal sealed class CreateHostWithoutUserCommandHandler(
  IHostsRepository hostRepository)
  : ICommandHandler<CreateHostWithoutUserCommand, Guid>
{
  private readonly IHostsRepository _hostRepository = hostRepository;

  public async Task<Result<Guid>> Handle(CreateHostWithoutUserCommand request, CancellationToken cancellationToken)
  {
    var host = Host.Create(request.Name);

    if (host.IsFailure)
    {
      return await Task.FromResult(Result.Failure<Guid>(HostErrors.CannotCreateHost));
    }

    _hostRepository.AddHost(host.Value);

    return host.Value.Id.Value;
  }
}

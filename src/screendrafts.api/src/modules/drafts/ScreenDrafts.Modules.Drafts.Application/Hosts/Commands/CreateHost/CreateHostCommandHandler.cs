namespace ScreenDrafts.Modules.Drafts.Application.Hosts.Commands.CreateHost;

internal sealed class CreateHostCommandHandler(
  IHostsRepository hostRepository,
  IUsersApi usersApi) : ICommandHandler<CreateHostCommand, Guid>
{
  private readonly IHostsRepository _hostRepository = hostRepository;
  private readonly IUsersApi _usersApi = usersApi;

  public async Task<Result<Guid>> Handle(CreateHostCommand request, CancellationToken cancellationToken)
  {
    var user = await _usersApi.GetUserByIdAsync(request.UserId, cancellationToken);

    if (user is null)
    {
      return Result.Failure<Guid>(HostErrors.NotFound(user!.UserId));
    }

    var hostName = $"{user.FirstName} {user.MiddleName} {user.LastName}";

    var host = Host.Create(hostName, user.UserId);

    _hostRepository.AddHost(host.Value);

    return host.Value.Id.Value;
  }
}

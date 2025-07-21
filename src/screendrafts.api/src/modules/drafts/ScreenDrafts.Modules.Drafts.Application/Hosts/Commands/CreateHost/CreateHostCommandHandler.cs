namespace ScreenDrafts.Modules.Drafts.Application.Hosts.Commands.CreateHost;

internal sealed class CreateHostCommandHandler(
  IHostsRepository hostRepository,
  IPersonRepository personRepository) : ICommandHandler<CreateHostCommand, Guid>
{
  private readonly IHostsRepository _hostRepository = hostRepository;
  private readonly IPersonRepository _personRepository = personRepository;

  public async Task<Result<Guid>> Handle(CreateHostCommand request, CancellationToken cancellationToken)
  {
    var persondId = PersonId.Create(request.PersonId);

    var person = await _personRepository.GetByIdAsync(persondId, cancellationToken);

    if (person is null)
    {
      return Result.Failure<Guid>(PersonErrors.NotFound(request.PersonId));
    }

    var host = Host.Create(person);

    _hostRepository.AddHost(host.Value);

    return host.Value.Id.Value;
  }
}

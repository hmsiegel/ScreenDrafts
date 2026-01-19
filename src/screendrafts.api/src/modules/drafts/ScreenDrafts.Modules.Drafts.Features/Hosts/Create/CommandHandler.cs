namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Create;

internal sealed class CommandHandler(
  IHostsRepository hostsRepository,
  IPersonRepository personRepository,
  IPublicIdGenerator publicIdGenerator)
  : Common.Features.Abstractions.Messaging.ICommandHandler<Command, string>
{
  private readonly IHostsRepository _hostsRepository = hostsRepository;
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
  {
    var person = await _personRepository.GetByPublicIdAsync(
      request.PersonPublicId,
      cancellationToken);

    if (person is null)
    {
      return Result.Failure<string>(PersonErrors.NotFound(request.PersonPublicId));
    }

    // Make sure the Host with the person Id doesn't already exist.
    if (await _hostsRepository.HostExistsAsync(person.PublicId, cancellationToken))
    {
      return Result.Failure<string>(HostErrors.AlreadyExists(person.PublicId));
    }

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Host);

    var host = Host.Create(person, publicId);

    if (host.IsFailure)
    {
      return Result.Failure<string>(host.Errors);
    }

    _hostsRepository.Add(host.Value);
    return Result.Success(host.Value.PublicId);
  }
}

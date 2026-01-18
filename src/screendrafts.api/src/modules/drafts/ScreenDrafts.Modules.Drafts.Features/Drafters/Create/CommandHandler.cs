namespace ScreenDrafts.Modules.Drafts.Features.Drafters.Create;

internal sealed class CommandHandler(
  IPersonRepository personRepository,
  IPublicIdGenerator publicIdGenerator,
  IDraftersRepository draftersRepository)
  : Common.Features.Abstractions.Messaging.ICommandHandler<Command, string>
{
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;
  private readonly IDraftersRepository _draftersRepository = draftersRepository;

  public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
  {
    var person = await _personRepository.GetByPublicIdAsync(request.PersonId, cancellationToken);

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Drafter);

    if (person is null)
    {
      return Result.Failure<string>(PersonErrors.NotFound(request.PersonId));
    }

    var alreadyExists = await _draftersRepository.ExistsForPersonAsync(request.PersonId, cancellationToken);

    if (alreadyExists)
    {
      return Result.Failure<string>(DrafterErrors.AlreadyExistsForPerson(request.PersonId));
    }

    var drafterResult = Drafter.Create(person, publicId);

    if (drafterResult.IsFailure)
    {
      return Result.Failure<string>(drafterResult.Errors);
    }

    var drafter = drafterResult.Value;

    _draftersRepository.Add(drafter);

    return Result.Success(drafter.PublicId);
  }
}

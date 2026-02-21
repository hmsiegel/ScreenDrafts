namespace ScreenDrafts.Modules.Drafts.Features.Drafters.Create;

internal sealed class CreateDrafterCommandHandler(
  IPersonRepository personRepository,
  IPublicIdGenerator publicIdGenerator,
  IDrafterRepository draftersRepository)
  : ICommandHandler<CreateDrafterCommand, string>
{
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;
  private readonly IDrafterRepository _draftersRepository = draftersRepository;

  public async Task<Result<string>> Handle(CreateDrafterCommand CreateDrafterRequest, CancellationToken cancellationToken)
  {
    var person = await _personRepository.GetByPublicIdAsync(CreateDrafterRequest.PersonId, cancellationToken);

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Drafter);

    if (person is null)
    {
      return Result.Failure<string>(PersonErrors.NotFound(CreateDrafterRequest.PersonId));
    }

    var alreadyExists = await _draftersRepository.ExistsForPersonAsync(CreateDrafterRequest.PersonId, cancellationToken);

    if (alreadyExists)
    {
      return Result.Failure<string>(DrafterErrors.AlreadyExistsForPerson(CreateDrafterRequest.PersonId));
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




namespace ScreenDrafts.Modules.Drafts.Features.People.Create;

internal sealed class CreatePersonCommandHandler(
  IPersonRepository personRepository,
  IUsersApi usersApi,
  IPublicIdGenerator publicIdGenerator)
  : ICommandHandler<CreatePersonCommand, string>
{
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(CreatePersonCommand CreatePersonRequest, CancellationToken cancellationToken)
  {
    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Person);

    Result<Person> createResult;
    if (CreatePersonRequest.UserId is not null)
    {
      var user = await _usersApi.GetUserById(CreatePersonRequest.UserId.Value, cancellationToken);

      if (user is null)
      {
        return Result.Failure<string>(PersonErrors.NotFound(CreatePersonRequest.UserId.Value));
      }

      var firstName = string.IsNullOrWhiteSpace(CreatePersonRequest.FirstName) ? user.FirstName : CreatePersonRequest.FirstName;
      var lastName = string.IsNullOrWhiteSpace(CreatePersonRequest.LastName) ? user.LastName : CreatePersonRequest.LastName;

      createResult = Person.Create(
        publicId,
        firstName,
        lastName,
        CreatePersonRequest.UserId.Value);

    }
    else
    {
      createResult = Person.Create(
        publicId,
        CreatePersonRequest.FirstName,
        CreatePersonRequest.LastName);

    }

    if (createResult.IsFailure)
    {
      return Result.Failure<string>(createResult.Errors);
    }

    var person = createResult.Value;

    _personRepository.Add(person);

    return Result.Success(person.PublicId);
  }
}




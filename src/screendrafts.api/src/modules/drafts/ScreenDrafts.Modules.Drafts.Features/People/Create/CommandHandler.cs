namespace ScreenDrafts.Modules.Drafts.Features.People.Create;

internal sealed class CommandHandler(
  IPersonRepository personRepository,
  IUsersApi usersApi,
  IPublicIdGenerator publicIdGenerator)
  : Common.Features.Abstractions.Messaging.ICommandHandler<Command, string>
{
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
  {
    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Person);

    Result<Person> createResult;
    if (request.UserId is not null)
    {
      var user = await _usersApi.GetUserByPublicId(request.UserId.Value, cancellationToken);

      if (user is null)
      {
        return Result.Failure<string>(PersonErrors.NotFound(request.UserId.Value));
      }

      var firstName = string.IsNullOrWhiteSpace(request.FirstName) ? user.FirstName : request.FirstName;
      var lastName = string.IsNullOrWhiteSpace(request.LastName) ? user.LastName : request.LastName;

      createResult = Person.Create(
        publicId,
        firstName,
        lastName,
        request.UserId.Value);

    }
    else
    {
      createResult = Person.Create(
        publicId,
        request.FirstName,
        request.LastName);

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

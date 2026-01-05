namespace ScreenDrafts.Modules.Drafts.Application.People.Commands.CreatePerson;

internal sealed class CreatePersonCommandHandler(IPersonRepository personRepository, IUsersApi usersApi)
  : ICommandHandler<CreatePersonCommand, Guid>
{
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly IUsersApi _usersApi = usersApi;

  public async Task<Result<Guid>> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
  {
    Result<Person>? person = null;

    if (request.UserId is not null && !string.IsNullOrWhiteSpace(request.FirstName) && !string.IsNullOrWhiteSpace(request.LastName))
    {
      person = Person.Create(
        request.FirstName,
        request.LastName,
        request.UserId);
    }

    if (request.UserId is null && string.IsNullOrWhiteSpace(request.FirstName) && string.IsNullOrWhiteSpace(request.LastName))
    {
      return Result.Failure<Guid>(PersonErrors.CannotCreatePerson);
    }

    if (request.UserId is null)
    {
      ArgumentNullException.ThrowIfNull(request.FirstName);
      ArgumentNullException.ThrowIfNull(request.LastName);

      person = Person.Create(
        firstName: request.FirstName,
        lastName: request.LastName);
    }

    if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
    {
      var userId = request.UserId!.Value;

      var user = await _usersApi.GetUserByIdAsync(userId, cancellationToken);

      if (user is null)
      {
        return Result.Failure<Guid>(PersonErrors.NotFound(userId));
      }

      var firstName = user.FirstName;
      var lastName = user.LastName;

      person = Person.Create(
        firstName: firstName,
        lastName: lastName,
        userId: userId);
    }

    _personRepository.Add(person!.Value);
    return person!.Value.Id.Value;
  }
}

namespace ScreenDrafts.Modules.Drafts.Features.People.LinkUser;

internal sealed class LinkUserPersonCommandHandler(
  IPersonRepository personRepository,
  IUsersApi usersApi,
  IEventBus eventBus)
  : ICommandHandler<LinkUserPersonCommand>
{
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly IEventBus _eventBus = eventBus;

  public async Task<Result> Handle(LinkUserPersonCommand LinkUserPersonRequest, CancellationToken cancellationToken)
  {
    var person = await _personRepository.GetByPublicIdAsync(LinkUserPersonRequest.PublicId, cancellationToken);

    if (person is null)
    {
      return Result.Failure(PersonErrors.NotFound(LinkUserPersonRequest.PublicId));
    }

    var user = await _usersApi.GetUserById(LinkUserPersonRequest.UserId, cancellationToken);

    if (user is null)
    {
      return Result.Failure(PersonErrors.NotFound(LinkUserPersonRequest.UserId));
    }

    person.AssignUserId(LinkUserPersonRequest.UserId);

    await _eventBus.PublishAsync(
      new PersonCreatedForUserIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: DateTime.UtcNow,
        personId: person.Id.Value,
        personPublicId: person.PublicId,
        userId: user.UserId),
      cancellationToken: cancellationToken
    );

    return Result.Success();
  }
}




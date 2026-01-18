
namespace ScreenDrafts.Modules.Drafts.Features.People.LinkUser;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request>
{
  public override void Configure()
  {
    Post(PeopleRoutes.LinkUser);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.People_LinkUser)
      .WithTags(DraftsOpenApi.Tags.People)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(Features.Permissions.PersonUpdate);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var command = new Command
    {
      PublicId = req.PublicId,
      UserId = req.UserId
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

internal sealed record Request
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }
  public Guid UserId { get; init; }
}

internal sealed class Summary : Summary<Endpoint>
  {
  public Summary()
  {
    Summary = "Link a user to a person.";
    Description = "Link a user to a person in the system.";
    Response(StatusCodes.Status204NoContent, "The user was successfully linked to the person.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid.");
    Response(StatusCodes.Status401Unauthorized, "The user is not authenticated.");
    Response(StatusCodes.Status403Forbidden, "The user does not have permission to link a user to a person.");
  }
}

internal sealed record Command : Common.Features.Abstractions.Messaging.ICommand
{
  public required string PublicId { get; init; }
  public required Guid UserId { get; init; }
}

internal sealed class Validator : AbstractValidator<Command>
{
  public Validator()
  {
    RuleFor(x => x.PublicId)
      .NotEmpty().WithMessage("PublicId is required.")
      .Must(publicId => PublicIdGuards.IsValidWithPrefix(publicId, PublicIdPrefixes.Person))
      .WithMessage("PublicId is not valid or does not have the correct prefix.");

    RuleFor(x => x.UserId)
      .NotEmpty().WithMessage("UserId is required.");
  }
}

internal sealed class CommandHandler(
  IPersonRepository personRepository,
  IUsersApi usersApi,
  Common.Features.Abstractions.EventBus.IEventBus eventBus)
  : Common.Features.Abstractions.Messaging.ICommandHandler<Command>
{
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly Common.Features.Abstractions.EventBus.IEventBus _eventBus = eventBus;

  public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
  {
    var person = await _personRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);

    if (person is null)
    {
      return Result.Failure(PersonErrors.NotFound(request.PublicId));
    }

    var user = await _usersApi.GetUserById(request.UserId, cancellationToken);

    if (user is null)
    {
      return Result.Failure(PersonErrors.NotFound(request.UserId));
    }

    person.AssignUserId(request.UserId);

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

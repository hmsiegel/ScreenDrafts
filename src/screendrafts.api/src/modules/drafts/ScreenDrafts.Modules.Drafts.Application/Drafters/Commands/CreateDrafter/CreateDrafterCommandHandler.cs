namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.CreateDrafter;

internal sealed class CreateDrafterCommandHandler(
  IDraftersRepository drafterRepository,
  IPersonRepository personRepository)
  : ICommandHandler<CreateDrafterCommand, Guid>
{
  private readonly IDraftersRepository _drafterRepository = drafterRepository;
  private readonly IPersonRepository _personRepository = personRepository;

  public async Task<Result<Guid>> Handle(CreateDrafterCommand request, CancellationToken cancellationToken)
  {
    var personId = PersonId.Create(request.PersonId);

    var person = await _personRepository.GetByIdAsync(personId, cancellationToken);

    if (person is null)
    {
      return Result.Failure<Guid>(PersonErrors.NotFound(request.PersonId));
    }

    var drafter = Drafter.Create(person);

    _drafterRepository.Add(drafter!.Value);

    return drafter!.Value.Id.Value;
  }
}

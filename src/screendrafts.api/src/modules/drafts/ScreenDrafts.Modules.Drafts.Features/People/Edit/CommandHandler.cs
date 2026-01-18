namespace ScreenDrafts.Modules.Drafts.Features.People.Edit;

internal sealed class CommandHandler(IPersonRepository personRepository) : Common.Features.Abstractions.Messaging.ICommandHandler<Command>
{
  private readonly IPersonRepository _personRepository = personRepository;

  public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
  {
    var person = await _personRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);

    if (person is null)
    {
      return Result.Failure(PersonErrors.NotFound(request.PublicId));
    }

    var updateResult = person.Update(
      request.FirstName,
      request.LastName,
      request.DisplayName);

    if (updateResult.IsFailure)
    {
      return Result.Failure(updateResult.Errors);
    }

    _personRepository.Update(person);

    return Result.Success();
  }
}

namespace ScreenDrafts.Modules.Drafts.Features.People.Edit;

internal sealed class EditPersonCommandHandler(IPersonRepository personRepository) : ICommandHandler<EditPersonCommand>
{
  private readonly IPersonRepository _personRepository = personRepository;

  public async Task<Result> Handle(EditPersonCommand EditPersonRequest, CancellationToken cancellationToken)
  {
    var person = await _personRepository.GetByPublicIdAsync(EditPersonRequest.PublicId, cancellationToken);

    if (person is null)
    {
      return Result.Failure(PersonErrors.NotFound(EditPersonRequest.PublicId));
    }

    var updateResult = person.Update(
      EditPersonRequest.FirstName,
      EditPersonRequest.LastName,
      EditPersonRequest.DisplayName);

    if (updateResult.IsFailure)
    {
      return Result.Failure(updateResult.Errors);
    }

    _personRepository.Update(person);

    return Result.Success();
  }
}




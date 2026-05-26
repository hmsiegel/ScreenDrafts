namespace ScreenDrafts.Modules.Drafts.Features.People.UpdateProfile;

internal sealed class UpdateProfileCommandHandler(IPersonRepository personRepository)
  : ICommandHandler<UpdateProfileCommand>
{
  private readonly IPersonRepository _personRepository = personRepository;

  public async Task<Result> Handle(
    UpdateProfileCommand request,
    CancellationToken cancellationToken
  )
  {
    var person = await _personRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);

    if (person is null)
    {
      return Result.Failure(PersonErrors.NotFound(request.PublicId));
    }

    if (request.DisplayName is not null)
    {
      var updateResult = person.Update(person.FirstName, person.LastName, request.DisplayName);

      if (updateResult.IsFailure)
      {
        return Result.Failure(updateResult.Errors);
      }
    }

    var updateBiographyResult = person.UpdateBiography(request.Biography);

    if (updateBiographyResult.IsFailure)
    {
      return Result.Failure(updateBiographyResult.Errors);
    }

    var updateLocationResult = person.UpdateLocation(request.Location);

    if (updateLocationResult.IsFailure)
    {
      return Result.Failure(updateLocationResult.Errors);
    }

    _personRepository.Update(person);

    return Result.Success();
  }
}

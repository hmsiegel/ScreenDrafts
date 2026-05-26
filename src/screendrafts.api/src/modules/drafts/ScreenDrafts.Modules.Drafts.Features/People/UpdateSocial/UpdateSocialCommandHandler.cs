namespace ScreenDrafts.Modules.Drafts.Features.People.UpdateSocial;

internal sealed class UpdateSocialCommandHandler(IPersonRepository personRepository)
  : ICommandHandler<UpdateSocialCommand>
{
  private readonly IPersonRepository _personRepository = personRepository;

  public async Task<Result> Handle(UpdateSocialCommand request, CancellationToken cancellationToken)
  {
    var person = await _personRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);

    if (person is null)
    {
      return Result.Failure(PersonErrors.NotFound(request.PublicId));
    }

    var result = person.UpdateSocialHandles(
      twitterHandle: request.TwitterHandle,
      instagramHandle: request.InstagramHandle,
      letterboxdHandle: request.LetterboxdHandle,
      blueskyHandle: request.BlueskyHandle
    );

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _personRepository.Update(person);

    return Result.Success();
  }
}

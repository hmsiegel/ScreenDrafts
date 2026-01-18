namespace ScreenDrafts.Modules.Users.Features.Users.Update;

internal sealed class CommandHandler(IUserRepository userRepository)
  : Common.Features.Abstractions.Messaging.ICommandHandler<Command>
{
  private readonly IUserRepository _userRepository = userRepository;

  public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetByPublicIdAsync(command.PublicId, cancellationToken);

    if (user is null)
    {
      return Result.Failure(UserErrors.PublicIdNotFound(command.PublicId));
    }

    user.Update(
      firstName: FirstName.Create(command.FirstName).Value,
      lastName: LastName.Create(command.LastName).Value,
      middleName: command.MiddleName is not null ? command.MiddleName : null);

    if (!string.IsNullOrWhiteSpace(command.ProfilePicture))
    {
      user.UpdateProfilePicture(command.ProfilePicture);
    }

    if (!string.IsNullOrWhiteSpace(command.TwitterHandle)
      || !string.IsNullOrWhiteSpace(command.InstagramHandle)
      || !string.IsNullOrWhiteSpace(command.LetterboxdHandle)
      || !string.IsNullOrWhiteSpace(command.BlueskyHandle))
    {
      user.UpdateSocialHandles(
        twitterHandle: command.TwitterHandle,
        instagramHandle: command.InstagramHandle,
        letterboxdHandle: command.LetterboxdHandle,
        blueskyHandle: command.BlueskyHandle);
    }

    _userRepository.Update(user);

    return Result.Success();
  }
}

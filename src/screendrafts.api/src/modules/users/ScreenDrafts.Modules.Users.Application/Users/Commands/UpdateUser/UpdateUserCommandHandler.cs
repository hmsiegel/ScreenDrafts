namespace ScreenDrafts.Modules.Users.Application.Users.Commands.UpdateUser;

internal sealed class UpdateUserCommandHandler(IUnitOfWork unitOfWork, IUserRepository userRepository)
  : ICommandHandler<UpdateUserCommand>
{
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  private readonly IUserRepository _userRepository = userRepository;

  public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetAsync(UserId.Create(request.UserId), cancellationToken);

    if (user is null)
    {
      return Result.Failure(UserErrors.NotFound(request.UserId));
    }

    user.Update(
      firstName: FirstName.Create(request.FirstName).Value,
      lastName: LastName.Create(request.LastName).Value,
      middleName: request.MiddleName is not null ? request.MiddleName : null);

    if (!string.IsNullOrWhiteSpace(request.ProfilePicturePath))
    {
      user.UpdateProfilePicture(request.ProfilePicturePath);
    }

    if (!string.IsNullOrWhiteSpace(request.TwitterHandle)
      || !string.IsNullOrWhiteSpace(request.InstagramHandle)
      || !string.IsNullOrWhiteSpace(request.LetterboxdHandles)
      || !string.IsNullOrWhiteSpace(request.BlueskyHandle))
    {
      user.UpdateSocialHandles(
        twitterHandle: request.TwitterHandle,
        instagramHandle: request.InstagramHandle,
        letterboxdHandle: request.LetterboxdHandles,
        blueskyHandle: request.BlueskyHandle);
    }

    _userRepository.Update(user);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}


namespace ScreenDrafts.Modules.Users.Features.Users.Update;

internal sealed class UpdateUserCommandHandler(IUserRepository userRepository)
  : ICommandHandler<UpdateUserCommand>
{
  private readonly IUserRepository _userRepository = userRepository;

  public async Task<Result> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetByPublicIdAsync(command.PublicId, cancellationToken);

    if (user is null)
    {
      return Result.Failure(UserErrors.PublicIdNotFound(command.PublicId));
    }

    user.Update(
      firstName: FirstName.Create(command.FirstName).Value,
      lastName: LastName.Create(command.LastName).Value,
      middleName: command.MiddleName is not null ? command.MiddleName : null
    );

    _userRepository.Update(user);

    return Result.Success();
  }
}

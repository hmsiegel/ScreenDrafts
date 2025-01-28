namespace ScreenDrafts.Modules.Users.Application.Users.Commands.UpdateUser;

internal sealed class UpdateUserCommandHandler(IUnitOfWork unitOfWork, IUserRepository userRepository)
  : ICommandHandler<UpdateUserCommand>
{
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  private readonly IUserRepository _userRepository = userRepository;

  public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetAsync(request.UserId, cancellationToken);

    if (user is null)
    {
      return Result.Failure(UserErrors.NotFound(request.UserId));
    }

    user.Update(
      firstName: FirstName.Create(request.FirstName).Value,
      lastName: LastName.Create(request.LastName).Value,
      middleName: request.MiddleName is not null ? request.MiddleName : null);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}


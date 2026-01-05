namespace ScreenDrafts.Modules.Users.Application.Users.Commands.RegisterUser;

internal sealed class RegisterUserCommandHandler(
  IUserRepository userRepository,
  IUnitOfWork unitOfWork,
  IIdentityProviderService identityProviderService)
  : ICommandHandler<RegisterUserCommand, Guid>
{
  private readonly IUserRepository _userRepository = userRepository;
  private readonly IIdentityProviderService _identityProviderService = identityProviderService;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
  {
    if (request.Password.Length < 6)
    {
      return Result.Failure<Guid>(UserErrors.PasswordTooShort);
    }

    var emailResult = Email.Create(request.Email);

    if (emailResult.IsFailure)
    {
      return Result.Failure<Guid>(emailResult.Errors[0]);
    }

    if (!await _userRepository.IsEmailUniqueAsync(emailResult.Value, cancellationToken))
    {
      return Result.Failure<Guid>(UserErrors.EmailInUse);
    }

    var firstNameResult = FirstName.Create(request.FirstName);
    var lastNameResult = LastName.Create(request.LastName);

    if (firstNameResult.IsFailure || lastNameResult.IsFailure)
    {
      return Result.Failure<Guid>(UserErrors.InvalidName);
    }

    var result = await _identityProviderService.RegisterUserAsync(
      new UserModel(
        request.Email,
        request.Password,
        request.FirstName,
        request.LastName),
      cancellationToken);

    if (result.IsFailure)
    {
      return Result.Failure<Guid>(result.Errors[0]);
    }

    var userResult = User.Create(
      emailResult.Value,
      firstNameResult.Value,
      lastNameResult.Value,
      result.Value,
      request.MiddleName);

    if (userResult.IsFailure)
    {
      return Result.Failure<Guid>(userResult.Errors[0]);
    }

    var user = userResult.Value;

    _userRepository.Add(user);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return user.Id.Value;
  }
}

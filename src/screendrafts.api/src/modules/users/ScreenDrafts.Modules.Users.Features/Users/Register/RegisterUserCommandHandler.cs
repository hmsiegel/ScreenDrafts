namespace ScreenDrafts.Modules.Users.Features.Users.Register;

internal sealed class RegisterUserCommandHandler(
  IUserRepository userRepository,
  IIdentityProviderService identityProviderService,
  IPublicIdGenerator publicIdGenerator)
  : ICommandHandler<RegisterUserCommand, Guid>
{
  private readonly IUserRepository _userRepository = userRepository;
  private readonly IIdentityProviderService _identityProviderService = identityProviderService;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
  {
    var emailResult = Email.Create(request.Email);

    if (emailResult.IsFailure)
    {
      return Result.Failure<Guid>(emailResult.Errors[0]);
    }

    var emailValue = emailResult.Value.Value;

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

    var firstNameValue = firstNameResult.Value.Value;
    var lastNameValue = lastNameResult.Value.Value;

    var identityResult = await _identityProviderService.RegisterUserAsync(
      new UserModel(
        emailValue!,
        request.Password,
        firstNameValue!,
        lastNameValue!),
      cancellationToken);

    if (identityResult.IsFailure)
    {
      return Result.Failure<Guid>(identityResult.Errors[0]);
    }

    var userId = identityResult.Value;
    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.User);

    var userResult = User.Create(
      email: emailResult.Value,
      firstName: firstNameResult.Value,
      lastName: lastNameResult.Value,
      identityId: userId,
      publicId: publicId,
      middleName: request.MiddleName);

    if (userResult.IsFailure)
    {
      return Result.Failure<Guid>(userResult.Errors[0]);
    }

    var user = userResult.Value;

    _userRepository.Add(user);

    return user.Id.Value;
  }
}

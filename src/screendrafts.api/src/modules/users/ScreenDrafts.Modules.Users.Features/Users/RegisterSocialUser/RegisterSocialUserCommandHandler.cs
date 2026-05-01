namespace ScreenDrafts.Modules.Users.Features.Users.RegisterSocialUser;

internal sealed class RegisterSocialUserCommandHandler(
  IUserRepository userRepository,
  IPublicIdGenerator publicIdGenerator)
  : ICommandHandler<RegisterSocialUserCommand, string>
{
  private readonly IUserRepository _userRepository = userRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(
    RegisterSocialUserCommand request,
    CancellationToken cancellationToken)
  {
    var existing = await _userRepository.GetByIdentityIdAsync(request.IdentityId, cancellationToken);

    if (existing is not null)
    {
      return existing.PublicId;
    }

    var emailResult = Email.Create(request.Email);
    if (emailResult.IsFailure)
    {
      return Result.Failure<string>(emailResult.Errors[0]);
    }

    var firstName = string.IsNullOrWhiteSpace(request.FirstName)
        ? "User"
        : request.FirstName;

    var lastName = string.IsNullOrWhiteSpace(request.LastName)
        ? request.Email.Split('@')[0]
        : request.LastName;

    var firstNameResult = FirstName.Create(firstName);
    var lastNameResult = LastName.Create(lastName);

    if (firstNameResult.IsFailure || lastNameResult.IsFailure)
    {
      return Result.Failure<string>(UserErrors.InvalidName);
    }

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.User);

    var userResult = User.Create(
      emailResult.Value,
      firstNameResult.Value,
      lastNameResult.Value,
      request.IdentityId,
      publicId);

    if (userResult.IsFailure)
    {
      return Result.Failure<string>(userResult.Errors[0]);
    }

    _userRepository.Add(userResult.Value);

    return Result.Success(userResult.Value.PublicId);
  }
}


namespace ScreenDrafts.Modules.Users.Features.Users.UpdatePerson;

internal sealed class CommandHandler(IUserRepository userRepository)
  : Common.Features.Abstractions.Messaging.ICommandHandler<Command>
{
  private readonly IUserRepository _userRepository = userRepository;

  public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
  {
    var userId = UserId.Create(request.UserId);

    var user = await _userRepository.GetAsync(userId, cancellationToken);

    if (user is null)
    {
      return Result.Failure(UserErrors.NotFound(request.UserId));
    }

    user.LinkPerson(request.PersonId, request.PersonPublicId);

    _userRepository.Update(user);

    return Result.Success();
  }
}

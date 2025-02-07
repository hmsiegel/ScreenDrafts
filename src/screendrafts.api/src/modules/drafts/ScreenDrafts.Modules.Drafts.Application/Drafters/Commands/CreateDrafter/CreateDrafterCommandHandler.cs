namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.CreateDrafter;

internal sealed class CreateDrafterCommandHandler(
  IDraftersRepository drafterRepository,
  IUnitOfWork unitOfWork,
  IUsersApi usersApi)
  : ICommandHandler<CreateDrafterCommand, Guid>
{
  private readonly IDraftersRepository _drafterRepository = drafterRepository;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public async Task<Result<Guid>> Handle(CreateDrafterCommand request, CancellationToken cancellationToken)
  {
    Result<Drafter>? drafter = null;

    if (request.UserId is not null && request.Name is not null)
    {
      drafter = Drafter.Create(
        request.Name!,
        request.UserId!.Value);
    }

    if (request.UserId is null && request.Name is null)
    {
      return Result.Failure<Guid>(DrafterErrors.CannotCreatDrafter);
    }

    if (request.UserId is null)
    {
      drafter = Drafter.Create(request.Name!);

      _drafterRepository.Add(drafter.Value);
    }

    if (request.Name is null)
    {
      var userId = request.UserId!.Value;

      var user = await _usersApi.GetUserByIdAsync(userId, cancellationToken);

      if (user is null)
      {
        return Result.Failure<Guid>(DrafterErrors.NotFound(userId));
      }

      var drafterName = $"{user.FirstName} {user.MiddleName} {user.LastName}";

      drafter = Drafter.Create(
        drafterName,
        user.UserId);

      _drafterRepository.Add(drafter.Value);
    }

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return drafter!.Value.Id.Value;
  }
}

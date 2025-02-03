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
    var user = await _usersApi.GetUserByIdAsync(request.UserId, cancellationToken);

    if (user is null)
    {
      return Result.Failure<Guid>(DrafterErrors.NotFound(request.UserId));
    }

    var drafterName = $"{user.FirstName} {user.MiddleName} {user.LastName}";

    var drafter = Drafter.Create(
      drafterName,
      user.UserId);


    _drafterRepository.Add(drafter.Value);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return drafter.Value.Id.Value;
  }
}

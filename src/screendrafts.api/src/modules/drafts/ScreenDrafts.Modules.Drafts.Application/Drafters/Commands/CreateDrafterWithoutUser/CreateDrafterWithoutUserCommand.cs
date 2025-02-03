namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.CreateDrafterWithoutUser;

public sealed record CreateDrafterWithoutUserCommand(string Name) : ICommand<Guid>;

internal sealed class CreateDrafterWithoutUserCommandHandler(
  IDraftersRepository drafterRepository,
  IUnitOfWork unitOfWork) : ICommandHandler<CreateDrafterWithoutUserCommand, Guid>
{
  private readonly IDraftersRepository _drafterRepository = drafterRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public async Task<Result<Guid>> Handle(CreateDrafterWithoutUserCommand request, CancellationToken cancellationToken)
  {
    var drafter = Drafter.Create(request.Name);

    _drafterRepository.Add(drafter.Value);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return drafter.Value.Id.Value;
  }
}

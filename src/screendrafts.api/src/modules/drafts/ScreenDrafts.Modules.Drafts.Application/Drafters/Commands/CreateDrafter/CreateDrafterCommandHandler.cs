namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.CreateDrafter;

internal sealed class CreateDrafterCommandHandler(
  IDraftersRepository drafterRepository,
  IUnitOfWork unitOfWork)
  : ICommandHandler<CreateDrafterCommand>
{
  private readonly IDraftersRepository _drafterRepository = drafterRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public async Task<Result> Handle(CreateDrafterCommand request, CancellationToken cancellationToken)
  {
    var drafter = Drafter.Create(
      request.Name,
      request.UserId);

    _drafterRepository.Add(drafter.Value);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}

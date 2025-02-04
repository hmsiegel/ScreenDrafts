namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Commands.ExecuteVetoOverride;
public sealed record ExecuteVetoOverrideCommand(Guid DrafterId, Guid VetoId) : ICommand;

internal sealed class ExecuteVetoOverrideCommandHandler(
  IDraftersRepository draftersRepository,
  IUnitOfWork unitOfWork,
  IVetoRepository vetoRepository)
  : ICommandHandler<ExecuteVetoOverrideCommand>
{
  private readonly IDraftersRepository _draftersRepository = draftersRepository;
  private readonly IVetoRepository _vetoRepository = vetoRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public async Task<Result> Handle(ExecuteVetoOverrideCommand request, CancellationToken cancellationToken)
  {
    var drafterId = DrafterId.Create(request.DrafterId);

    var drafter = await _draftersRepository.GetByIdAsync(drafterId, cancellationToken);

    if (drafter is null)
    {
      return Result.Failure<Drafter>(DrafterErrors.NotFound(request.DrafterId));
    }

    var veto = await _vetoRepository.GetByIdAsync(request.VetoId, cancellationToken);

    if (veto is null)
    {
      return Result.Failure<Drafter>(VetoErrors.NotFound(request.VetoId));
    }

    drafter.AddVetoOverride(veto);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}

internal sealed class ExecuteVetoOverrideCommandValidator : AbstractValidator<ExecuteVetoOverrideCommand>
{
  public ExecuteVetoOverrideCommandValidator()
  {
    RuleFor(x => x.DrafterId)
      .NotEmpty();
    RuleFor(x => x.VetoId)
      .NotEmpty();
  }
}

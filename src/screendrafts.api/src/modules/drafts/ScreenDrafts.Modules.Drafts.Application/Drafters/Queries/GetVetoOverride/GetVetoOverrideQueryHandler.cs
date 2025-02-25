namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetVetoOverride;

internal sealed class GetVetoOverrideQueryHandler(IVetoRepository vetoRepository)
  : IQueryHandler<GetVetoOverrideQuery, VetoOverrideDto>
{
  private readonly IVetoRepository _vetoRepository = vetoRepository;

  public async Task<Result<VetoOverrideDto>> Handle(GetVetoOverrideQuery request, CancellationToken cancellationToken)
  {
    var vetooverride = await _vetoRepository.GetVetoOverrideByIdAsync( VetoOverrideId.Create(request.VetoOverrideId), cancellationToken);

    if (vetooverride == null)
    {
      return Result.Failure<VetoOverrideDto>(DrafterErrors.VetoOverrideNotFound(request.VetoOverrideId));
    }
    
    var vetooverrideDto = new VetoOverrideDto(
      Id: vetooverride.Id.Value,
      Veto: new VetoDto(
        Id: vetooverride.Veto.Id.Value,
        PickId: vetooverride.Veto.Pick.Id,
        DrafterId: vetooverride.Veto.Pick.Drafter.Id.Value,
        IsUsed: vetooverride.Veto.IsUsed),
      IsUsed: vetooverride.IsUsed);

    return Result.Success(vetooverrideDto);
  }
}

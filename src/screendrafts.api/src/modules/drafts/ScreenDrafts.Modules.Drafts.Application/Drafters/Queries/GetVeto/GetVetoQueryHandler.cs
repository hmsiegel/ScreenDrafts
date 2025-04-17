namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetVeto;

internal sealed class GetVetoQueryHandler(IVetoRepository vetoRepository) : IQueryHandler<GetVetoQuery, VetoDto>
{
  private readonly IVetoRepository _vetoRepository = vetoRepository;

  public async Task<Result<VetoDto>> Handle(GetVetoQuery request, CancellationToken cancellationToken)
  {
    var veto = await _vetoRepository.GetByIdAsync( VetoId.Create(request.VetoId), cancellationToken);

    if (veto == null)
    {
      return Result.Failure<VetoDto>(DrafterErrors.VetoNotFound(request.VetoId));
    }
    
    var vetoDto = new VetoDto(
      Id: veto.Id.Value,
      PickId: veto.Pick.Id.Value,
      DrafterId: veto.Pick.Drafter!.Id.Value);

    return Result.Success(vetoDto);
  }
}

namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetVeto;

internal sealed class GetVetoQueryHandler(IVetoRepository vetoRepository) : IQueryHandler<GetVetoQuery, VetoResponse>
{
  private readonly IVetoRepository _vetoRepository = vetoRepository;

  public async Task<Result<VetoResponse>> Handle(GetVetoQuery request, CancellationToken cancellationToken)
  {
    var veto = await _vetoRepository.GetByIdAsync( VetoId.Create(request.VetoId), cancellationToken);

    if (veto == null)
    {
      return Result.Failure<VetoResponse>(DrafterErrors.VetoNotFound(request.VetoId));
    }
    
    var vetoDto = new VetoResponse(
      Id: veto.Id.Value,
      PickId: veto.Pick.Id.Value,
      PickPosition: veto.Pick.Position,
      PickPlayOrder: veto.Pick.PlayOrder,
      MovieTitle: veto.Pick.Movie.MovieTitle,
      DrafterId: veto.Drafter?.Id.Value,
      DrafterName: veto.Drafter?.Name);

    return Result.Success(vetoDto);
  }
}

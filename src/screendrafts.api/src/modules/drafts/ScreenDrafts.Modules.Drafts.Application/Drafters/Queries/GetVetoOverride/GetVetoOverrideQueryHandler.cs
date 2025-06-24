namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetVetoOverride;

internal sealed class GetVetoOverrideQueryHandler(IVetoRepository vetoRepository)
  : IQueryHandler<GetVetoOverrideQuery, VetoOverrideResponse>
{
  private readonly IVetoRepository _vetoRepository = vetoRepository;

  public async Task<Result<VetoOverrideResponse>> Handle(GetVetoOverrideQuery request, CancellationToken cancellationToken)
  {
    var vetooverride = await _vetoRepository.GetVetoOverrideByIdAsync(VetoOverrideId.Create(request.VetoOverrideId), cancellationToken);

    if (vetooverride == null)
    {
      return Result.Failure<VetoOverrideResponse>(DrafterErrors.VetoOverrideNotFound(request.VetoOverrideId));
    }

    var vetooverrideDto = new VetoOverrideResponse(
      Id: vetooverride.Id.Value,
      Veto: new VetoResponse(
        Id: vetooverride.Veto.Id.Value,
        PickId: vetooverride.Veto.Pick.Id.Value,
        PickPosition: vetooverride.Veto.Pick.Position,
        PickPlayOrder: vetooverride.Veto.Pick.PlayOrder,
        MovieTitle: vetooverride.Veto.Pick.Movie.MovieTitle,
        DrafterId: vetooverride.Veto.Drafter?.Id.Value,
        DrafterName: vetooverride.Veto.Drafter?.Name,
        DrafterTeamId: vetooverride.Veto.DrafterTeam?.Id.Value,
        DrafterTeamName: vetooverride.Veto.Pick.DrafterTeam?.Name),
      DrafterId: vetooverride.DrafterId?.Value,
      DrafterName: vetooverride.Drafter?.Name,
      DrafterTeamId: vetooverride.DrafterTeamId?.Value,
      DrafterTeamName: vetooverride.DrafterTeam?.Name);

    return Result.Success(vetooverrideDto);
  }
}

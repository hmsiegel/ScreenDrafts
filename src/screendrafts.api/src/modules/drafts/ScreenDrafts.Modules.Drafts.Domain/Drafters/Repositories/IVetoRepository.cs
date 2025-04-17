﻿namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Repositories;
public interface IVetoRepository : IRepository
{
  Task<Veto?> GetByIdAsync(VetoId id, CancellationToken cancellationToken);

  Task<Veto?> GetByPickAsync(PickId pickId, CancellationToken cancellationToken);

  Task<VetoOverride?> GetVetoOverrideByIdAsync(VetoOverrideId id, CancellationToken cancellationToken);

  Task<VetoOverride?> GetVetoOverrideByVetoIdAsync(VetoId vetoId, CancellationToken cancellationToken);
}

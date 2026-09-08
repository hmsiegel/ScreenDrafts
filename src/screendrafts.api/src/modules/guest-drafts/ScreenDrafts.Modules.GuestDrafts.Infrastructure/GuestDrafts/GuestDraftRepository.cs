using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Repositories;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.ValueObjects;

namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.GuestDrafts;

internal sealed class GuestDraftRepository(GuestDraftsDbContext dbContext) : IDraftRepository
{
  private readonly GuestDraftsDbContext _dbContext = dbContext;

  public void Add(Draft guestDraft)
  {
    _dbContext.GuestDrafts.Add(guestDraft);
  }

  public void Update(Draft guestDraft)
  {
    _dbContext.GuestDrafts.Update(guestDraft);
  }

  public void Delete(Draft guestDraft)
  {
    _dbContext.GuestDrafts.Remove(guestDraft);
  }

  public Task<bool> ExistsAsync(DraftId id, CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafts.AnyAsync(d => d.Id == id, cancellationToken);
  }

  public Task<Draft?> GetByIdAsync(DraftId id, CancellationToken cancellationToken)
  {
    return _dbContext
      .GuestDrafts.Include(d => d.Participants)
      .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
  }

  public Task<Draft?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafts.FirstOrDefaultAsync(
      d => d.PublicId == publicId,
      cancellationToken
    );
  }

  public Task<Draft?> GetByPublicIdWithParticipantsAsync(
    string publicId,
    CancellationToken cancellationToken = default
  )
  {
    return _dbContext
      .GuestDrafts.Include(d => d.Participants)
      .FirstOrDefaultAsync(d => d.PublicId == publicId, cancellationToken);
  }

  public Task<List<Draft>> GetAllAsync(CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafts.ToListAsync(cancellationToken);
  }

  public Task<Draft?> GetByPublicIdForGameplayAsync(
    string publicId,
    CancellationToken cancellationToken
  )
  {
    // Owned collections (GuestDraftPick.History) are always loaded automatically by
    // EF Core when their owner is queried -- no explicit Include needed for those.
    return _dbContext
      .GuestDrafts.Include(d => d.Participants)
      .Include(d => d.GameBoard!)
        .ThenInclude(gb => gb.Positions)
      .Include(d => d.Picks)
        .ThenInclude(p => p.PlayedByParticipant)
      .Include(d => d.Picks)
        .ThenInclude(p => p.RevealAuthorizedParticipant)
      .Include(d => d.Picks)
        .ThenInclude(p => p.Vetoes)
          .ThenInclude(v => v.IssuedByParticipant)
      .Include(d => d.Picks)
        .ThenInclude(p => p.Vetoes)
          .ThenInclude(v => v.VetoOverride!)
            .ThenInclude(vo => vo.IssuedByParticipant)
      .Include(d => d.Picks)
        .ThenInclude(p => p.CommissionerOverride)
      .AsSplitQuery()
      .FirstOrDefaultAsync(d => d.PublicId == publicId, cancellationToken);
  }
}

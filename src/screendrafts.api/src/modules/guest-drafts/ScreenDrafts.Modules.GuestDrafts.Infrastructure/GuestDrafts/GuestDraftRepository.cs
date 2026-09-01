namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.GuestDrafts;

internal sealed class GuestDraftRepository(GuestDraftsDbContext dbContext) : IGuestDraftRepository
{
  private readonly GuestDraftsDbContext _dbContext = dbContext;

  public void Add(GuestDraft guestDraft)
  {
    _dbContext.GuestDrafts.Add(guestDraft);
  }

  public void Update(GuestDraft guestDraft)
  {
    _dbContext.GuestDrafts.Update(guestDraft);
  }

  public void Delete(GuestDraft guestDraft)
  {
    _dbContext.GuestDrafts.Remove(guestDraft);
  }

  public Task<bool> ExistsAsync(GuestDraftId id, CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafts.AnyAsync(d => d.Id == id, cancellationToken);
  }

  public Task<GuestDraft?> GetByIdAsync(GuestDraftId id, CancellationToken cancellationToken)
  {
    return _dbContext
      .GuestDrafts.Include(d => d.Participants)
      .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
  }

  public Task<GuestDraft?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafts.FirstOrDefaultAsync(
      d => d.PublicId == publicId,
      cancellationToken
    );
  }

  public Task<GuestDraft?> GetByPublicIdWithParticipantsAsync(
    string publicId,
    CancellationToken cancellationToken = default
  )
  {
    return _dbContext
      .GuestDrafts.Include(d => d.Participants)
      .FirstOrDefaultAsync(d => d.PublicId == publicId, cancellationToken);
  }

  public Task<List<GuestDraft>> GetAllAsync(CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafts.ToListAsync(cancellationToken);
  }
  public Task<GuestDraft?> GetByPublicIdForGameplayAsync(
  string publicId,
  CancellationToken cancellationToken)
  {
    // Owned collections (GuestDraftPick.History) are always loaded automatically by
    // EF Core when their owner is queried -- no explicit Include needed for those.
    return _dbContext.GuestDrafts
      .Include(d => d.Participants)
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

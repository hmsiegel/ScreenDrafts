namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.Invitations;

internal sealed class InvitationRepository(GuestDraftsDbContext dbContext) : IInvitationRepository
{
  private readonly GuestDraftsDbContext _dbContext = dbContext;

  public void Add(Invitation invitation) => _dbContext.Invitations.Add(invitation);

  public void Update(Invitation invitation) => _dbContext.Invitations.Update(invitation);

  public void Delete(Invitation invitation) => _dbContext.Invitations.Remove(invitation);

  public Task<bool> ExistsAsync(InvitationId id, CancellationToken cancellationToken) =>
    _dbContext.Invitations.AnyAsync(i => i.Id == id, cancellationToken);

  public Task<Invitation?> GetByIdAsync(InvitationId id, CancellationToken cancellationToken) =>
    _dbContext.Invitations.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

  public Task<Invitation?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken) =>
    _dbContext.Invitations.FirstOrDefaultAsync(i => i.PublicId == publicId, cancellationToken);

  public Task<List<Invitation>> GetAllAsync(CancellationToken cancellationToken) =>
    _dbContext.Invitations.ToListAsync(cancellationToken);

  public Task<Invitation?> GetByDraftAndEmailAsync(
    Guid draftId,
    string normalizedEmail,
    CancellationToken cancellationToken = default
  )
  {
    var id = DraftId.Create(draftId);

    return _dbContext.Invitations.FirstOrDefaultAsync(
      i => i.DraftId == id && i.Email == normalizedEmail,
      cancellationToken
    );
  }

  public Task<int> CountForDraftAsync(Guid draftId, CancellationToken cancellationToken = default)
  {
    var id = DraftId.Create(draftId);

    return _dbContext.Invitations.CountAsync(i => i.DraftId == id, cancellationToken);
  }

  public async Task<IReadOnlyList<Invitation>> GetPendingByEmailAsync(
    string normalizedEmail,
    CancellationToken cancellationToken = default
  )
  {
    return await _dbContext
      .Invitations.Where(i => i.Email == normalizedEmail && i.AcceptedOnUtc == null)
      .ToListAsync(cancellationToken);
  }
}

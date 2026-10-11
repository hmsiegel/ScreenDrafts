namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Abstractions;

/// <summary>
/// In-memory IInvitationRepository for handler tests that must not touch a database.
/// Records every Add/Update so tests can assert "nothing was written" on failure paths.
/// </summary>
public sealed class InMemoryInvitationRepository : IInvitationRepository
{
  private readonly List<Invitation> _store = [];

  private readonly List<Invitation> _added = [];
  private readonly List<Invitation> _updated = [];

  public IReadOnlyList<Invitation> Added => _added;
  public IReadOnlyList<Invitation> Updated => _updated;

  public IReadOnlyList<Invitation> All => _store;

  /// <summary>Seeds an invitation without recording it as an Add.</summary>
  public void Seed(Invitation invitation) => _store.Add(invitation);

  public void Add(Invitation entity)
  {
    _added.Add(entity);
    _store.Add(entity);
  }

  public void Update(Invitation entity) => _updated.Add(entity);

  public void Delete(Invitation entity) => _store.Remove(entity);

  public Task<Invitation?> GetByIdAsync(InvitationId id, CancellationToken cancellationToken) =>
    Task.FromResult(_store.FirstOrDefault(i => i.Id == id));

  public Task<bool> ExistsAsync(InvitationId id, CancellationToken cancellationToken) =>
    Task.FromResult(_store.Any(i => i.Id == id));

  public Task<Invitation?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken) =>
    Task.FromResult(_store.FirstOrDefault(i => i.PublicId == publicId));

  public Task<List<Invitation>> GetAllAsync(CancellationToken cancellationToken) =>
    Task.FromResult(_store.ToList());

  public Task<Invitation?> GetByDraftAndEmailAsync(
    Guid draftId,
    string normalizedEmail,
    CancellationToken cancellationToken = default
  ) =>
    Task.FromResult(
      _store.FirstOrDefault(i => i.DraftId.Value == draftId && i.Email == normalizedEmail)
    );

  public Task<int> CountForDraftAsync(Guid draftId, CancellationToken cancellationToken = default) =>
    Task.FromResult(_store.Count(i => i.DraftId.Value == draftId));

  public Task<IReadOnlyList<Invitation>> GetPendingByEmailAsync(
    string normalizedEmail,
    CancellationToken cancellationToken = default
  ) =>
    Task.FromResult<IReadOnlyList<Invitation>>(
      [.. _store.Where(i => i.Email == normalizedEmail && i.IsPending)]
    );
}

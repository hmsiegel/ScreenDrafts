namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Abstractions;

/// <summary>In-memory IDraftRepository for handler tests that must not touch a database.</summary>
public sealed class InMemoryDraftRepository : IDraftRepository
{
  private readonly List<Draft> _store = [];

  private readonly List<Draft> _updated = [];

  public IReadOnlyList<Draft> Updated => _updated;

  public void Seed(Draft draft) => _store.Add(draft);

  public void Add(Draft entity) => _store.Add(entity);

  public void Update(Draft entity) => _updated.Add(entity);

  public void Delete(Draft entity) => _store.Remove(entity);

  public Task<Draft?> GetByIdAsync(DraftId id, CancellationToken cancellationToken) =>
    Task.FromResult(_store.FirstOrDefault(d => d.Id == id));

  public Task<bool> ExistsAsync(DraftId id, CancellationToken cancellationToken) =>
    Task.FromResult(_store.Any(d => d.Id == id));

  public Task<Draft?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken) =>
    Task.FromResult(_store.FirstOrDefault(d => d.PublicId == publicId));

  public Task<List<Draft>> GetAllAsync(CancellationToken cancellationToken) =>
    Task.FromResult(_store.ToList());

  public Task<Draft?> GetByPublicIdWithParticipantsAsync(
    string publicId,
    CancellationToken cancellationToken = default
  ) => GetByPublicIdAsync(publicId, cancellationToken);

  public Task<Draft?> GetByPublicIdForGameplayAsync(
    string publicId,
    CancellationToken cancellationToken
  ) => GetByPublicIdAsync(publicId, cancellationToken);
}

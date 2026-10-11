namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Abstractions;

/// <summary>In-memory IDrafterRepository for handler tests that must not touch a database.</summary>
public sealed class InMemoryDrafterRepository : IDrafterRepository
{
  private readonly List<Drafter> _store = [];

  private readonly List<Drafter> _added = [];

  public IReadOnlyList<Drafter> Added => _added;

  public void Seed(Drafter drafter) => _store.Add(drafter);

  public void Add(Drafter entity)
  {
    _added.Add(entity);
    _store.Add(entity);
  }

  public void Update(Drafter entity) { }

  public void Delete(Drafter entity) => _store.Remove(entity);

  public Task<Drafter?> GetByIdAsync(DrafterId id, CancellationToken cancellationToken) =>
    Task.FromResult(_store.FirstOrDefault(d => d.Id == id));

  public Task<bool> ExistsAsync(DrafterId id, CancellationToken cancellationToken) =>
    Task.FromResult(_store.Any(d => d.Id == id));

  public Task<Drafter?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken) =>
    Task.FromResult(_store.FirstOrDefault(d => d.PublicId == publicId));

  public Task<List<Drafter>> GetAllAsync(CancellationToken cancellationToken) =>
    Task.FromResult(_store.ToList());

  public Task<Drafter?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
    Task.FromResult(_store.FirstOrDefault(d => d.UserId == userId));
}

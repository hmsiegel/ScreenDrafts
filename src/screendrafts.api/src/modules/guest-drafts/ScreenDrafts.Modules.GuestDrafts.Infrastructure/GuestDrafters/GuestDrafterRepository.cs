namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.GuestDrafters;

internal sealed class GuestDrafterRepository(GuestDraftsDbContext dbContext)
  : IGuestDrafterRepository
{
  private readonly GuestDraftsDbContext _dbContext = dbContext;

  public void Add(GuestDrafter drafter)
  {
    _dbContext.GuestDrafters.Add(drafter);
  }

  public void Update(GuestDrafter drafter)
  {
    _dbContext.GuestDrafters.Update(drafter);
  }

  public void Delete(GuestDrafter drafter)
  {
    _dbContext.GuestDrafters.Remove(drafter);
  }

  public Task<bool> ExistsAsync(GuestDrafterId id, CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafters.AnyAsync(d => d.Id == id, cancellationToken);
  }

  public Task<GuestDrafter?> GetByIdAsync(GuestDrafterId id, CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafters.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
  }

  public Task<GuestDrafter?> GetByPublicIdAsync(
    string publicId,
    CancellationToken cancellationToken
  )
  {
    return _dbContext.GuestDrafters.FirstOrDefaultAsync(
      d => d.PublicId == publicId,
      cancellationToken
    );
  }

  public Task<GuestDrafter?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafters.FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);
  }

  public Task<List<GuestDrafter>> GetAllAsync(CancellationToken cancellationToken)
  {
    return _dbContext.GuestDrafters.ToListAsync(cancellationToken);
  }

  // NOTE: 50-result cap and ILike-based matching are reasonable defaults, not
  // confirmed against an existing canonical SearchDrafters implementation (I
  // haven't seen one) -- flag if you want pagination or a different limit.
  public async Task<IReadOnlyList<GuestDrafter>> SearchAsync(
    string? search,
    CancellationToken cancellationToken
  )
  {
    var query = _dbContext.GuestDrafters.AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
    {
      var pattern = $"%{search.Trim()}%";
      query = query.Where(d =>
        EF.Functions.ILike(d.FirstName, pattern) || EF.Functions.ILike(d.LastName, pattern)
      );
    }

    return await query
      .OrderBy(d => d.LastName)
      .ThenBy(d => d.FirstName)
      .Take(50)
      .ToListAsync(cancellationToken);
  }
}

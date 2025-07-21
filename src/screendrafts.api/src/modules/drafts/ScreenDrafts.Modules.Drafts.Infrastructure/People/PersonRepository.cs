namespace ScreenDrafts.Modules.Drafts.Infrastructure.People;
internal sealed class PersonRepository(DraftsDbContext dbContext) : IPersonRepository
{
  private readonly DraftsDbContext _dbContext = dbContext;

  public void Add(Person person)
  {
    var  existingEntity = _dbContext.ChangeTracker.Entries<Person>()
      .FirstOrDefault(e => e.Entity.Id == person.Id);

    if (existingEntity is not null)
    {
      return;
    }

    _dbContext.Add(person);
  }

  public async Task<List<Person>> GetAllAsync(CancellationToken cancellationToken = default)
  {
    return await _dbContext.People
      .ToListAsync(cancellationToken);
  }

  public async Task<Person?> GetByIdAsync(PersonId personId, CancellationToken cancellationToken)
  {
    var person = await _dbContext.People
      .Include(p => p.DrafterProfile)
      .Include(p => p.HostProfile)
      .SingleOrDefaultAsync(p => p.Id == personId, cancellationToken);

    return person;
  }

  public async Task<Person?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
  {
    var person = await _dbContext.People
      .SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);

    return person;
  }

  public void Update(Person person)
  {
    _dbContext.People.Update(person);
  }
}

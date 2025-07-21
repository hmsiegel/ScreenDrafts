namespace ScreenDrafts.Modules.Drafts.Domain.People.Repositories;

public interface IPersonRepository : IRepository
{
  void Add(Person person);

  void Update(Person person);

  Task<Person?> GetByIdAsync(PersonId personId, CancellationToken cancellationToken);

  Task<Person?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

  Task<List<Person>> GetAllAsync(CancellationToken cancellationToken = default);
}

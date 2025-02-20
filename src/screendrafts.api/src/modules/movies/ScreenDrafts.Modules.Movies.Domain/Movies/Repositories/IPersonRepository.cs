namespace ScreenDrafts.Modules.Movies.Domain.Movies.Repositories;

public interface IPersonRepository : IRepository
{
  Task<Person?> FindByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default);

  void Add(Person person);
}

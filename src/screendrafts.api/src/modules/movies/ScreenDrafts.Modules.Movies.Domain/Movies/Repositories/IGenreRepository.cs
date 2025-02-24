namespace ScreenDrafts.Modules.Movies.Domain.Movies.Repositories;

public interface IGenreRepository : IRepository
{
  Task<Genre?> FindByNameAsync(string name, CancellationToken cancellationToken = default);

  void Add(Genre genre);
}

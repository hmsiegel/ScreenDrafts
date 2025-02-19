namespace ScreenDrafts.Modules.Movies.Domain.Movies.Repositories;

public interface IGenreRepository
{
  Task<Genre?> FindByNameAsync(string name, CancellationToken cancellationToken = default);

  void Add(Genre genre);
}

namespace ScreenDrafts.Modules.Movies.Infrastructure.Genres;

internal sealed class GenreRepository(MoviesDbContext context) : IGenreRepository
{
  private readonly MoviesDbContext _context = context;

  public void Add(Genre genre)
  {
    _context.Genres.Add(genre);
  }

  public async Task<Genre?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
  {
    return await _context.Genres
      .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
  }
}

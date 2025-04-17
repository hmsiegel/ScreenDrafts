namespace ScreenDrafts.Modules.Movies.Infrastructure.Genres;

internal sealed class GenreRepository(MoviesDbContext context)
  : MoviesRepositoryBase<Genre>(context), IGenreRepository
{
  private readonly MoviesDbContext _context = context;

  public void Add(Genre genre)
  {
    if (_context.Entry(genre).State == EntityState.Detached)
    {
      _context.Genres.Attach(genre);
    }

    _context.Genres.Add(genre);
  }

  public async Task<Genre?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
  {
    return await _context.Genres
      .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
  }
}

namespace ScreenDrafts.Modules.Movies.Domain.Medias.Entities;

public sealed class MediaGenre
{
  private MediaGenre(
    MediaId mediaId,
    Guid genreId)
  {
    MediaId = mediaId;
    GenreId = genreId;
  }

  private MediaGenre()
  {
  }

  public MediaId MediaId { get; private set; } = default!;

  public Media Media { get; private set; } = default!;

  public Guid GenreId { get; private set; } = Guid.Empty;

  public Genre Genre { get; private set; } = default!;

  public static MediaGenre Create(
    MediaId mediaId,
    Guid genreId)
  {
    return new MediaGenre(mediaId, genreId);
  }
}

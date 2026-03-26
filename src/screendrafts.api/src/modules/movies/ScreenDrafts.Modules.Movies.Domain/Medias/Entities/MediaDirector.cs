namespace ScreenDrafts.Modules.Movies.Domain.Medias.Entities;

public sealed class MediaDirector : Entity
{
  private MediaDirector(
    Guid id,
    MediaId mediaId,
    PersonId actorId)
    : base(id)
  {
    Id = id;
    MediaId = Guard.Against.Null(mediaId);
    DirectorId = Guard.Against.Null(actorId);
  }

  private MediaDirector()
  {
  }

  public MediaId MediaId { get; private set; } = default!;

  public Media Media { get; private set; } = default!;

  public PersonId DirectorId { get; private set; } = default!;

  public Person Director { get; private set; } = default!;

  public static MediaDirector Create(
    MediaId mediaId,
    PersonId actorId,
    Guid? id = null)
  {
    return new MediaDirector(
      mediaId: mediaId,
      actorId: actorId,
      id: id ?? Guid.NewGuid());
  }
}

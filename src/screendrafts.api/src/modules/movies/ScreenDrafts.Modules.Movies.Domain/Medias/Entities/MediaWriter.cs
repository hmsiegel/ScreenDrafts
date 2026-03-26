namespace ScreenDrafts.Modules.Movies.Domain.Medias.Entities;

public sealed class MediaWriter : Entity
{
  private MediaWriter(
    Guid id,
    MediaId mediaId,
    PersonId actorId)
    : base(id)
  {
    Id = id;
    MediaId = Guard.Against.Null(mediaId);
    WriterId = Guard.Against.Null(actorId);
  }

  private MediaWriter()
  {
  }

  public MediaId MediaId { get; private set; } = default!;

  public Media Media { get; private set; } = default!;

  public PersonId WriterId { get; private set; } = default!;

  public Person Writer { get; private set; } = default!;

  public static MediaWriter Create(
    MediaId mediaId,
    PersonId actorId,
    Guid? id = null)
  {
    return new MediaWriter(
      mediaId: mediaId,
      actorId: actorId,
      id: id ?? Guid.NewGuid());
  }
}

namespace ScreenDrafts.Modules.Movies.Domain.Medias.Entities;

public sealed class MediaActor : Entity
{
  private MediaActor(
    Guid id,
    MediaId mediaId,
    PersonId actorId)
    : base(id)
  {
    Id = id;
    MediaId = Guard.Against.Null(mediaId);
    ActorId = Guard.Against.Null(actorId);
  }

  private MediaActor()
  {
  }

  public MediaId MediaId { get; private set; } = default!;

  public Media Media { get; private set; } = default!;

  public PersonId ActorId { get; private set; } = default!;

  public Person Actor { get; private set; } = default!;

  public static MediaActor Create(
    MediaId mediaId,
    PersonId actorId,
    Guid? id = null)
  {
    return new MediaActor(
      mediaId: mediaId,
      actorId: actorId,
      id: id ?? Guid.NewGuid());
  }
}

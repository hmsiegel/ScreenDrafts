namespace ScreenDrafts.Modules.Movies.Domain.Medias.Entities;

public sealed class MediaProducer : Entity
{
  private MediaProducer(
    Guid id,
    MediaId mediaId,
    PersonId actorId)
    : base(id)
  {
    Id = id;
    MediaId = Guard.Against.Null(mediaId);
    ProducerId = Guard.Against.Null(actorId);
  }

  private MediaProducer()
  {
  }

  public MediaId MediaId { get; private set; } = default!;

  public Media Media { get; private set; } = default!;

  public PersonId ProducerId { get; private set; } = default!;

  public Person Producer { get; private set; } = default!;

  public static MediaProducer Create(
    MediaId mediaId,
    PersonId actorId,
    Guid? id = null)
  {
    return new MediaProducer(
      mediaId: mediaId,
      actorId: actorId,
      id: id ?? Guid.NewGuid());
  }
}

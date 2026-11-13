namespace ScreenDrafts.Modules.Movies.Domain.Medias.Entities;

public sealed class MediaActor : Entity
{
  private MediaActor(Guid id, MediaId mediaId, PersonId actorId, int? billingOrder)
    : base(id)
  {
    Id = id;
    MediaId = Guard.Against.Null(mediaId);
    ActorId = Guard.Against.Null(actorId);
    BillingOrder = billingOrder;
  }

  private MediaActor() { }

  public MediaId MediaId { get; private set; } = default!;
  public Media Media { get; private set; } = default!;
  public PersonId ActorId { get; private set; } = default!;
  public Person Actor { get; private set; } = default!;

  /// <summary>Zero-based position in the TMDb cast list; null until synced.</summary>
  public int? BillingOrder { get; private set; }

  public void SetBillingOrder(int billingOrder) => BillingOrder = billingOrder;

  public static MediaActor Create(
    MediaId mediaId,
    PersonId actorId,
    int? billingOrder = null,
    Guid? id = null
  )
  {
    return new MediaActor(
      mediaId: mediaId,
      actorId: actorId,
      billingOrder: billingOrder,
      id: id ?? Guid.NewGuid()
    );
  }
}

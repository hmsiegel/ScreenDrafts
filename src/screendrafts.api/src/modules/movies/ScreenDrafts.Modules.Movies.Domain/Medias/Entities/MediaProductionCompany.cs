namespace ScreenDrafts.Modules.Movies.Domain.Medias.Entities;

public sealed class MediaProductionCompany
{
  private MediaProductionCompany(
    MediaId mediaId,
    Guid productionCompanyId)
  {
    MediaId = mediaId;
    ProductionCompanyId = productionCompanyId;
  }

  private MediaProductionCompany()
  {
  }

  public MediaId MediaId { get; private set; } = default!;

  public Media Media { get; private set; } = default!;

  public Guid ProductionCompanyId { get; private set; } = Guid.Empty;

  public ProductionCompany ProductionCompany { get; private set; } = default!;

  public static MediaProductionCompany Create(
    MediaId mediaId,
    Guid productionCompanyId)
  {
    return new MediaProductionCompany(mediaId, productionCompanyId);
  }
}

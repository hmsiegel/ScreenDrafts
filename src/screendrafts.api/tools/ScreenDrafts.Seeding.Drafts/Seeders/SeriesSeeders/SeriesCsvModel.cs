namespace ScreenDrafts.Seeding.Drafts.Seeders.SeriesSeeders;

internal sealed class SeriesCsvModel
{
  public Guid? Id { get; set; }

  public string Name { get; set; } = string.Empty;

  public int SeriesKind { get; set; }

  public int CanonicalPolicy { get; set; }

  public int ContinuityScope { get; set; }

  public int ContinuityDateRule { get; set; }

  public int? DefaultDraftType { get; set; }

  public int AllowedDraftTypes { get; set; }
}

namespace ScreenDrafts.Modules.Reporting.Domain.Drafts;

public sealed class SiteStats : Entity
{
  private SiteStats(Guid id)
  {
    Id = id;
    VetoesCount = 0;
    UpdatedAt = DateTime.UtcNow;
  }

  private SiteStats() { }

  public int VetoesCount { get; private set; }
  public DateTime UpdatedAt { get; private set; }

  public static SiteStats Create() => new(Guid.NewGuid());

  public void IncrementVetoes(int count)
  {
    VetoesCount += count;
    UpdatedAt = DateTime.UtcNow;
  }
}

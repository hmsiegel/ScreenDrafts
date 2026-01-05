namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;

public sealed class SeriesKind(string name, int value) : SmartEnum<SeriesKind>(name, value)
{
  public static readonly SeriesKind Regular = new(nameof(Regular), 0);
  public static readonly SeriesKind FranchiseMiniSuper = new(nameof( FranchiseMiniSuper), 1);
  public static readonly SeriesKind InMemoriam = new(nameof(InMemoriam), 2);
  public static readonly SeriesKind Legends = new(nameof(Legends), 3);
  public static readonly SeriesKind BestPictureNominee = new(nameof(BestPictureNominee), 4);
  public static readonly SeriesKind PatreonVs = new(nameof(PatreonVs), 5);
  public static readonly SeriesKind LiveDraft = new(nameof(LiveDraft), 6);
  public static readonly SeriesKind Special = new(nameof(Special), 7);
  public static readonly SeriesKind SpeedDrafts = new(nameof(SpeedDrafts), 8);
  public static readonly SeriesKind MissionImpossibleSuperDraft = new(nameof(MissionImpossibleSuperDraft), 9);
}

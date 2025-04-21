namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Enums;

public sealed class EpisodeType(string name, int value) : SmartEnum<EpisodeType>(name, value)
{
  public static readonly EpisodeType MainFeed = new(nameof(MainFeed), 0);
  public static readonly EpisodeType FranchiseMiniSuper = new(nameof( FranchiseMiniSuper), 1);
  public static readonly EpisodeType InMemorium = new(nameof(InMemorium), 2);
  public static readonly EpisodeType Legends = new(nameof(Legends), 3);
  public static readonly EpisodeType BestPictureNominee = new(nameof(BestPictureNominee), 4);
  public static readonly EpisodeType PatreonsVs = new(nameof(PatreonsVs), 5);
  public static readonly EpisodeType LiveDraft = new(nameof(LiveDraft), 6);
  public static readonly EpisodeType Special = new(nameof(Special), 7);
}

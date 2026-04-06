namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Enums;

public sealed class SubDraftStatus(string name, int value)
  : SmartEnum<SubDraftStatus>(name, value)
{
  public static readonly SubDraftStatus Pending = new(nameof(Pending), 0);
  public static readonly SubDraftStatus Active = new(nameof(Active), 1);
  public static readonly SubDraftStatus Completed = new(nameof(Completed), 2);
}

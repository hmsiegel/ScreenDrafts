namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Enums;

public sealed class SubjectKind(string name, int value) : SmartEnum<SubjectKind>(name, value)
{
  public static readonly SubjectKind Actor = new(nameof(Actor), 0);
  public static readonly SubjectKind Director = new(nameof(Director), 1);
  public static readonly SubjectKind Word = new(nameof(Word), 2);
}

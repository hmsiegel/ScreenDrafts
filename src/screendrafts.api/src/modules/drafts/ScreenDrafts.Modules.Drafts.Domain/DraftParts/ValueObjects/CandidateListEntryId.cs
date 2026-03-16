namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;

public sealed record CandidateListEntryId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static CandidateListEntryId Create(Guid values) => new(values);
  public static CandidateListEntryId CreateUnique() => new(Guid.NewGuid());
  public static CandidateListEntryId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));
}

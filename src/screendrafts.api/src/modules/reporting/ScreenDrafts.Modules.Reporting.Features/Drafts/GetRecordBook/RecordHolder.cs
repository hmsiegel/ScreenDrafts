namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

internal sealed record RecordHolder
{
  /// <summary>"drafter", "draft" or "title".</summary>
  public required string Kind { get; init; }
  public required string Name { get; init; }

  /// <summary>For a drafter, the person's public id (the site's /drafters/{id} route). For a draft or title, its own public id.</summary>
  public string? PublicId { get; init; }

  /// <summary>Extra detail: the draft a single-draft record was set in, or "12 drafts" for a qualified record.</summary>
  public string? Context { get; init; }
}

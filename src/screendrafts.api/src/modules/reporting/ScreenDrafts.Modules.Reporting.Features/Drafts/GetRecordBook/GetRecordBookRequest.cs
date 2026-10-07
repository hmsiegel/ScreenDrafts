namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

internal sealed record GetRecordBookRequest
{
  /// <summary>
  /// Asks for Patreon and Speed drafts on top of canonical ones. Only honored for Patreon members.
  /// </summary>
  [FromQuery(Name = "includeAll")]
  public bool IncludeAll { get; init; }
}

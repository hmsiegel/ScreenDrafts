namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;

public sealed partial class Draft
{
  /// <summary>
  /// When set, restricts this draft's pool, boards, and candidate lists to media
  /// whose TvSeriesTmdbId matches this value — e.g. a draft restricted to Star
  /// Trek: The Original Series (TMDb series ID) can't accidentally accumulate an
  /// episode of The Twilight Zone. Null for movie drafts and for any draft that
  /// isn't restricted to a single TV series. Lives on Draft rather than DraftPart
  /// because DraftPool and DraftBoard are both Draft-scoped, not part-scoped —
  /// putting it on DraftPart would leave pool/board adds with nothing to check
  /// against.
  /// </summary>
  public int? RestrictedTvSeriesTmdbId { get; private set; }

  /// <summary>
  /// Display name of the restricted series (e.g. "The Twilight Zone"), captured
  /// once when the restriction is set — the admin already has it on hand from
  /// whichever TV series search picked RestrictedTvSeriesTmdbId, so there's no
  /// need to round-trip TMDb again just to show it back. Always null when
  /// RestrictedTvSeriesTmdbId is null; not independently nullable in practice,
  /// though the type allows it since nothing in the domain actually enforces
  /// that pairing at the type level.
  /// </summary>
  public string? RestrictedTvSeriesTitle { get; private set; }

  /// <summary>
  /// Sets or clears the TV series restriction. Pass null to lift it. Intended to
  /// be set once, before any candidates are added — changing it after items
  /// already exist in the pool/boards/candidate lists does not retroactively
  /// validate them.
  /// </summary>
  public void SetTvSeriesRestriction(int? tvSeriesTmdbId, string? tvSeriesTitle)
  {
    RestrictedTvSeriesTmdbId = tvSeriesTmdbId;
    RestrictedTvSeriesTitle = tvSeriesTitle;
    UpdatedAtUtc = DateTime.UtcNow;
  }
}

namespace ScreenDrafts.Modules.Integrations.Features.People.SearchImdbPeople;

// NOTE: field names on IMDbApiLib.Models.SearchData/its result items
// (Id/Title/Image/Description below) are inferred from the library's
// general search-response convention shared across SearchByTitle/
// SearchByMovie/SearchByName/etc — not verified against the actual model
// class. First real call will confirm or break this; check the mapping in
// the handler below against whatever SearchData actually looks like.

// ── Request ───────────────────────────────────────────────────────────────────

internal sealed record SearchImdbPeopleRequest
{
  [FromQuery(Name = "query")]
  public required string Query { get; init; }
}

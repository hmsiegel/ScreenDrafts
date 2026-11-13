namespace ScreenDrafts.Modules.Movies.Features.Movies.Shared;

/// <summary>
/// Maps the role names accepted by the Vault's person filter and the person
/// picker (any, actor, director, writer, producer) to their credit join tables.
/// Table and column names come only from this fixed list, never from request
/// input, so splicing them into SQL text is safe.
/// </summary>
internal static class MediaCreditRoles
{
  internal const string Any = "any";

  internal readonly record struct CreditSource(string Table, string PersonColumn);

  private static readonly Dictionary<string, CreditSource> Sources = new(
    StringComparer.OrdinalIgnoreCase
  )
  {
    ["actor"] = new("movies.media_actors", "actor_id"),
    ["director"] = new("movies.media_directors", "director_id"),
    ["writer"] = new("movies.media_writers", "writer_id"),
    ["producer"] = new("movies.media_producers", "producer_id"),
  };

  /// <summary>
  /// Resolves a role to the credit tables it covers. Null, blank, or "any" covers all
  /// four. Returns false for an unknown role so callers fail loudly instead of
  /// silently widening the search.
  /// </summary>
  internal static bool TryResolve(string? role, out IReadOnlyList<CreditSource> sources)
  {
    if (string.IsNullOrWhiteSpace(role) || role.Trim().Equals(Any, StringComparison.OrdinalIgnoreCase))
    {
      sources = [.. Sources.Values];
      return true;
    }

    if (Sources.TryGetValue(role.Trim(), out var source))
    {
      sources = [source];
      return true;
    }

    sources = [];
    return false;
  }
}

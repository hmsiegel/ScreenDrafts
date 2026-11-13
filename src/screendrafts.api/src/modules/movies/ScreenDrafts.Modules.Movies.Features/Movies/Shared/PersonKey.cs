using System.Globalization;

namespace ScreenDrafts.Modules.Movies.Features.Movies.Shared;

/// <summary>
/// A person's identity for the Vault's cast and crew filter: their TMDb ID when they have
/// one ("t:31644"), otherwise their IMDb ID ("i:nm0000229"). About four in five people
/// predate the TMDb import and have tmdb_id = 0, so TMDb ID alone cannot identify them.
/// </summary>
internal readonly record struct PersonKey(int? TmdbId, string? ImdbId)
{
  internal static bool TryParse(string? value, out PersonKey key)
  {
    key = default;

    if (string.IsNullOrWhiteSpace(value) || value.Length < 3)
    {
      return false;
    }

    var body = value[2..];

    if (
      value.StartsWith("t:", StringComparison.Ordinal)
      && int.TryParse(body, NumberStyles.None, CultureInfo.InvariantCulture, out var tmdbId)
      && tmdbId > 0
    )
    {
      key = new PersonKey(tmdbId, null);
      return true;
    }

    if (value.StartsWith("i:", StringComparison.Ordinal) && body.All(char.IsAsciiLetterOrDigit))
    {
      key = new PersonKey(null, body);
      return true;
    }

    return false;
  }
}

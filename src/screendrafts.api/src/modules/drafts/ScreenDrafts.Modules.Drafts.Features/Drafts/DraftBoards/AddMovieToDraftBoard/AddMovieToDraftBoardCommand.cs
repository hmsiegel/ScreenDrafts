namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards.AddMovieToDraftBoard;

internal sealed record AddMovieToDraftBoardCommand : ICommand
{
  public string DraftId { get; init; } = default!;
  public string UserPublicId { get; init; } = default!;
  public int TmdbId { get; init; } = default!;
  public string? Notes { get; init; } = default!;
  public int? Priority { get; init; } = default!;

  /// <summary>
  /// Required, not defaulted — a silent default here is exactly the class of bug
  /// this field fixes (this handler previously hardcoded Movie regardless of
  /// what was actually being added, which silently broke episode fetches).
  /// </summary>
  public required MediaType MediaType { get; init; }
  public int? TvSeriesTmdbId { get; init; }
  public int? SeasonNumber { get; init; }
  public int? EpisodeNumber { get; init; }
}

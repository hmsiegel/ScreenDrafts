namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class DraftsModel
{
  public Guid Id { get; set; }

  public string Title { get; set; } = string.Empty;

  public int DraftType { get; set; } 

  public int TotalPicks { get; set; }

  public int TotalDrafters { get; set; }

  public int TotalHosts { get; set; }

  public int DraftStatus { get; set; } = Domain.Drafts.Enums.DraftStatus.Completed;

  public string? EpisodeNumber { get; set; } = string.Empty;

  public int EpisodeType { get; set; }

  [JsonConverter(typeof(DateOnlyJsonConverter))]
  public List<DateOnly> ReleaseDates { get; set; } = [];

  public bool IsPatreonOnly { get; set; }
}

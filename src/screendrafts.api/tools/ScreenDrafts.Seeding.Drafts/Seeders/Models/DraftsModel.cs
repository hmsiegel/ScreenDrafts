namespace ScreenDrafts.Seeding.Drafts.Seeders.Models;

internal sealed class DraftsModel
{
  public Guid? Id { get; set; }

  public string Title { get; set; } = string.Empty;

  public int DraftType { get; set; }

  public int TotalPicks { get; set; }

  public int TotalDrafters { get; set; }

  public int TotalDrafterTeams { get; set; }

  public int TotalHosts { get; set; }

  public int DraftStatus { get; set; } = Modules.Drafts.Domain.Drafts.Enums.DraftStatus.Created;

  public string? EpisodeNumber { get; set; } = string.Empty;

  public int EpisodeType { get; set; }

  [JsonConverter(typeof(DateOnlyJsonConverter))]
  public List<DateOnly> ReleaseDates { get; set; } = [];

  public bool IsPatreonOnly { get; set; }

  public bool IsNonCanonical { get; set; }
}

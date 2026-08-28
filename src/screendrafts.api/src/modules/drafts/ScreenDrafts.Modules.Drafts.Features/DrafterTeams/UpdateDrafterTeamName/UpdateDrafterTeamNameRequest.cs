namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.UpdateDrafterTeamName;

internal sealed record UpdateDrafterTeamNameRequest
{
  [FromRoute(Name = "publicId")]
  public string DrafterTeamId { get; init; } = default!;

  public required string Name { get; init; }
}

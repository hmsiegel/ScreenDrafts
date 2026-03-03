namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Get;

internal sealed record GetDrafterTeamRequest
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }
}

namespace ScreenDrafts.Modules.Drafts.Features.Drafters.DrafterProfile;

internal sealed record GetDrafterProfileQuery : IQuery<GetDrafterProfileResponse>
{
  public required string PublicId { get; init; }
}

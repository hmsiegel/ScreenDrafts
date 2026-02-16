
namespace ScreenDrafts.Modules.Drafts.Features.People.LinkUser;

internal sealed record LinkUserPersonRequest
{
  [FromRoute(Name = "publicId")]
  public required string PublicId { get; init; }
  public Guid UserId { get; init; }
}


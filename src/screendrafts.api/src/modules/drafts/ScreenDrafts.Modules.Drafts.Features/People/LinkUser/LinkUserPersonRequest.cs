
namespace ScreenDrafts.Modules.Drafts.Features.People.LinkUser;

internal sealed record LinkUserPersonRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
  public Guid UserId { get; init; }
}


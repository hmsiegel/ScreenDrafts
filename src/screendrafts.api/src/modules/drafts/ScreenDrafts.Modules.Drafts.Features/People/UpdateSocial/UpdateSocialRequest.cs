namespace ScreenDrafts.Modules.Drafts.Features.People.UpdateSocial;

internal sealed record UpdateSocialRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;

  public string? TwitterHandle { get; init; }
  public string? InstagramHandle { get; init; }
  public string? LetterboxdHandle { get; init; }
  public string? BlueskyHandle { get; init; }
}

namespace ScreenDrafts.Modules.Drafts.Features.People.UpdateSocial;

internal sealed record UpdateSocialCommand : ICommand
{
  public required string PublicId { get; init; }
  public string? TwitterHandle { get; init; }
  public string? InstagramHandle { get; init; }
  public string? LetterboxdHandle { get; init; }
  public string? BlueskyHandle { get; init; }
}

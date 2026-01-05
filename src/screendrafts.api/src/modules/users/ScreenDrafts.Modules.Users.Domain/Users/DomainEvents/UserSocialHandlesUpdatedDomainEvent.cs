namespace ScreenDrafts.Modules.Users.Domain.Users.DomainEvents;

public sealed class UserSocialHandlesUpdatedDomainEvent(
  Guid userId,
  string? twitterHandle,
  string? instagramHandle,
  string? letterboxdHandle,
  string? blueskyHandle) : DomainEvent
{
  public Guid UserId { get; init; } = userId;
  public string? TwitterHandle { get; init; } = twitterHandle;
  public string? InstagramHandle { get; init; } = instagramHandle;
  public string? LetterboxdHandle { get; init; } = letterboxdHandle;
  public string? BlueskyHandle { get; init; } = blueskyHandle;
}

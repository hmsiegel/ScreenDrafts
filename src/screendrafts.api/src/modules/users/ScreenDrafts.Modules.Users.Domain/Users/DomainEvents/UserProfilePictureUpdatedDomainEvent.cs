namespace ScreenDrafts.Modules.Users.Domain.Users.DomainEvents;

public sealed class UserProfilePictureUpdatedDomainEvent(Guid userId, string profilePicturePath) : DomainEvent
{
  public Guid UserId { get; init; } = userId;
  public string ProfilePicturePath { get; init; } = profilePicturePath;
}

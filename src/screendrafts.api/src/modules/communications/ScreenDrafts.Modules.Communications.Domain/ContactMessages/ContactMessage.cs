namespace ScreenDrafts.Modules.Communications.Domain.ContactMessages;

/// <summary>
/// A message submitted through the public Contact Us form. Persisted before the
/// notification email is attempted, so a mail failure never loses the message;
/// <see cref="EmailedOnUtc"/> stays null until the email actually went out.
/// Plain class (no Entity base) because Communications.Domain has no Common.Domain
/// dependency today — swap in Entity if you add one.
/// </summary>
public sealed class ContactMessage
{
  public const int MaxNameLength = 100;
  public const int MaxEmailLength = 254;
  public const int MaxMessageLength = 2000;
  public const int MaxUserPublicIdLength = 50;

  private ContactMessage(
    Guid id,
    string name,
    string email,
    ContactCategory category,
    string message,
    string? userPublicId,
    DateTime createdOnUtc
  )
  {
    Id = id;
    Name = name;
    Email = email;
    Category = category;
    Message = message;
    UserPublicId = userPublicId;
    CreatedOnUtc = createdOnUtc;
  }

  private ContactMessage() { }

  public Guid Id { get; private set; }

  public string Name { get; private set; } = default!;

  public string Email { get; private set; } = default!;

  public ContactCategory Category { get; private set; }

  public string Message { get; private set; } = default!;

  /// <summary>
  /// The signed-in sender's public ID (u_...), if any. Reference only — never used
  /// for authorization.
  /// </summary>
  public string? UserPublicId { get; private set; }

  public DateTime CreatedOnUtc { get; private set; }

  /// <summary>Null means the notification email has not (yet) been sent.</summary>
  public DateTime? EmailedOnUtc { get; private set; }

  public static ContactMessage Create(
    string name,
    string email,
    ContactCategory category,
    string message,
    string? userPublicId,
    DateTime createdOnUtc
  )
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    ArgumentException.ThrowIfNullOrWhiteSpace(email);
    ArgumentException.ThrowIfNullOrWhiteSpace(message);

    return new ContactMessage(
      id: Guid.NewGuid(),
      name: name.Trim(),
      email: email.Trim(),
      category: category,
      message: message.Trim(),
      userPublicId: string.IsNullOrWhiteSpace(userPublicId) ? null : userPublicId.Trim(),
      createdOnUtc: createdOnUtc
    );
  }

  public void MarkEmailed(DateTime emailedOnUtc)
  {
    EmailedOnUtc = emailedOnUtc;
  }
}

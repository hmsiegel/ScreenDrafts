namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;

public sealed record Title(string Value)
{
  public const int MaxLength = 255;
  public static Title Create(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException("Title cannot be empty", nameof(value));
    }
    if (value.Length > MaxLength)
    {
      throw new ArgumentException($"Title cannot be longer than {MaxLength} characters", nameof(value));
    }
    return new Title(value);
  }

  public static Title Empty => new(string.Empty);
}

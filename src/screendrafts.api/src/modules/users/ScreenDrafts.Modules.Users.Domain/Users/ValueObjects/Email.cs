using ScreenDrafts.Common.Abstractions.Results;

namespace ScreenDrafts.Modules.Users.Domain.Users.ValueObjects;

public sealed record Email(string? Value)
{
  public const int MaxLength = 255;

  public string? Value { get; init; } = Value;

  public static Result<Email> Create(string? email) =>
    Result.Ensure(
      email,
      (e => !string.IsNullOrWhiteSpace(e), EmailErrors.Empty),
      (e => e!.Length <= MaxLength, EmailErrors.TooLong),
      (e => e!.Split('@').Length == 2, EmailErrors.Invalid))
    .Map(e => new Email(e));
}

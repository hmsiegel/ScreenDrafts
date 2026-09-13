using ScreenDrafts.Common.Abstractions.Results;

namespace ScreenDrafts.Modules.Users.Domain.Users.ValueObjects;

public sealed record Email(string? Value)
{
  public const int MaxLength = 255;

  public string? Value { get; init; } = Value;

  public static Result<Email> Create(string? email) =>
    Result
      .Create(email)
      .Ensure(e => !string.IsNullOrWhiteSpace(e), EmailErrors.Empty)
      .Ensure(e => e!.Length <= MaxLength, EmailErrors.TooLong)
      .Ensure(e => e!.Split('@').Length == 2, EmailErrors.Invalid)
      .Map(e => new Email(e));
}

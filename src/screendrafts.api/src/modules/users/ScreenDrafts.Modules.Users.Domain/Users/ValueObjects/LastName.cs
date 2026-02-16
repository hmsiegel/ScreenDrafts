using ScreenDrafts.Common.Abstractions.Results;

namespace ScreenDrafts.Modules.Users.Domain.Users.ValueObjects;

public class LastName(string? Value)
{
  public const int MaxLength = 50;

  public string? Value { get; init; } = Value;

  public override string ToString() => Value ?? string.Empty;

  public static Result<LastName> Create(string lastName) =>
    Result.Create(lastName)
      .Ensure(lastName => !string.IsNullOrWhiteSpace(lastName), LastNameErrors.Empty)
      .Ensure(lastName => lastName!.Length <= MaxLength, LastNameErrors.TooLong)
    .Map(e => new LastName(lastName));
}

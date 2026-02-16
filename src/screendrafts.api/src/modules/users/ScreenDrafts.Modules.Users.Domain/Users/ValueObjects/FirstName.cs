using ScreenDrafts.Common.Abstractions.Results;

namespace ScreenDrafts.Modules.Users.Domain.Users.ValueObjects;

public class FirstName(string? Value)
{
  public const int MaxLength = 50;

  public string? Value { get; init; } = Value;

  public override string ToString() => Value ?? string.Empty;

  public static Result<FirstName> Create(string firstName) =>
    Result.Create(firstName)
      .Ensure(firstName => !string.IsNullOrWhiteSpace(firstName), FirstNameErrors.Empty)
      .Ensure(firstName => firstName!.Length <= MaxLength, FirstNameErrors.TooLong)
    .Map(e => new FirstName(firstName));
}

namespace ScreenDrafts.Common.Abstractions.Errors;

public record SDError
{
  public static readonly SDError None = new(string.Empty, string.Empty, ErrorType.Failure);
  public static readonly SDError NullValue = new(
      "General.Null",
      "Null value was provided",
      ErrorType.Failure);

  public SDError(string code, string description, ErrorType type)
  {
    Code = code;
    Description = description;
    Type = type;
  }

  public string Code { get; }

  public string Description { get; }

  public ErrorType Type { get; }

  public static SDError Failure(string code, string description) =>
      new(code, description, ErrorType.Failure);

  public static SDError NotFound(string code, string description) =>
      new(code, description, ErrorType.NotFound);

  public static SDError Problem(string code, string description) =>
      new(code, description, ErrorType.Problem);

  public static SDError Conflict(string code, string description) =>
      new(code, description, ErrorType.Conflict);
}

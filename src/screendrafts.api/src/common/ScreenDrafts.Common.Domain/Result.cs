namespace ScreenDrafts.Common.Domain;

public class Result
{
  public Result(bool isSuccess, SDError error)
  {
    if (isSuccess && error != SDError.None ||
        !isSuccess && error == SDError.None)
    {
      throw new ArgumentException("Invalid error", nameof(error));
    }

    IsSuccess = isSuccess;
    Error = error;
  }

  public bool IsSuccess { get; }

  public bool IsFailure => !IsSuccess;

  public SDError Error { get; }

  public static Result Success() => new(true, SDError.None);

  public static Result<TValue> Success<TValue>(TValue value) =>
      new(value, true, SDError.None);

  public static Result Failure(SDError error) => new(false, error);

  public static Result<TValue> Failure<TValue>(SDError error) =>
      new(default, false, error);
}

public class Result<TValue>(TValue? value, bool isSuccess, SDError error) : Result(isSuccess, error)
{
  private readonly TValue? _value = value;

  [NotNull]
  public TValue Value => IsSuccess
    ? _value!
    : throw new InvalidOperationException("The value of a failure result can't be accessed.");

  public static implicit operator Result<TValue>(TValue? value) =>
    value is not null ? Success(value) : Failure<TValue>(SDError.NullValue);

  public static Result<TValue> ValidationFailure(SDError error) =>
      new(default, false, error);
}

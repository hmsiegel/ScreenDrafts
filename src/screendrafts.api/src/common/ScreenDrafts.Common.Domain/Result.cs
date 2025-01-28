namespace ScreenDrafts.Common.Domain;

public class Result
{
  protected internal Result(bool isSuccess, SDError error)
  {
    if (isSuccess && error != SDError.None ||
        !isSuccess && error == SDError.None)
    {
      throw new ArgumentException("Invalid error", nameof(error));
    }

    IsSuccess = isSuccess;
    Errors = [error];
  }

  protected internal Result(bool isSuccess, SDError[] errors)
  {
    IsSuccess = isSuccess;
    Errors = errors;
  }

  public bool IsSuccess { get; }

  public bool IsFailure => !IsSuccess;

  public SDError[] Errors { get; }

  public SDError?  Error { get; }

  public static Result Success() => new(true, SDError.None);

  public static Result<TValue> Success<TValue>(TValue value) =>
      new(value, true, SDError.None);

  public static Result Failure(SDError error) => new(false, error);

  public static Result Failure(SDError[] errors) => new(false, errors);

  public static Result<TValue> Failure<TValue>(SDError error) =>
      new(default, false, error);

  public static Result<TValue> Failure<TValue>(SDError[] errors) =>
      new(default, false, errors);

  public static Result<TValue> Create<TValue>(TValue value) =>
      value is not null ? Success(value) : Failure<TValue>(SDError.NullValue);

  public static Result<T> Ensure<T>(T value, Func<T, bool> predicate, SDError error)
  {
    ArgumentNullException.ThrowIfNull(predicate);

    return predicate(value)
      ? Success(value)
      : Failure<T>(error);
  }

  public static Result<T> Ensure<T>(T value, params (Func<T, bool> predicate, SDError error)[] functions)
  {
    ArgumentNullException.ThrowIfNull(functions);
    var results = new List<Result<T>>();

    foreach (var (predicate, error) in functions)
    {
      results.Add(Ensure(value, predicate, error));
    }
    return Combine(results.ToArray());
  }

  public static Result<T> Combine<T>(params Result<T>[] results)
  {
    ArgumentNullException.ThrowIfNull(results);

    if (results.Any(r => r.IsFailure))
    {
      return Failure<T>(
        results.SelectMany(r => r.Errors).Distinct().ToArray());
    }
    return Success(results[0].Value);
  }

  public static Result<(T1, T2)> Combine<T1, T2>(Result<T1> result1, Result<T2> result2)
  {
    ArgumentNullException.ThrowIfNull(result1);
    ArgumentNullException.ThrowIfNull(result2);

    if (result1.IsFailure)
    {
      return Failure<(T1, T2)>(result1.Errors[0]);
    }

    if (result2.IsFailure)
    {
      return Failure<(T1, T2)>(result2.Errors[0]);
    }

    return Success((result1.Value, result2.Value));
  }
}

public class Result<TValue> : Result
{
  private readonly TValue? _value;

  protected internal Result(TValue? value, bool isSuccess, SDError error)
    : base(isSuccess, error) =>
    _value = value;

  protected internal Result(TValue? value, bool isSuccess, SDError[] errors)
    : base(isSuccess, errors) =>
    _value = value;

  [NotNull]
  public TValue Value => IsSuccess
    ? _value!
    : throw new InvalidOperationException("The value of a failure result can't be accessed.");

  public static implicit operator Result<TValue>(TValue? value) =>
    value is not null ? Success(value) : Failure<TValue>(SDError.NullValue);

  public static Result<TValue> ValidationFailure(SDError error) =>
      new(default, false, error);
}

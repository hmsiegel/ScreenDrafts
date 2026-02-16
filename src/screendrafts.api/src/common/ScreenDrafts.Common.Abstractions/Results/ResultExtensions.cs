using ScreenDrafts.Common.Abstractions.Errors;

namespace ScreenDrafts.Common.Abstractions.Results;

public static class ResultExtensions
{
  public static TOut Match<TOut>(
      this Result result,
      Func<TOut> onSuccess,
      Func<Result, TOut> onFailure)
  {
    ArgumentNullException.ThrowIfNull(result);
    ArgumentNullException.ThrowIfNull(onSuccess);
    ArgumentNullException.ThrowIfNull(onFailure);

    return result.IsSuccess ? onSuccess() : onFailure(result);
  }

  public static TOut Match<TIn, TOut>(
      this Result<TIn> result,
      Func<TIn, TOut> onSuccess,
      Func<Result<TIn>, TOut> onFailure)
  {
    ArgumentNullException.ThrowIfNull(result);
    ArgumentNullException.ThrowIfNull(onSuccess);
    ArgumentNullException.ThrowIfNull(onFailure);

    return result.IsSuccess ? onSuccess(result.Value) : onFailure(result);
  }

  public static Result<T> Ensure<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        SDError error)
  {
    ArgumentNullException.ThrowIfNull(result);
    ArgumentNullException.ThrowIfNull(predicate);

    if (result.IsFailure)
    {
      return result;
    }

    return predicate(result.Value)
        ? result
        : Result.Failure<T>(error);
  }

  public static Result<TOut> Map<TIn, TOut>(
      this Result<TIn> result,
      Func<TIn, TOut> mappingFunc)
  {
    ArgumentNullException.ThrowIfNull(result);
    ArgumentNullException.ThrowIfNull(mappingFunc);

    return result.IsSuccess
        ? Result.Success(mappingFunc(result.Value))
        : Result.Failure<TOut>(result.Errors);
  }

  public static async Task<Result> BindAsync<TIn>(
      this Result<TIn> result,
             Func<TIn, Task<Result>> bindingFunc)
  {
    ArgumentNullException.ThrowIfNull(result);
    ArgumentNullException.ThrowIfNull(bindingFunc);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    return await bindingFunc(result.Value);
  }
  public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
      this Result<TIn> result,
      Func<TIn, Task<Result<TOut>>> bindingFunc)
  {
    ArgumentNullException.ThrowIfNull(result);
    ArgumentNullException.ThrowIfNull(bindingFunc);

    if (result.IsFailure)
    {
      return Result.Failure<TOut>(result.Errors);
    }

    return await bindingFunc(result.Value);
  }

  public static Result<TIn> Tap<TIn>(
      this Result<TIn> result,
      Action<TIn> action)
  {
    ArgumentNullException.ThrowIfNull(result);
    ArgumentNullException.ThrowIfNull(action);

    if (result.IsSuccess)
    {
      action(result.Value);
    }

    return result;
  }

  public static async Task<Result<TIn>> TapAsync<TIn>(
      this Result<TIn> result,
      Func<Task> func)
  {
    ArgumentNullException.ThrowIfNull(result);
    ArgumentNullException.ThrowIfNull(func);

    if (result.IsSuccess)
    {
      await func();
    }

    return result;
  }

  public static async Task<Result<TIn>> TapAsync<TIn>(
      this Task<Result<TIn>> resultTask,
      Func<TIn, Task> func)
  {
    ArgumentNullException.ThrowIfNull(resultTask);
    ArgumentNullException.ThrowIfNull(func);

    Result<TIn> result = await resultTask;

    if (result.IsSuccess)
    {
      await func(result.Value);
    }

    return result;
  }
}

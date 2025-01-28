namespace ScreenDrafts.Common.Domain;

public sealed record ValidationError : SDError
{
    public ValidationError(SDError[] errors)
        : base(
            "General.Validation",
            "One or more validation errors occurred",
            ErrorType.Validation)
    {
        Errors = errors;
    }

    public SDError[] Errors { get; }

    public static ValidationError FromResults(IEnumerable<Result> results) =>
        new(results.Where(r => r.IsFailure).Select(r => r.Errors[0]).ToArray());
}

namespace ScreenDrafts.Common.Domain;

public sealed record ValidationError : SDError
{
    public ValidationError(IReadOnlyList<SDError> errors)
        : base(
            "General.Validation",
            "One or more validation errors occurred",
            ErrorType.Validation)
    {
        Errors = errors;
    }

    public IReadOnlyList<SDError> Errors { get; }

    public static ValidationError FromResults(IEnumerable<Result> results) =>
        new(results.Where(r => r.IsFailure).Select(r => r.Error).ToList());
}

namespace ScreenDrafts.Domain.Common.DomainErrors;
public partial class Errors
{
    public static class Drafter
    {
        public static Error InvalidDrafterId => Error.Validation(
            "Drafter.Validation",
            "The provided drafter id is invalid.");
    }
}

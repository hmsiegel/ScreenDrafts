namespace ScreenDrafts.Domain.Common.DomainErrors;
public partial class Errors
{
    public static class Draft
    {
        public static Error InvalidDraftId => Error.Validation(
            "Draft.Validation",
            "The provided draft id is invalid.");
    }
}

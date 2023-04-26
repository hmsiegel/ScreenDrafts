using ErrorOr;

namespace ScreenDrafts.Domain.Common.Errors;
public partial class Errors
{
    public static class Draft
    {
        public static Error InvalidDraftId => Error.Validation(
            "Draft.Validation",
            "The provided draft id is invalid.");
    }

}

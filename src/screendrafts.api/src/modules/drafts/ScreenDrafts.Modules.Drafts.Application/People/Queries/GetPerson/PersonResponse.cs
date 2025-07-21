namespace ScreenDrafts.Modules.Drafts.Application.People.Queries.GetPerson;

public sealed record PersonResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string DisplayName,
    bool IsDrafter,
    bool IsHost)
{
    public PersonResponse()
        : this(
            Guid.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            false,
            false)
    {
    }
}

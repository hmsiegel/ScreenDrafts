namespace ScreenDrafts.Modules.Drafts.Application.People.Queries.GetPerson;

public sealed record PersonResponse(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string DisplayName,
    Guid DrafterId,
    Guid HostId,
    bool IsDrafter,
    bool IsHost)
{
    public PersonResponse()
        : this(
            Guid.Empty,
            Guid.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            Guid.Empty,
            Guid.Empty,
            false,
            false)
    {
    }
}

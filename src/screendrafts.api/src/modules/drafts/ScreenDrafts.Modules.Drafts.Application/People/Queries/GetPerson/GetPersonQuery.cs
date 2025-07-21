namespace ScreenDrafts.Modules.Drafts.Application.People.Queries.GetPerson;

public sealed record GetPersonQuery(Guid PersonId) : IQuery<PersonResponse>;

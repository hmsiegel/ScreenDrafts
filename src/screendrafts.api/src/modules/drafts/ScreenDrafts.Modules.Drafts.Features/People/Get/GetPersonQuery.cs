namespace ScreenDrafts.Modules.Drafts.Features.People.Get;

internal sealed record GetPersonQuery(string PublicId) : IQuery<PersonResponse>;


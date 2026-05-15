namespace ScreenDrafts.Modules.Drafts.Features.People.GetUserSocials;

public sealed record GetUserSocialsQuery(string PublicId) : IQuery<GetUserSocialsResponse?>;

namespace ScreenDrafts.Application.Drafters.Commands;
public sealed record CreateDrafterCommand(string UserId) : IRequest<Drafter>;

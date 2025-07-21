namespace ScreenDrafts.Modules.Drafts.Application.People.Commands.CreatePerson;

public sealed record CreatePersonCommand(string FirstName, string LastName, Guid? UserId = null) : ICommand<Guid>;

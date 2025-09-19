namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddReleaseToDraftPart;

public sealed record AddReleaseToDraftPartCommand(Guid DraftPartId, string ReleaseChannel, DateOnly ReleaseDate) : ICommand;

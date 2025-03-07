namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.UpdateReleaseDate;

public sealed record UpdateReleaseDateCommand(Guid DraftId, DateOnly ReleaseDate)
  : ICommand;

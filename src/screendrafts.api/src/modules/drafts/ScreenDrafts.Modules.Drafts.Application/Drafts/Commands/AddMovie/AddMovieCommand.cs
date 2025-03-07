namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddMovie;

public sealed record AddMovieCommand(Guid Id, string Title) : ICommand<Guid>;

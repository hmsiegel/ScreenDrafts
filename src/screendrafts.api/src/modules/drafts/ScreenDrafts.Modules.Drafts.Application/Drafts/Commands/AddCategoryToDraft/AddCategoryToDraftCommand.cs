namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddCategoryToDraft;

public sealed record AddCategoryToDraftCommand(Guid DraftId, Guid CategoryId) : ICommand<Guid>;

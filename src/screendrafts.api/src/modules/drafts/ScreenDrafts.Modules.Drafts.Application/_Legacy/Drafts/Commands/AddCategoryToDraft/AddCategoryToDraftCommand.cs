namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.AddCategoryToDraft;

public sealed record AddCategoryToDraftCommand(Guid DraftId, Guid CategoryId) : ICommand<Guid>;

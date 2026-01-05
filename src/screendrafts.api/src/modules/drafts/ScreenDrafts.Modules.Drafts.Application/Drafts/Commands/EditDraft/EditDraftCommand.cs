namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.EditDraft;

public sealed record EditDraftCommand(
  Guid DraftId,
  string Title,
  DraftType DraftType,
  string? Description) : ICommand;

namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetVeto;

public sealed record VetoDto(
  Guid Id,
  Guid PickId,
  Guid DrafterId,
  bool IsUsed);

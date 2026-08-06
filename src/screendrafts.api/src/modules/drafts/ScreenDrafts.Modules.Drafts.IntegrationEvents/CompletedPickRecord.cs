namespace ScreenDrafts.Modules.Drafts.IntegrationEvents;

public sealed record CompletedPickRecord(int Position, string MediaPublicId, string MediaTitle);

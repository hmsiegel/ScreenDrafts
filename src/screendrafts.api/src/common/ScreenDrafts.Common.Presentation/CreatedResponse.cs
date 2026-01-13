namespace ScreenDrafts.Common.Presentation;

public sealed record CreatedResponse(string PublicId);

public sealed record CreatedResponse<T>(T Data);

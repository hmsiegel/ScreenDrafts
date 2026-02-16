namespace ScreenDrafts.Common.Presentation.Responses;

public sealed record CreatedResponse(string PublicId);

public sealed record CreatedIdResponse(Guid Id);

public sealed record CreatedResponse<T>(T Data);

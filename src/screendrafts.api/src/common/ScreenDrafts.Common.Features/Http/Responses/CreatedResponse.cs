namespace ScreenDrafts.Common.Features.Http.Responses;

public sealed record CreatedResponse(string PublicId);

public sealed record CreatedResponse<T>(T Data);

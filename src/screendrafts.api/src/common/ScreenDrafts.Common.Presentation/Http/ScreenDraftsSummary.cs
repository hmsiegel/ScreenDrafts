namespace ScreenDrafts.Common.Presentation.Http;

public abstract class ScreenDraftsSummary<TEndpoint, TRequest> : Summary<TEndpoint>
  where TEndpoint : ScreenDraftsEndpoint<TRequest>
  where TRequest : notnull
{
}


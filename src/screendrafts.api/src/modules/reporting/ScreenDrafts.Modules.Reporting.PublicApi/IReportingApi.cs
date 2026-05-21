namespace ScreenDrafts.Modules.Reporting.PublicApi;

public interface IReportingApi
{
  Task<DrafterHonorificResponse?> GetDrafterHonorificAsync(
    Guid drafterInternalId,
    CancellationToken cancellationToken = default
  );

  Task<IReadOnlyList<Guid>> GetDrafterIdsByHonorificAsync(
    int honorificValue,
    CancellationToken cancellationToken = default
  );

  Task<MediaHonorificRecord?> GetMediaHonorificAsync(
    string mediaPublicId,
    CancellationToken ct = default
  );
}

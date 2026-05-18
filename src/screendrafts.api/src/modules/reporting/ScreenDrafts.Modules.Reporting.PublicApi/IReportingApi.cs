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
}

public sealed record DrafterHonorificResponse
{
  public required int HonorificValue { get; init; }
  public required string HonorificName { get; init; }
  public required int AppearanceCount { get; init; }
}

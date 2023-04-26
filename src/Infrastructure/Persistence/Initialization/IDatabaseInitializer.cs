namespace ScreenDrafts.Infrastructure.Persistence.Initialization;

internal interface IDatabaseInitializer
{
    Task InitializeDatabasesAsync(CancellationToken cancellationToken);
    Task InitializeApplicationDbForTenantAsync(ScreenDraftsTenantInfo tenant, CancellationToken cancellationToken);
}
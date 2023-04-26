using Finbuckle.MultiTenant.Stores;
using ScreenDrafts.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace ScreenDrafts.Infrastructure.Multitenancy;

public class TenantDbContext : EFCoreStoreDbContext<ScreenDraftsTenantInfo>
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options)
        : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ScreenDraftsTenantInfo>().ToTable("Tenants", SchemaNames.MultiTenancy);
    }
}
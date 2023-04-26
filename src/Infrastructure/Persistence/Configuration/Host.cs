using Host = ScreenDrafts.Domain.CommissionerAggregate.Host;

namespace ScreenDrafts.Infrastructure.Persistence.Configuration;
public sealed class HostConfiguration : IEntityTypeConfiguration<Host>
{
    public void Configure(EntityTypeBuilder<Host> builder)
    {
        ConfigureHostsTable(builder);
    }

    private static void ConfigureHostsTable(EntityTypeBuilder<Host> builder)
    {
        builder.ToTable(SchemaNames.Hosts)
            .IsMultiTenant();

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .HasConversion(
            x => x.Value,
            x => UserId.Create(x));
    }
}

namespace ScreenDrafts.Infrastructure.Persistence.Configuration;
public class DraftConfigurations : IEntityTypeConfiguration<Draft>
{
    public void Configure(EntityTypeBuilder<Draft> builder)
    {
        ConfigureDraftsTable(builder);
        ConfigureSelectedMoviesTable(builder);
        ConfigureDraftDrafterIdsTable(builder);
        ConfigureDraftHostIdsTable(builder);
    }

    private static void ConfigureDraftHostIdsTable(EntityTypeBuilder<Draft> builder)
    {
        builder.OwnsMany(d => d.Hosts, di =>
        {
            di.ToTable(nameof(TableNames.DraftHostIds));

            di.WithOwner()
                .HasForeignKey(ValueObjectNames.DraftId);

            di.Property(d => d.Value)
                .HasColumnName(ValueObjectNames.HostId)
                .ValueGeneratedNever();
        });

        builder.Metadata.FindNavigation(nameof(Draft.Hosts))
                        .SetPropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureDraftDrafterIdsTable(EntityTypeBuilder<Draft> builder)
    {
        builder.OwnsMany(d => d.DrafterIds, di =>
        {
            di.ToTable(nameof(TableNames.DraftDrafterIds));

            di.WithOwner()
                .HasForeignKey(ValueObjectNames.DraftId);

            di.Property(d => d.Value)
                .HasColumnName(ValueObjectNames.DrafterId)
                .ValueGeneratedNever();
        });

        builder.Metadata.FindNavigation(nameof(Draft.DrafterIds))
                        .SetPropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureSelectedMoviesTable(EntityTypeBuilder<Draft> builder)
    {
        builder.OwnsMany(d => d.SelectedMovies, sm =>
        {
            sm.ToTable(SchemaNames.SelectedMovies);

            sm.WithOwner()
                .HasForeignKey(ValueObjectNames.DraftId);

            sm.Property(x => x.DrafterId)
                .HasColumnName(ValueObjectNames.DrafterId)
                .IsRequired();

            sm.Property(x => x.MovieId)
                .HasColumnName(ValueObjectNames.MovieId)
                .IsRequired();

            sm.HasKey(
                DatabaseConstants.Id,
                ValueObjectNames.DraftId,
                ValueObjectNames.MovieId,
                ValueObjectNames.DrafterId,
                ValueObjectNames.DraftPosition);

            sm.Property(m => m.Id)
                .HasColumnName(ValueObjectNames.SelectedMovieId)
                .ValueGeneratedNever()
                .HasConversion(
                id => id.Value,
                value => SelectedMovieId.Create(value));

            sm.Property(m => m.MovieId)
                .HasColumnName(ValueObjectNames.MovieId)
                .ValueGeneratedNever()
                .HasConversion(
                id => id.Value,
                value => MovieId.Create(value));

            sm.Property(m => m.DrafterId)
                .HasColumnName(ValueObjectNames.DrafterId)
                .ValueGeneratedNever()
                .HasConversion(
                id => id.Value,
                value => DrafterId.Create(value));

            sm.Property(m => m.DrafterWhoPlayedVeto)
                .HasColumnName(ColumnNames.DrafterWhoPlayedVeto)
                .HasConversion(
                id => id.Value,
                value => DrafterId.Create(value));

            sm.Property(m => m.DrafterWhoPlayedVetoOverride)
                .HasColumnName(ColumnNames.DrafterWhoPlayedVetoOverride)
                .HasConversion(
                id => id.Value,
                value => DrafterId.Create(value));
        });

        builder.Metadata.FindNavigation(nameof(Draft.SelectedMovies))
                        .SetPropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureDraftsTable(EntityTypeBuilder<Draft> builder)
    {
        builder.ToTable(SchemaNames.Drafts)
             .IsMultiTenant();

        builder.HasKey(d => d.Id);

        builder.Property(d => d.DraftName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.DraftType)
            .IsRequired();
    }
}

namespace ScreenDrafts.Infrastructure.Persistence.Configuration;
public sealed class DrafterConfiguration : IEntityTypeConfiguration<Drafter>
{
    public void Configure(EntityTypeBuilder<Drafter> builder)
    {
        ConfigureDrafterTable(builder);
        ConfigureDrafterParticipatedDraftsTable(builder);
        ConfigureMovieListTable(builder);
    }

    private static void ConfigureMovieListTable(EntityTypeBuilder<Drafter> builder)
    {
        builder.OwnsMany(d => d.MoviesToDraft, mtd =>
        {
            mtd.ToTable(TableNames.DrafterMovieList);

            mtd.HasKey(
                DatabaseConstants.Id,
                ValueObjectNames.MovieId,
                ValueObjectNames.DraftId);

            mtd.Property(md => md.Id)
                .ValueGeneratedNever()
                .HasConversion(
                id => id.Value,
                value => MovieToDraftId.Create(value));

            mtd.Property(md => md.MovieId)
                .ValueGeneratedNever()
                .HasConversion(
                id => id.Value,
                value => MovieId.Create(value));

            mtd.Property(md => md.DraftId)
                .ValueGeneratedNever()
                .HasConversion(
                id => id.Value,
                value => DraftId.Create(value));
        });
    }

    private static void ConfigureDrafterParticipatedDraftsTable(EntityTypeBuilder<Drafter> builder)
    {
        builder.OwnsMany(d => d.ParticipatedDrafts, pd =>
        {
            pd.ToTable(TableNames.DrafterParticipatedDrafts);

            pd.WithOwner().HasForeignKey(ValueObjectNames.DrafterId);
        });

        builder.Metadata.FindNavigation(nameof(Drafter.ParticipatedDrafts))
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureDrafterTable(EntityTypeBuilder<Drafter> builder)
    {
        builder.ToTable(SchemaNames.Drafters)
            .IsMultiTenant();

        builder.HasKey(drafter => drafter.Id);

        builder.Property(dr => dr.UserId)
            .ValueGeneratedNever()
            .HasConversion(
            id => id.Value,
            value => UserId.Create(value));
    }
}

namespace ScreenDrafts.Modules.Drafts.Infrastructure.People;

internal sealed class DrafterConfiguration : IEntityTypeConfiguration<Drafter>
{
  public void Configure(EntityTypeBuilder<Drafter> builder)
  {
    builder.ToTable(Tables.Drafters);

    // Id
    builder.HasKey(drafter => drafter.Id);

    builder.Property(drafter => drafter.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DrafterIdConverter);

    // Public Id
    builder.Property(d => d.PublicId)
      .IsRequired()
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.HasIndex(d => d.PublicId)
      .IsUnique()
      .HasDatabaseName("ix_drafters_public_id");

    // Person Id
    builder.HasIndex(d => d.PersonId)
      .IsUnique();

    builder.HasOne(d => d.Person)
      .WithOne(p => p.DrafterProfile)
      .HasForeignKey<Drafter>(d => d.PersonId)
      .IsRequired()
      .OnDelete(DeleteBehavior.Restrict);

    builder.Property(d => d.IsRetired)
      .IsRequired();
  }
}

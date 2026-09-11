namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.DrafterTeams;

internal sealed class DrafterTeamConfiguration : IEntityTypeConfiguration<DrafterTeam>
{
  public void Configure(EntityTypeBuilder<DrafterTeam> builder)
  {
    builder.ToTable(Tables.DrafterTeams);

    builder.HasKey(t => t.Id);

    builder
      .Property(t => t.Id)
      .ValueGeneratedNever()
      .HasColumnName("id")
      .HasConversion(IdConverters.DrafterTeamIdConverter);

    builder.Property(t => t.PublicId).IsRequired().HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.HasIndex(t => t.PublicId).IsUnique();

    builder.Property(t => t.Name).IsRequired().HasMaxLength(DrafterTeam.TeamNameMaxLength);

    // Many-to-many, no payload on the join -- plain join table, matching
    // canonical DrafterTeamDrafter's shape.
    builder
      .HasMany(t => t.Drafters)
      .WithMany()
      .UsingEntity(j => j.ToTable(Tables.DrafterTeamMembers));

    builder.Navigation(t => t.Drafters).UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}

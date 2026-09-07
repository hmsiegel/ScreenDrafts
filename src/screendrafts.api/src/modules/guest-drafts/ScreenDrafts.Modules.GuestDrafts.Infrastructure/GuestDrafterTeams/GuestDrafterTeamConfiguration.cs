namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.GuestDrafterTeams;

internal sealed class GuestDrafterTeamConfiguration : IEntityTypeConfiguration<GuestDrafterTeam>
{
  public void Configure(EntityTypeBuilder<GuestDrafterTeam> builder)
  {
    builder.ToTable(Tables.GuestDrafterTeams);

    builder.HasKey(t => t.Id);

    builder
      .Property(t => t.Id)
      .ValueGeneratedNever()
      .HasColumnName("id")
      .HasConversion(IdConverters.GuestDrafterTeamIdConverter);

    builder.Property(t => t.PublicId).IsRequired().HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.HasIndex(t => t.PublicId).IsUnique();

    builder.Property(t => t.Name).IsRequired().HasMaxLength(GuestDrafterTeam.TeamNameMaxLength);

    // Many-to-many, no payload on the join -- plain join table, matching
    // canonical DrafterTeamDrafter's shape.
    builder
      .HasMany(t => t.Drafters)
      .WithMany()
      .UsingEntity(j => j.ToTable(Tables.GuestDrafterTeamMembers));

    builder.Navigation(t => t.Drafters).UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}

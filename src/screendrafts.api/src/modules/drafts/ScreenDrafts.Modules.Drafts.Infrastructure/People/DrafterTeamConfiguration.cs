namespace ScreenDrafts.Modules.Drafts.Infrastructure.People;

internal sealed class DrafterTeamConfiguration : IEntityTypeConfiguration<DrafterTeam>
{
  public void Configure(EntityTypeBuilder<DrafterTeam> builder)
  {
    builder.ToTable(Tables.DrafterTeams);

    // Id
    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
      .ValueGeneratedNever()
      .HasConversion(
      x => x.Value,
      value => DrafterTeamId.Create(value));

    // Name
    builder.Property(x => x.Name)
        .HasMaxLength(100)
        .IsRequired();

    builder.Property(x => x.NumberOfDrafters)
      .IsRequired();

    builder.HasMany(dt => dt.Drafters)
      .WithMany()
      .UsingEntity<Dictionary<string, object>>(
        Tables.DrafterTeamDrafter,
        x => x.HasOne<Drafter>().WithMany().HasForeignKey("drafter_id").OnDelete(DeleteBehavior.Cascade),
        x => x.HasOne<DrafterTeam>().WithMany().HasForeignKey("drafter_team_id").OnDelete(DeleteBehavior.Cascade),
        x =>
        {
          x.HasKey("drafter_id", "drafter_team_id");
          x.ToTable(Tables.DrafterTeamDrafter);
        });
  }
}

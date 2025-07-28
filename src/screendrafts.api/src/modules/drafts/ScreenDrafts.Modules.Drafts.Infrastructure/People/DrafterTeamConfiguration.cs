namespace ScreenDrafts.Modules.Drafts.Infrastructure.People;

internal sealed class DrafterTeamConfiguration : IEntityTypeConfiguration<DrafterTeam>
{
  public void Configure(EntityTypeBuilder<DrafterTeam> builder)
  {
    builder.ToTable(Tables.DrafterTeams);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
      .ValueGeneratedNever()
      .HasConversion(
      x => x.Value,
      value => DrafterTeamId.Create(value));

    builder.Property(x => x.Name)
        .HasMaxLength(100)
        .IsRequired();

    builder.HasMany(dt => dt.Picks)
      .WithOne(p => p.DrafterTeam)
      .HasForeignKey(p => p.DrafterTeamId);

    builder.HasOne(dt => dt.RolloverVeto)
      .WithOne(rv => rv.DrafterTeam)
      .HasForeignKey<RolloverVeto>(rv => rv.DrafterTeamId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(dt => dt.RolloverVetoOverride)
      .WithOne(rv => rv.DrafterTeam)
      .HasForeignKey<RolloverVetoOverride>(rv => rv.DrafterTeamId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(dt => dt.Drafts)
      .WithMany(d => d.DrafterTeams)
      .UsingEntity<Dictionary<string, string>>(
        Tables.DrafterTeamsDrafts,
        x => x.HasOne<Draft>().WithMany().HasForeignKey("draft_id").OnDelete(DeleteBehavior.Cascade),
        x => x.HasOne<DrafterTeam>().WithMany().HasForeignKey("drafter_team_id").OnDelete(DeleteBehavior.Cascade),
        x =>
        {
          x.HasKey("draft_id", "drafter_team_id");
          x.ToTable(Tables.DrafterTeamsDrafts);
        });

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

    builder.HasMany(d => d.DraftStats)
      .WithOne(ds => ds.DrafterTeam)
      .HasForeignKey(ds => ds.DrafterTeamId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Navigation(d => d.DraftStats)
      .HasField("_draftStats")
      .UsePropertyAccessMode(PropertyAccessMode.Field);
  }
}

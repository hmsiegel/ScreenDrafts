namespace ScreenDrafts.Modules.Drafts.Infrastructure.People;

internal sealed class DrafterConfiguration : IEntityTypeConfiguration<Drafter>
{
  public void Configure(EntityTypeBuilder<Drafter> builder)
  {
    builder.ToTable(Tables.Drafters);

    builder.HasKey(drafter => drafter.Id);

    builder.Property(drafter => drafter.Id)
      .ValueGeneratedNever()
      .HasConversion(
      id => id.Value,
      value => DrafterId.Create(value));

    builder.HasOne(d => d.Person)
      .WithOne(p => p.DrafterProfile)
      .HasForeignKey<Drafter>(d => d.PersonId);

    builder.Property(d => d.ReadableId)
      .ValueGeneratedOnAdd();

    builder.HasMany(d => d.Picks)
      .WithOne(p => p.Drafter)
      .HasForeignKey(d => d.DrafterId);

    builder.HasOne(d => d.RolloverVeto)
      .WithOne(rv => rv.Drafter)
      .HasForeignKey<RolloverVeto>(rv => rv.DrafterId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(d => d.RolloverVetoOverride)
      .WithOne(rv => rv.Drafter)
      .HasForeignKey<RolloverVetoOverride>(rv => rv.DrafterId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(d => d.Drafts)
      .WithMany(d => d.Drafters)
      .UsingEntity<Dictionary<string, string>>(
      Tables.DraftsDrafters,
      x => x.HasOne<Draft>().WithMany().HasForeignKey("draft_id").OnDelete(DeleteBehavior.Cascade),
      x => x.HasOne<Drafter>().WithMany().HasForeignKey("drafter_id").OnDelete(DeleteBehavior.Cascade),
      x =>
      {
        x.HasKey("draft_id", "drafter_id");
        x.ToTable(Tables.DraftsDrafters);
      });
  }
}

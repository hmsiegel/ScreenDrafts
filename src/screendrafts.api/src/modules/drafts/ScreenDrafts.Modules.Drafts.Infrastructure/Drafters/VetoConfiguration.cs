namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafters;
internal sealed class VetoConfiguration : IEntityTypeConfiguration<Veto>
{
  public void Configure(EntityTypeBuilder<Veto> builder)
  {
    builder.ToTable(Tables.Vetoes);

    builder.HasKey(veto => veto.Id);

    builder.Property(veto => veto.Id)
      .ValueGeneratedNever()
      .HasConversion(
      v => v.Value,
      v => VetoId.Create(v));

    builder.Property(v => v.IsUsed)
      .IsRequired();

    builder.Property(v => v.PickId)
      .IsRequired();

    builder.HasOne(v => v.Drafter)
      .WithMany(d => d.Vetoes)
      .HasForeignKey("drafterId")
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(v => v.Pick)
      .WithMany()
      .HasForeignKey("pickId")
      .OnDelete(DeleteBehavior.Cascade);
  } 
}

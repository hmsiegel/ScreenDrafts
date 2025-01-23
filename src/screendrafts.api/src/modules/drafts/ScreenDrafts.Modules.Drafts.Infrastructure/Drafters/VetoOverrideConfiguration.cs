using ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities;

namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafters;
internal sealed class VetoOverrideConfiguration : IEntityTypeConfiguration<VetoOverride>
{
  public void Configure(EntityTypeBuilder<VetoOverride> builder)
  {
    builder.ToTable(Tables.VetoOverrides);

    builder.HasKey(v => v.Id);

    builder.Property(v => v.Id)
      .ValueGeneratedNever()
      .HasConversion(
      v => v.Value,
      v => new VetoOverrideId(v));

    builder.Property(v => v.IsUsed)
      .IsRequired();

    builder.Property(vo => vo.VetoId)
      .IsRequired();

    builder.HasOne(vo => vo.Drafter)
      .WithMany(d => d.VetoOverrides)
      .HasForeignKey("drafterId");

    builder.HasOne(vo => vo.Veto)
      .WithOne(v => v.VetoOverride)
      .HasForeignKey<VetoOverride>("vetoId")
      .OnDelete(DeleteBehavior.Cascade);
  }
}

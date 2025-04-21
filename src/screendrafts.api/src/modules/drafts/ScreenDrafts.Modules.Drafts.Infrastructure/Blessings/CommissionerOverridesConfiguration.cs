namespace ScreenDrafts.Modules.Drafts.Infrastructure.Blessings;

internal sealed class CommissionerOverridesConfiguration : IEntityTypeConfiguration<CommissionerOverride>
{
  public void Configure(EntityTypeBuilder<CommissionerOverride> builder)
  {
    builder.ToTable(Tables.CommissionerOverrides);

    builder.HasKey(co => co.Id);

    builder.Property(co => co.PickId)
      .IsRequired()
      .HasConversion(
        pickId => pickId.Value,
        value => PickId.Create(value));

    builder.HasOne(co => co.Pick)
      .WithOne(p => p.CommissionerOverride)
      .HasForeignKey<CommissionerOverride>(co => co.PickId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}

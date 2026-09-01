namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.GuestDrafts;

internal sealed class GuestDraftCommissionerOverrideConfiguration
  : IEntityTypeConfiguration<GuestDraftCommissionerOverride>
{
  public void Configure(EntityTypeBuilder<GuestDraftCommissionerOverride> builder)
  {
    builder.ToTable(Tables.GuestDraftCommissionerOverrides);

    builder.HasKey(co => co.Id);

    builder.Property(co => co.Id)
      .ValueGeneratedNever()
      .HasColumnName("id")
      .HasConversion(IdConverters.GuestDraftCommissionerOverrideIdConverter);

    builder.Property(co => co.PickId)
      .IsRequired()
      .HasConversion(IdConverters.GuestDraftPickIdConverter);

    // Pick <-> CommissionerOverride 1:1, containment, cascades. Configured from
    // GuestDraftPickConfiguration's side (HasOne(p => p.CommissionerOverride)); not
    // repeated here to avoid configuring the same relationship from both ends.
  }
}

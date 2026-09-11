namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.Drafts;

internal sealed class CommissionerOverrideConfiguration
  : IEntityTypeConfiguration<CommissionerOverride>
{
  public void Configure(EntityTypeBuilder<CommissionerOverride> builder)
  {
    builder.ToTable(Tables.CommissionerOverrides);

    builder.HasKey(co => co.Id);

    builder
      .Property(co => co.Id)
      .ValueGeneratedNever()
      .HasColumnName("id")
      .HasConversion(IdConverters.CommissionerOverrideIdConverter);

    builder.Property(co => co.PickId).IsRequired().HasConversion(IdConverters.PickIdConverter);

    // Pick <-> CommissionerOverride 1:1, containment, cascades. Configured from
    // GuestDraftPickConfiguration's side (HasOne(p => p.CommissionerOverride)); not
    // repeated here to avoid configuring the same relationship from both ends.
  }
}

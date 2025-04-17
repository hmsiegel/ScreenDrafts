
namespace ScreenDrafts.Modules.Drafts.Infrastructure.CommissionerOverrides;

internal sealed class CommissionerOverridesConfiguration : IEntityTypeConfiguration<CommissionerOverride>
{
  public void Configure(EntityTypeBuilder<CommissionerOverride> builder)
  {
    builder.ToTable(Tables.CommissionerOverrides);

    builder.HasKey(co => co.Id);

    builder.HasOne(co => co.Pick)
      .WithOne()
      .HasForeignKey<CommissionerOverride>(co => co.PickId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Property(co => co.PickId)
      .IsRequired()
      .HasConversion(
        pickId => pickId.Value,
        value => PickId.Create(value));
  }
}

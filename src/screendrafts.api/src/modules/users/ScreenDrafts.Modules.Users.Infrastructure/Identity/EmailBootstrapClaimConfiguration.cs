namespace ScreenDrafts.Modules.Users.Infrastructure.Identity;

internal sealed class EmailBootstrapClaimConfiguration
  : IEntityTypeConfiguration<EmailBootstrapClaim>
{
  public void Configure(EntityTypeBuilder<EmailBootstrapClaim> builder)
  {
    builder.ToTable(Tables.EmailBootstrapClaims);

    builder.HasKey(x => x.Id);

    builder
      .Property(x => x.UserId)
      .HasConversion(id => id.Value, value => UserId.Create(value))
      .IsRequired();

    builder.HasIndex(x => x.UserId).IsUnique();

    builder.Property(x => x.IssuedAt).IsRequired();
    builder.Property(x => x.ExpiresAt).IsRequired();
    builder.Property(x => x.BatchLabel).HasMaxLength(100);
    builder.Property(x => x.ClaimedAt);
    builder.Property(x => x.ClaimedEmail).HasMaxLength(320);
  }
}

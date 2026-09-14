namespace ScreenDrafts.Modules.Users.Infrastructure.Users;

internal sealed class EmailChangeTokenConfiguration : IEntityTypeConfiguration<EmailChangeToken>
{
  public void Configure(EntityTypeBuilder<EmailChangeToken> builder)
  {
    builder.ToTable(Tables.EmailChangeTokens);

    builder.HasKey(x => x.Id);

    builder
      .Property(x => x.UserId)
      .HasConversion(id => id.Value, value => UserId.Create(value))
      .IsRequired();

    // Not unique on UserId — history of tokens per user is fine; "only one
    // active at a time" is enforced in the command handler (Invalidate on
    // re-request), not at the schema level.
    builder.HasIndex(x => x.UserId);
    builder.HasIndex(x => x.TokenHash).IsUnique();

    builder.Property(x => x.TokenHash).HasMaxLength(64).IsRequired(); // SHA-256 hex
    builder.Property(x => x.NewEmail).HasMaxLength(320).IsRequired();
    builder.Property(x => x.IssuedAt).IsRequired();
    builder.Property(x => x.ExpiresAt).IsRequired();
    builder.Property(x => x.UsedAt);
  }
}

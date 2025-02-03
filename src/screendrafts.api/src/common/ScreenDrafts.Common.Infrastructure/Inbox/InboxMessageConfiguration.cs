namespace ScreenDrafts.Common.Infrastructure.Inbox;

public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
  public void Configure(EntityTypeBuilder<InboxMessage> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable(TableNames.InboxMessages);

    builder.HasKey(x => x.Id);

    builder.Property(o => o.Content)
      .HasMaxLength(2000)
      .HasColumnType("jsonb");
  }
}

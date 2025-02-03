namespace ScreenDrafts.Common.Infrastructure.Inbox;

public sealed class InboxMessageConsumerConfiguration : IEntityTypeConfiguration<InboxMessageConsumer>
{
  public void Configure(EntityTypeBuilder<InboxMessageConsumer> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable(TableNames.InboxMessageConsumers);

    builder.HasKey(o => new { o.InboxMessageId, o.Name });

    builder.Property(o => o.Name)
      .HasMaxLength(500);
  }
}

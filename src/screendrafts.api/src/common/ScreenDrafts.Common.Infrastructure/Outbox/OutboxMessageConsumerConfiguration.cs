using ScreenDrafts.Common.Infrastructure.Inbox;

namespace ScreenDrafts.Common.Infrastructure.Outbox;

public sealed class OutboxMessageConsumerConfiguration : IEntityTypeConfiguration<OutboxMessageConsumer>
{
  public void Configure(EntityTypeBuilder<OutboxMessageConsumer> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable(TableNames.OutboxMessageConsumers);

    builder.HasKey(o => new { o.OutboxMessageId, o.Name });

    builder.Property(o => o.Name)
      .HasMaxLength(500);
  }
}

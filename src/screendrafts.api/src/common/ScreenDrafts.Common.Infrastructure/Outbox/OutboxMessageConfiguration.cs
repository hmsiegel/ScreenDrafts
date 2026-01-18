namespace ScreenDrafts.Common.Infrastructure.Outbox;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
  public void Configure(EntityTypeBuilder<OutboxMessage> builder)
  {
    ArgumentNullException.ThrowIfNull(builder);

    builder.ToTable(TableNames.OutboxMessages);

    builder.HasKey(x => x.Id);

    builder.Property(o => o.Content)
      .HasMaxLength(2000)
      .HasColumnType("jsonb");
  }
}

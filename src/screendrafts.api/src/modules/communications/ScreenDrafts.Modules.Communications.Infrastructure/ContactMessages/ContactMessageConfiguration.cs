namespace ScreenDrafts.Modules.Communications.Infrastructure.ContactMessages;

internal sealed class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
  public void Configure(EntityTypeBuilder<ContactMessage> builder)
  {
    builder.ToTable("contact_messages");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id).ValueGeneratedNever();

    builder.Property(x => x.Name).IsRequired().HasMaxLength(ContactMessage.MaxNameLength);

    builder.Property(x => x.Email).IsRequired().HasMaxLength(ContactMessage.MaxEmailLength);

    builder.Property(x => x.Category).IsRequired().HasMaxLength(20).HasConversion<string>();

    builder.Property(x => x.Message).IsRequired().HasMaxLength(ContactMessage.MaxMessageLength);

    builder.Property(x => x.UserPublicId).HasMaxLength(ContactMessage.MaxUserPublicIdLength);

    builder.Property(x => x.CreatedOnUtc).IsRequired();

    builder.Property(x => x.EmailedOnUtc);

    builder.HasIndex(x => x.CreatedOnUtc).HasDatabaseName("ix_contact_messages_created_on_utc");
  }
}

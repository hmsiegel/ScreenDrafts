using ScreenDrafts.Modules.GuestDrafts.Domain.Drafters;

namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.GuestDrafters;

internal sealed class GuestDrafterConfiguration : IEntityTypeConfiguration<Drafter>
{
  public void Configure(EntityTypeBuilder<Drafter> builder)
  {
    builder.ToTable(Tables.GuestDrafters);

    builder.HasKey(d => d.Id);

    builder
      .Property(d => d.Id)
      .ValueGeneratedNever()
      .HasColumnName("id")
      .HasConversion(IdConverters.GuestDrafterIdConverter);

    builder.Property(d => d.PublicId).IsRequired().HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.HasIndex(d => d.PublicId).IsUnique();

    builder.Property(d => d.UserId).IsRequired();

    // One GuestDrafter per User -- enforced at the DB level, not just by the
    // find-or-create logic in CreateGuestDrafterCommandHandler/
    // UserRegisteredIntegrationEventConsumer.
    builder.HasIndex(d => d.UserId).IsUnique();

    builder.Property(d => d.FirstName).IsRequired().HasMaxLength(100);

    builder.Property(d => d.LastName).IsRequired().HasMaxLength(100);

    // DisplayName is computed (FirstName + LastName), not persisted.
    builder.Ignore(d => d.DisplayName);
  }
}

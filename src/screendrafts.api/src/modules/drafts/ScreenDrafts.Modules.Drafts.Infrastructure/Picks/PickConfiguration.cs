using ScreenDrafts.Modules.Drafts.Domain.Drafters.Enums;

namespace ScreenDrafts.Modules.Drafts.Infrastructure.Picks;

internal sealed class PickConfiguration : IEntityTypeConfiguration<Pick>
{
  public void Configure(EntityTypeBuilder<Pick> builder)
  {
    builder.ToTable(Tables.Picks);

    // Identity
    builder.HasKey(p => p.Id);

    builder.Property(p => p.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftPickIdConverter);

    // Public Id
    builder.Property(p => p.PublicId)
      .IsRequired()
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);

    builder.HasIndex(p => p.PublicId)
      .IsUnique();

    // Draft Part
    builder.HasOne(p => p.DraftPart)
      .WithMany(d => d.Picks)
      .HasForeignKey(p => p.DraftPartId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Property(p => p.DraftPartId)
      .IsRequired()
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftPartIdConverter);

    // Movie
    builder.Property(p => p.MovieId)
      .IsRequired();

    builder.HasOne(p => p.Movie)
      .WithMany(m => m.Picks)
      .HasForeignKey(p => p.MovieId)
      .OnDelete(DeleteBehavior.Restrict);

    // Order/ Position
    builder.Property(p => p.Position)
      .IsRequired();

    builder.Property(p => p.PlayOrder)
      .IsRequired();

    // Uniqueness
    builder.HasIndex(p => new { p.DraftPartId, p.PlayOrder })
      .IsUnique();

    builder.HasIndex(p => new { p.DraftPartId, p.Position, p.PlayOrder })
      .IsUnique();

    // Participant
    builder.Property(p => p.PlayedById)
      .IsRequired();

    builder.Property(p => p.PlayedByKind)
      .IsRequired()
      .HasConversion(
        v => v.Value,
        v => ParticipantKind.FromValue(v));

    // Relationships
    builder.HasOne(p => p.Veto)
      .WithOne()
      .HasForeignKey<Veto>(v => v.PickId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(p => p.CommissionerOverride)
      .WithOne()
      .HasForeignKey<CommissionerOverride>(co => co.PickId)
      .OnDelete(DeleteBehavior.Cascade);

    // Ignored
    builder.Ignore(p => p.PlayedBy);
    builder.Ignore(p => p.IsVetoed);
    builder.Ignore(p => p.IsComissionerOverridden);
    builder.Ignore(p => p.IsActiveOnFinalBoard);
    builder.Ignore(p => p.History);
  }
}

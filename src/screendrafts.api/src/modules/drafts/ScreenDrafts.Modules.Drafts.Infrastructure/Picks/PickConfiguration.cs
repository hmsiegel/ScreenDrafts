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

    // Draft Part
    builder.HasOne(p => p.DraftPart)
      .WithMany("_picks")
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

    builder.Property(p => p.MovieVersionName)
      .HasColumnName("movie_version_name")
      .HasMaxLength(100)
      .IsRequired(false);

    // Order/ Position
    builder.Property(p => p.Position)
      .IsRequired();

    builder.Property(p => p.PlayOrder)
      .IsRequired();

    // Participant
    builder.Property(x => x.PlayedByParticipantId)
      .IsRequired()
      .ValueGeneratedNever()
      .HasConversion(IdConverters.DraftPartParticipantIdConverter);

    builder.HasOne(p => p.PlayedByParticipant)
      .WithMany(p => p.Picks)
      .HasForeignKey(p => p.PlayedByParticipantId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.Property(p => p.PlayedByParticipantKindValue)
      .IsRequired()
      .HasConversion(IdConverters.ParticipantKindConverter);

    builder.Property(p => p.PlayedByParticipantIdValue)
      .IsRequired();

    builder.HasIndex(x => new { x.DraftPartId, x.PlayedByParticipantIdValue, x.PlayedByParticipantKindValue });


    // Relationships
    builder.HasOne(p => p.Veto)
      .WithOne(v => v.TargetPick)
      .HasForeignKey<Veto>(v => v.TargetPickId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(p => p.CommissionerOverride)
      .WithOne(co => co.Pick)
      .HasForeignKey<CommissionerOverride>(co => co.PickId)
      .OnDelete(DeleteBehavior.Cascade);

    // Ignored
    builder.Ignore(p => p.IsVetoed);
    builder.Ignore(p => p.IsCommissionerOverridden);
    builder.Ignore(p => p.IsActiveOnFinalBoard);
    builder.Ignore(p => p.History);

    builder.HasIndex(x => new { x.DraftPartId, x.PlayOrder }).IsUnique();
    builder.HasIndex(x => new { x.DraftPartId, x.Position });
    builder.HasIndex(x => new { x.PlayedByParticipantId, x.PlayOrder });
  }
}

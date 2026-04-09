namespace ScreenDrafts.Modules.Drafts.Infrastructure.Predictions;

internal sealed class SurrogateAssignmentConfiguration : IEntityTypeConfiguration<SurrogateAssignment>
{
  public void Configure(EntityTypeBuilder<SurrogateAssignment> builder)
  {
    builder.ToTable(Tables.SurrogateAssignments);

    builder.HasKey(sa => sa.Id);

    builder.Property(sa => sa.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.SurrogateAssignmentIdConverter);

    builder.Property(sa => sa.PrimarySetId)
      .IsRequired()
      .HasConversion(IdConverters.DraftPredictionSetIdConverter);

    builder.Property(sa => sa.SurrogateSetId)
      .IsRequired()
      .HasConversion(IdConverters.DraftPredictionSetIdConverter);

    builder.Property(sa => sa.MergePolicy)
      .IsRequired()
      .HasConversion(EnumConverters.MergePolicyConverter);

    // Primary set relationship declared in DraftPredictionSetConfiguration.
    // Declare surrogate side here to avoid shadow-property conflicts.
    builder.HasOne(sa => sa.SurrogateSet)
      .WithMany()
      .HasForeignKey(sa => sa.SurrogateSetId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}

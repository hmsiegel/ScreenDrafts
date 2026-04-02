namespace ScreenDrafts.Modules.Drafts.Infrastructure.DraftParts;

internal sealed class SubDraftConfiguration : IEntityTypeConfiguration<SubDraft>
{
  public void Configure(EntityTypeBuilder<SubDraft> builder)
  {
    builder.ToTable(Tables.SubDrafts);

    builder.HasKey(s => s.Id);

    builder.Property(s => s.Id)
      .ValueGeneratedNever()
      .HasConversion(IdConverters.SubDraftIdConverter);

    builder.Property(s => s.PublicId)
      .IsRequired();

    builder.HasIndex(s => s.PublicId)
      .IsUnique();

    builder.Property(s => s.DraftPartId)
      .IsRequired()
      .HasConversion(IdConverters.DraftPartIdConverter);

    builder.HasIndex(s => new {s.DraftPartId, s.Index })
      .IsUnique() ;

    builder.Property(s => s.Index)
      .IsRequired();

    builder.Property(s => s.SubjectKind)
      .IsRequired()
      .HasConversion(
      sk => sk.Value,
      value => SubjectKind.FromValue(value));

    builder.Property(s => s.SubjectName)
      .IsRequired()
      .HasMaxLength(200);

    builder.Ignore(s => s.GameBoard);
  }
}

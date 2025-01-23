namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class PickConfiguration : IEntityTypeConfiguration<Pick>
{
  public void Configure(EntityTypeBuilder<Pick> builder)
  {
    builder.ToTable(Tables.Picks);

    builder.HasKey(p => p.Id);

    builder.Property(p => p.Id)
      .HasColumnName("pickId");

    builder.HasOne(p => p.Movie)
      .WithMany()
      .HasForeignKey("pickId");

    builder.HasOne(p => p.Drafter)
      .WithMany()
      .HasForeignKey("drafterId");
  }
}

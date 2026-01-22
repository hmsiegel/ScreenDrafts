namespace ScreenDrafts.Modules.Drafts.Infrastructure.Campaigns;

internal sealed class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
  public void Configure(EntityTypeBuilder<Campaign> builder)
  {
    builder.ToTable(Tables.Campaigns);

    builder.HasKey(x => x.Id);
    builder.Property(x => x.Id).ValueGeneratedNever();
    builder.HasIndex(x => x.Id);

    builder.Property(x => x.PublicId)
      .IsRequired()
      .HasMaxLength(PublicIdPrefixes.MaxPublicIdLength);
    builder.HasIndex(x => x.PublicId)
      .IsUnique();

    builder.Property(x => x.Slug)
      .IsRequired()
      .HasMaxLength(CampaignSlugs.MaxSlugLength);
    builder.HasIndex(x => x.Slug)
      .IsUnique();

    builder.Property(x => x.Name)
      .IsRequired()
      .HasMaxLength(Campaign.MaxNameLength);

    builder.Navigation(x => x.Drafts)
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    builder.HasMany<Draft>("_drafts")
      .WithOne(d => d.Campaign)
      .HasForeignKey(d => d.CampaignId)
      .OnDelete(DeleteBehavior.SetNull);
  }
}

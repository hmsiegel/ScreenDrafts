
namespace ScreenDrafts.Modules.Drafts.Infrastructure.Drafts;

internal sealed class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
  public void Configure(EntityTypeBuilder<Campaign> builder)
  {
    builder.ToTable(Tables.Campaigns);
    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id).ValueGeneratedNever();

    builder.Property(x => x.Name)
      .IsRequired();

    builder.Property(x => x.Slug)
      .IsRequired();

    builder.HasIndex(x => x.Id);
    builder.HasIndex(x => x.Slug)
      .IsUnique();
  }
}

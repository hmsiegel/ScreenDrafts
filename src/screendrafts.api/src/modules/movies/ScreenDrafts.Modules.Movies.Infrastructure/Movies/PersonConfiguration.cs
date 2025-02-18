
namespace ScreenDrafts.Modules.Movies.Infrastructure.Movies;

internal sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
  public void Configure(EntityTypeBuilder<Person> builder)
  {
    builder.ToTable(Tables.People);

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
      .ValueGeneratedNever()
      .HasConversion(
        x => x.Value,
        x => PersonId.Create(x));

    builder.Property(x => x.Name)
      .IsRequired()
      .HasMaxLength(100);
  }
}

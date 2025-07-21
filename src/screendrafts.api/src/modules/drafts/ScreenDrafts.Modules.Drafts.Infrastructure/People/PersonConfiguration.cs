namespace ScreenDrafts.Modules.Drafts.Infrastructure.People;
internal sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
  public void Configure(EntityTypeBuilder<Person> builder)
  {
    builder.ToTable(Tables.People);

    builder.HasKey(person => person.Id);

    builder.Property(person => person.Id)
      .ValueGeneratedNever()
      .HasConversion(
        id => id.Value,
        value => PersonId.Create(value));

    builder.HasOne(p => p.DrafterProfile)
      .WithOne(d => d.Person)
      .HasForeignKey<Drafter>(d => d.PersonId);

    builder.HasOne(p => p.HostProfile)
      .WithOne(h => h.Person)
      .HasForeignKey<Host>(h => h.PersonId);
  }
}

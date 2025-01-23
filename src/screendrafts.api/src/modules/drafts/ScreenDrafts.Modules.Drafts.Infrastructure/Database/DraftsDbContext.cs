﻿using System.Reflection;

using ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities;

namespace ScreenDrafts.Modules.Drafts.Infrastructure.Database;

public sealed class DraftsDbContext(DbContextOptions<DraftsDbContext> options)
  : DbContext(options), IUnitOfWork
{
  internal DbSet<Draft> Drafts { get; set; }

  internal DbSet<Drafter> Drafters { get; set; }

  internal DbSet<Host> Hosts { get; set; }

  internal DbSet<DrafterDraftStats> DrafterDraftStats { get; set; }

  internal DbSet<TriviaResult> TriviaResults { get; set; }

  internal DbSet<Pick> Picks { get; set; }

  internal DbSet<Veto> Vetoes { get; set; }

  internal DbSet<VetoOverride> VetoOverrides { get; set; }

  internal DbSet<RolloverVeto> RolloverVetoes { get; set; }

  internal DbSet<RolloverVetoOverride> RolloverVetoOverrides { get; set; }

  internal DbSet<Movie> Movies { get; set; }


  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    ArgumentNullException.ThrowIfNull(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

    modelBuilder.HasDefaultSchema(Schemas.Drafts);

  }

  protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
  {
    ArgumentNullException.ThrowIfNull(configurationBuilder);

    configurationBuilder.ConfigureSmartEnum();
  }
}

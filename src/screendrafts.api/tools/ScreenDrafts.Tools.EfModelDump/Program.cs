using System.Globalization;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ScreenDrafts.Common.Infrastructure.Database;
using ScreenDrafts.Common.Infrastructure.Outbox;
using ScreenDrafts.Modules.Drafts.Infrastructure.Database;

var services = new ServiceCollection();

var config = new ConfigurationBuilder()
  .AddJsonFile("appsettings.json", optional: false)
  .AddJsonFile("appsettings.Development.json", optional: true)
  .AddEnvironmentVariables()
  .Build();

services.AddSingleton<IConfiguration>(config);
services.AddSingleton<InsertOutboxMessagesInterceptor>();

services.AddDbContext<DraftsDbContext>((sp, options) =>
{
  options.UseModuleDefaults("Drafts", Schemas.Drafts, sp);
});

using var sp = services.BuildServiceProvider();

using var scope = sp.CreateScope();

using var context = scope.ServiceProvider.GetRequiredService<DraftsDbContext>();

var sb = new StringBuilder();

sb.AppendLine("=== EF CORE MODEL AUDIT: Drafts ===");
sb.AppendLine(CultureInfo.InvariantCulture, $"Generated: {DateTime.UtcNow:O}");
sb.AppendLine();

foreach (var entity in context.Model.GetEntityTypes()
         .OrderBy(e => e.GetSchema())
         .ThenBy(e => e.GetTableName()))
{
  DumpEntity(entity, sb);
}

var path = Path.Combine(Directory.GetCurrentDirectory(), "drafts-ef-targeted-dump.txt");
await File.WriteAllTextAsync(path, sb.ToString());

#pragma warning disable CA1303 // Do not pass literals as localized parameters
Console.WriteLine($"Targeted EF model dump written to:");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
Console.WriteLine(path);


static void DumpEntity(IEntityType entity, StringBuilder sb)
{
  sb.AppendLine("--------------------------------------------------");
  sb.AppendLine(CultureInfo.InvariantCulture, $"ENTITY: {entity.ClrType.FullName}");
  sb.AppendLine(CultureInfo.InvariantCulture, $"TABLE : {entity.GetSchema()}.{entity.GetTableName()}");

  // Primary key
  var pk = entity.FindPrimaryKey();
  if (pk is null)
  {
    sb.AppendLine("PK    : <NONE>");
  }
  else
  {
    sb.AppendLine(CultureInfo.InvariantCulture, $"PK    : {string.Join(", ", pk.Properties.Select(p => p.Name))}");
  }

  // Alternate keys
  foreach (var ak in entity.GetKeys().Where(k => !k.IsPrimaryKey()))
  {
    sb.AppendLine(CultureInfo.InvariantCulture, $"AK    : {string.Join(", ", ak.Properties.Select(p => p.Name))}");
  }

  // Properties
  sb.AppendLine("PROPERTIES:");
  foreach (var prop in entity.GetProperties().OrderBy(p => p.Name))
  {
    sb.AppendLine(
        CultureInfo.InvariantCulture,
        $"  - {prop.Name,-30} | " +
        $"{prop.ClrType.Name,-12} | " +
        $"Required={!prop.IsNullable,-5} | " +
        $"MaxLen={prop.GetMaxLength()?.ToString(CultureInfo.InvariantCulture) ?? "—",-4} | " +
        $"Conv={prop.GetValueConverter()?.GetType().Name ?? "—"}");
  }

  // Foreign keys
  sb.AppendLine("FOREIGN KEYS:");
  foreach (var fk in entity.GetForeignKeys())
  {
    sb.AppendLine(
        CultureInfo.InvariantCulture,
        $"  -> {fk.PrincipalEntityType.ClrType.Name} " +
        $"[{string.Join(", ", fk.Properties.Select(p => p.Name))}] | " +
        $"Required={fk.IsRequired,-5} | " +
        $"Delete={fk.DeleteBehavior}");
  }

  // Navigations
  sb.AppendLine("NAVIGATIONS:");
  foreach (var nav in entity.GetNavigations())
  {
    sb.AppendLine(
        CultureInfo.InvariantCulture,
        $"  - {nav.Name,-25} | " +
        $"{nav.TargetEntityType.ClrType.Name,-25} | " +
        $"Collection={nav.IsCollection}");
  }

  // Owned types
  var owned = entity.GetNavigations()
      .Where(n => n.TargetEntityType.IsOwned());

  foreach (var nav in owned)
  {
    sb.AppendLine(CultureInfo.InvariantCulture, $"OWNED : {nav.Name} -> {nav.TargetEntityType.ClrType.Name}");
  }

  // Indexes
  sb.AppendLine("INDEXES:");

  var storeObject = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());

  foreach (var idx in entity.GetIndexes().OrderBy(i => i.GetDatabaseName() ?? i.Name))
  {
    var dbName = idx.GetDatabaseName() ?? idx.Name;

    var propNames = string.Join(", ", idx.Properties.Select(p => p.Name));

    var colNames = string.Join(", ", idx.Properties.Select(p =>
    {
      return p.GetColumnName(storeObject) ?? p.Name;
    }));

    var filter = idx.GetFilter();

    sb.AppendLine(CultureInfo.InvariantCulture, $" - Name   : {dbName}");
    sb.AppendLine(CultureInfo.InvariantCulture, $"   Props  : ({propNames})");
    sb.AppendLine(CultureInfo.InvariantCulture, $"   Cols   : ({colNames})");
    sb.AppendLine(CultureInfo.InvariantCulture, $"   Unique : {idx.IsUnique}");
    sb.AppendLine(CultureInfo.InvariantCulture, $"   Filter : {(string.IsNullOrWhiteSpace(filter) ? "-" : filter)}");

  }

  sb.AppendLine();
}



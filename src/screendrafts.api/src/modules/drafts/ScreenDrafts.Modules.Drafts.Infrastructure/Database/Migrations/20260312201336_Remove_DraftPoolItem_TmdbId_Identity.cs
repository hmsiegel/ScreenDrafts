using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ScreenDrafts.Modules.Drafts.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class Remove_DraftPoolItem_TmdbId_Identity : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    ArgumentNullException.ThrowIfNull(migrationBuilder);

    migrationBuilder.AlterColumn<int>(
      name: "tmdb_id",
      schema: "drafts",
      table: "draft_pool_items",
      type: "integer",
      nullable: false,
      oldClrType: typeof(int),
      oldType: "integer")
      .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    ArgumentNullException.ThrowIfNull(migrationBuilder);

    migrationBuilder.AlterColumn<int>(
      name: "tmdb_id",
      schema: "drafts",
      table: "draft_pool_items",
      type: "integer",
      nullable: false,
      oldClrType: typeof(int),
      oldType: "integer")
      .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
  }
}

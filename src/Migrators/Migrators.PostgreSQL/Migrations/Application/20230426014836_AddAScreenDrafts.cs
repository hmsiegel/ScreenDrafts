using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Migrators.PostgreSQL.Migrations.Application
{
    /// <inheritdoc />
    public partial class AddAScreenDrafts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Drafters",
                schema: "Catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    HasRolloverVeto = table.Column<bool>(type: "boolean", nullable: false),
                    HasRolloverVetoOverride = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drafters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Drafts",
                schema: "Catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DraftName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DraftType = table.Column<int>(type: "integer", nullable: false),
                    NumberOfDrafters = table.Column<int>(type: "integer", nullable: false),
                    EpisodeReleaseDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RuntimeInMinutes = table.Column<int>(type: "integer", nullable: false),
                    EpisodeNumber = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drafts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Hosts",
                schema: "Catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    PredictionPoints = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hosts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Movies",
                schema: "Catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Year = table.Column<string>(type: "text", nullable: true),
                    Director = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ImdbUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsInMarqueeOfFame = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DrafterMovieList",
                schema: "Catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DraftId = table.Column<Guid>(type: "uuid", nullable: false),
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrafterId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrafterMovieList", x => new { x.Id, x.MovieId, x.DraftId });
                    table.ForeignKey(
                        name: "FK_DrafterMovieList_Drafters_DrafterId",
                        column: x => x.DrafterId,
                        principalSchema: "Catalog",
                        principalTable: "Drafters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DrafterParticipatedDrafts",
                schema: "Catalog",
                columns: table => new
                {
                    DrafterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrafterParticipatedDrafts", x => new { x.DrafterId, x.Id });
                    table.ForeignKey(
                        name: "FK_DrafterParticipatedDrafts_Drafters_DrafterId",
                        column: x => x.DrafterId,
                        principalSchema: "Catalog",
                        principalTable: "Drafters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DraftDrafterIds",
                schema: "Catalog",
                columns: table => new
                {
                    DraftId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DrafterId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftDrafterIds", x => new { x.DraftId, x.Id });
                    table.ForeignKey(
                        name: "FK_DraftDrafterIds_Drafts_DraftId",
                        column: x => x.DraftId,
                        principalSchema: "Catalog",
                        principalTable: "Drafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DraftHostIds",
                schema: "Catalog",
                columns: table => new
                {
                    DraftId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HostId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftHostIds", x => new { x.DraftId, x.Id });
                    table.ForeignKey(
                        name: "FK_DraftHostIds_Drafts_DraftId",
                        column: x => x.DraftId,
                        principalSchema: "Catalog",
                        principalTable: "Drafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SelectedMovies",
                schema: "Catalog",
                columns: table => new
                {
                    SelectedMovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    DraftPosition = table.Column<int>(type: "integer", nullable: false),
                    DrafterId = table.Column<Guid>(type: "uuid", nullable: false),
                    DraftId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsVetoed = table.Column<bool>(type: "boolean", nullable: false),
                    DrafterWhoPlayedVeto = table.Column<Guid>(type: "uuid", nullable: true),
                    WasVetoOverride = table.Column<bool>(type: "boolean", nullable: false),
                    DrafterWhoPlayedVetoOverride = table.Column<Guid>(type: "uuid", nullable: true),
                    WasCommissonerOverride = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelectedMovies", x => new { x.SelectedMovieId, x.DraftId, x.MovieId, x.DrafterId, x.DraftPosition });
                    table.ForeignKey(
                        name: "FK_SelectedMovies_Drafts_DraftId",
                        column: x => x.DraftId,
                        principalSchema: "Catalog",
                        principalTable: "Drafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MoviesDraftsSelectedInIds",
                schema: "Catalog",
                columns: table => new
                {
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DraftsSelectedInId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoviesDraftsSelectedInIds", x => new { x.MovieId, x.Id });
                    table.ForeignKey(
                        name: "FK_MoviesDraftsSelectedInIds_Movies_MovieId",
                        column: x => x.MovieId,
                        principalSchema: "Catalog",
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MoviesDraftsVetoedInIds",
                schema: "Catalog",
                columns: table => new
                {
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DraftsVetoedInId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoviesDraftsVetoedInIds", x => new { x.MovieId, x.Id });
                    table.ForeignKey(
                        name: "FK_MoviesDraftsVetoedInIds_Movies_MovieId",
                        column: x => x.MovieId,
                        principalSchema: "Catalog",
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DrafterMovieList_DrafterId",
                schema: "Catalog",
                table: "DrafterMovieList",
                column: "DrafterId");

            migrationBuilder.CreateIndex(
                name: "IX_SelectedMovies_DraftId",
                schema: "Catalog",
                table: "SelectedMovies",
                column: "DraftId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DraftDrafterIds",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "DrafterMovieList",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "DrafterParticipatedDrafts",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "DraftHostIds",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "Hosts",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "MoviesDraftsSelectedInIds",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "MoviesDraftsVetoedInIds",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "SelectedMovies",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "Drafters",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "Movies",
                schema: "Catalog");

            migrationBuilder.DropTable(
                name: "Drafts",
                schema: "Catalog");
        }
    }
}

﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ScreenDrafts.Modules.Drafts.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "drafts");

            migrationBuilder.CreateTable(
                name: "drafts",
                schema: "drafts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    readable_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    draft_type = table.Column<int>(type: "integer", nullable: false),
                    total_picks = table.Column<int>(type: "integer", nullable: false),
                    total_drafters = table.Column<int>(type: "integer", nullable: false),
                    total_hosts = table.Column<int>(type: "integer", nullable: false),
                    episode_number = table.Column<string>(type: "text", nullable: true),
                    draft_status = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_drafts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hosts",
                schema: "drafts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    host_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hosts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "movies",
                schema: "drafts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    movie_title = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_movies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "drafter_draft_stats",
                schema: "drafts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    drafter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    draft_id = table.Column<Guid>(type: "uuid", nullable: false),
                    starting_vetoes = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    starting_veto_overrides = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    rollovers_applied = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    trivia_vetoes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    trivia_veto_overrides = table.Column<int>(type: "integer", nullable: false),
                    draft_id1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_drafter_draft_stats", x => new { x.id, x.drafter_id, x.draft_id });
                    table.ForeignKey(
                        name: "fk_drafter_draft_stats_drafts_draft_id",
                        column: x => x.draft_id1,
                        principalSchema: "drafts",
                        principalTable: "drafts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "drafters",
                schema: "drafts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    readable_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    draft_id = table.Column<Guid>(type: "uuid", nullable: false),
                    draft_id1 = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_drafters", x => x.id);
                    table.ForeignKey(
                        name: "fk_drafters_drafts_draft_id",
                        column: x => x.draft_id1,
                        principalSchema: "drafts",
                        principalTable: "drafts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "game_boards",
                schema: "drafts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    draft_id = table.Column<Guid>(type: "uuid", nullable: false),
                    draft_id1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_game_boards", x => x.id);
                    table.ForeignKey(
                        name: "fk_game_boards_drafts_draft_id",
                        column: x => x.draft_id1,
                        principalSchema: "drafts",
                        principalTable: "drafts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trivia_results",
                schema: "drafts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    draft_id = table.Column<Guid>(type: "uuid", nullable: false),
                    drafter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    award_is_veto = table.Column<bool>(type: "boolean", nullable: false),
                    draft_id1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trivia_results", x => new { x.id, x.draft_id, x.drafter_id, x.position });
                    table.ForeignKey(
                        name: "fk_trivia_results_drafts_draft_id",
                        column: x => x.draft_id1,
                        principalSchema: "drafts",
                        principalTable: "drafts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "draft_host",
                schema: "drafts",
                columns: table => new
                {
                    hosted_drafts_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hosts_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_draft_host", x => new { x.hosted_drafts_id, x.hosts_id });
                    table.ForeignKey(
                        name: "fk_draft_host_drafts_hosted_drafts_id",
                        column: x => x.hosted_drafts_id,
                        principalSchema: "drafts",
                        principalTable: "drafts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_draft_host_hosts_hosts_id",
                        column: x => x.hosts_id,
                        principalSchema: "drafts",
                        principalTable: "hosts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "picks",
                schema: "drafts",
                columns: table => new
                {
                    pickId = table.Column<Guid>(type: "uuid", nullable: false),
                    pick_id = table.Column<Guid>(type: "uuid", nullable: true),
                    movie_id = table.Column<Guid>(type: "uuid", nullable: false),
                    drafter_id1 = table.Column<Guid>(type: "uuid", nullable: true),
                    drafter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    draft_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_picks", x => x.pickId);
                    table.ForeignKey(
                        name: "fk_picks_drafters_drafter_id",
                        column: x => x.drafter_id1,
                        principalSchema: "drafts",
                        principalTable: "drafters",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_picks_drafts_draft_id",
                        column: x => x.draft_id,
                        principalSchema: "drafts",
                        principalTable: "drafts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_picks_movies_pick_id",
                        column: x => x.pick_id,
                        principalSchema: "drafts",
                        principalTable: "movies",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "rollover_veto_overrides",
                schema: "drafts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    drafter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    drafter_id1 = table.Column<Guid>(type: "uuid", nullable: true),
                    from_draft_id = table.Column<Guid>(type: "uuid", nullable: false),
                    to_draft_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_used = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rollover_veto_overrides", x => x.id);
                    table.ForeignKey(
                        name: "fk_rollover_veto_overrides_drafters_drafter_id",
                        column: x => x.drafter_id1,
                        principalSchema: "drafts",
                        principalTable: "drafters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rollover_vetoes",
                schema: "drafts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    drafter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    drafter_id1 = table.Column<Guid>(type: "uuid", nullable: true),
                    from_draft_id = table.Column<Guid>(type: "uuid", nullable: false),
                    to_draft_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_used = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rollover_vetoes", x => x.id);
                    table.ForeignKey(
                        name: "fk_rollover_vetoes_drafters_drafter_id",
                        column: x => x.drafter_id1,
                        principalSchema: "drafts",
                        principalTable: "drafters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pick_assignments",
                schema: "drafts",
                columns: table => new
                {
                    position = table.Column<int>(type: "integer", nullable: false),
                    game_board_id = table.Column<Guid>(type: "uuid", nullable: false),
                    drafter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    extra_veto = table.Column<bool>(type: "boolean", nullable: false),
                    extra_veto_override = table.Column<bool>(type: "boolean", nullable: false),
                    game_board_id1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pick_assignments", x => new { x.game_board_id, x.position });
                    table.ForeignKey(
                        name: "fk_pick_assignments_game_boards_game_board_id",
                        column: x => x.game_board_id1,
                        principalSchema: "drafts",
                        principalTable: "game_boards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vetoes",
                schema: "drafts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    drafter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    drafter_id1 = table.Column<Guid>(type: "uuid", nullable: true),
                    pick_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pick_id1 = table.Column<Guid>(type: "uuid", nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vetoes", x => x.id);
                    table.ForeignKey(
                        name: "fk_vetoes_drafters_drafter_id",
                        column: x => x.drafter_id1,
                        principalSchema: "drafts",
                        principalTable: "drafters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vetoes_picks_pick_id",
                        column: x => x.pick_id1,
                        principalSchema: "drafts",
                        principalTable: "picks",
                        principalColumn: "pickId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "veto_overrides",
                schema: "drafts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    drafter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    drafter_id1 = table.Column<Guid>(type: "uuid", nullable: true),
                    veto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    veto_id1 = table.Column<Guid>(type: "uuid", nullable: true),
                    is_used = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_veto_overrides", x => x.id);
                    table.ForeignKey(
                        name: "fk_veto_overrides_drafters_drafter_id",
                        column: x => x.drafter_id1,
                        principalSchema: "drafts",
                        principalTable: "drafters",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_veto_overrides_vetoes_veto_id",
                        column: x => x.veto_id1,
                        principalSchema: "drafts",
                        principalTable: "vetoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_draft_host_hosts_id",
                schema: "drafts",
                table: "draft_host",
                column: "hosts_id");

            migrationBuilder.CreateIndex(
                name: "ix_drafter_draft_stats_draft_id",
                schema: "drafts",
                table: "drafter_draft_stats",
                column: "draft_id1");

            migrationBuilder.CreateIndex(
                name: "ix_drafters_draft_id",
                schema: "drafts",
                table: "drafters",
                column: "draft_id1");

            migrationBuilder.CreateIndex(
                name: "ix_game_boards_draft_id",
                schema: "drafts",
                table: "game_boards",
                column: "draft_id1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pick_assignments_game_board_id",
                schema: "drafts",
                table: "pick_assignments",
                column: "game_board_id1");

            migrationBuilder.CreateIndex(
                name: "ix_picks_draft_id",
                schema: "drafts",
                table: "picks",
                column: "draft_id");

            migrationBuilder.CreateIndex(
                name: "ix_picks_drafter_id",
                schema: "drafts",
                table: "picks",
                column: "drafter_id1");

            migrationBuilder.CreateIndex(
                name: "ix_picks_pick_id",
                schema: "drafts",
                table: "picks",
                column: "pick_id");

            migrationBuilder.CreateIndex(
                name: "ix_rollover_veto_overrides_drafter_id",
                schema: "drafts",
                table: "rollover_veto_overrides",
                column: "drafter_id1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rollover_vetoes_drafter_id",
                schema: "drafts",
                table: "rollover_vetoes",
                column: "drafter_id1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trivia_results_draft_id",
                schema: "drafts",
                table: "trivia_results",
                column: "draft_id1");

            migrationBuilder.CreateIndex(
                name: "ix_veto_overrides_drafter_id",
                schema: "drafts",
                table: "veto_overrides",
                column: "drafter_id1");

            migrationBuilder.CreateIndex(
                name: "ix_veto_overrides_veto_id",
                schema: "drafts",
                table: "veto_overrides",
                column: "veto_id1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vetoes_drafter_id",
                schema: "drafts",
                table: "vetoes",
                column: "drafter_id1");

            migrationBuilder.CreateIndex(
                name: "ix_vetoes_pick_id",
                schema: "drafts",
                table: "vetoes",
                column: "pick_id1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "draft_host",
                schema: "drafts");

            migrationBuilder.DropTable(
                name: "drafter_draft_stats",
                schema: "drafts");

            migrationBuilder.DropTable(
                name: "pick_assignments",
                schema: "drafts");

            migrationBuilder.DropTable(
                name: "rollover_veto_overrides",
                schema: "drafts");

            migrationBuilder.DropTable(
                name: "rollover_vetoes",
                schema: "drafts");

            migrationBuilder.DropTable(
                name: "trivia_results",
                schema: "drafts");

            migrationBuilder.DropTable(
                name: "veto_overrides",
                schema: "drafts");

            migrationBuilder.DropTable(
                name: "hosts",
                schema: "drafts");

            migrationBuilder.DropTable(
                name: "game_boards",
                schema: "drafts");

            migrationBuilder.DropTable(
                name: "vetoes",
                schema: "drafts");

            migrationBuilder.DropTable(
                name: "picks",
                schema: "drafts");

            migrationBuilder.DropTable(
                name: "drafters",
                schema: "drafts");

            migrationBuilder.DropTable(
                name: "movies",
                schema: "drafts");

            migrationBuilder.DropTable(
                name: "drafts",
                schema: "drafts");
        }
    }
}
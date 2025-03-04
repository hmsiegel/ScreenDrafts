﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ScreenDrafts.Modules.Users.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "users");

            migrationBuilder.CreateTable(
                name: "permissions",
                schema: "users",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissions", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "users",
                columns: table => new
                {
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    first_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    middle_name = table.Column<string>(type: "text", nullable: true),
                    last_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    identity_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "users",
                columns: table => new
                {
                    permission_code = table.Column<string>(type: "character varying(100)", nullable: false),
                    role_name = table.Column<string>(type: "character varying(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permissions", x => new { x.permission_code, x.role_name });
                    table.ForeignKey(
                        name: "fk_role_permissions_permissions_permission_code",
                        column: x => x.permission_code,
                        principalSchema: "users",
                        principalTable: "permissions",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_permissions_roles_role_name",
                        column: x => x.role_name,
                        principalSchema: "users",
                        principalTable: "roles",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "users",
                columns: table => new
                {
                    role_name = table.Column<string>(type: "character varying(50)", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => new { x.role_name, x.user_id });
                    table.ForeignKey(
                        name: "fk_user_roles_roles_roles_name",
                        column: x => x.role_name,
                        principalSchema: "users",
                        principalTable: "roles",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "users",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "users",
                table: "permissions",
                column: "code",
                values: new object[]
                {
                    "actors:search",
                    "crew:search",
                    "drafters:add",
                    "drafters:read",
                    "drafters:remove",
                    "drafters:update",
                    "drafts:create",
                    "drafts:read",
                    "drafts:search",
                    "drafts:update",
                    "genres:search",
                    "hosts:add",
                    "hosts:read",
                    "hosts:remove",
                    "hosts:update",
                    "movies:search",
                    "permissions:read",
                    "permissions:update",
                    "picks:add",
                    "picks:veto",
                    "picks:veto-override",
                    "roles:read",
                    "roles:update",
                    "studios:search",
                    "users:read",
                    "users:update"
                });

            migrationBuilder.InsertData(
                schema: "users",
                table: "roles",
                column: "name",
                values: new object[]
                {
                    "Administrator",
                    "Drafter",
                    "Guest",
                    "Host",
                    "SuperAdministrator"
                });

            migrationBuilder.InsertData(
                schema: "users",
                table: "role_permissions",
                columns: new[] { "permission_code", "role_name" },
                values: new object[,]
                {
                    { "actors:search", "Administrator" },
                    { "actors:search", "Drafter" },
                    { "actors:search", "Guest" },
                    { "actors:search", "Host" },
                    { "actors:search", "SuperAdministrator" },
                    { "crew:search", "Administrator" },
                    { "crew:search", "Drafter" },
                    { "crew:search", "Guest" },
                    { "crew:search", "Host" },
                    { "crew:search", "SuperAdministrator" },
                    { "drafters:add", "Administrator" },
                    { "drafters:add", "SuperAdministrator" },
                    { "drafters:read", "Administrator" },
                    { "drafters:read", "SuperAdministrator" },
                    { "drafters:remove", "Administrator" },
                    { "drafters:remove", "SuperAdministrator" },
                    { "drafters:update", "Administrator" },
                    { "drafters:update", "SuperAdministrator" },
                    { "drafts:create", "Administrator" },
                    { "drafts:create", "SuperAdministrator" },
                    { "drafts:read", "Administrator" },
                    { "drafts:read", "Host" },
                    { "drafts:read", "SuperAdministrator" },
                    { "drafts:search", "Administrator" },
                    { "drafts:search", "Drafter" },
                    { "drafts:search", "Guest" },
                    { "drafts:search", "Host" },
                    { "drafts:search", "SuperAdministrator" },
                    { "drafts:update", "Administrator" },
                    { "drafts:update", "SuperAdministrator" },
                    { "genres:search", "Administrator" },
                    { "genres:search", "Drafter" },
                    { "genres:search", "Guest" },
                    { "genres:search", "Host" },
                    { "genres:search", "SuperAdministrator" },
                    { "hosts:add", "Administrator" },
                    { "hosts:add", "SuperAdministrator" },
                    { "hosts:read", "Administrator" },
                    { "hosts:read", "SuperAdministrator" },
                    { "hosts:remove", "Administrator" },
                    { "hosts:remove", "SuperAdministrator" },
                    { "hosts:update", "Administrator" },
                    { "hosts:update", "SuperAdministrator" },
                    { "movies:search", "Administrator" },
                    { "movies:search", "Drafter" },
                    { "movies:search", "Guest" },
                    { "movies:search", "Host" },
                    { "movies:search", "SuperAdministrator" },
                    { "permissions:read", "SuperAdministrator" },
                    { "permissions:update", "SuperAdministrator" },
                    { "picks:add", "Administrator" },
                    { "picks:add", "Drafter" },
                    { "picks:add", "SuperAdministrator" },
                    { "picks:veto", "Administrator" },
                    { "picks:veto", "Drafter" },
                    { "picks:veto", "SuperAdministrator" },
                    { "picks:veto-override", "Administrator" },
                    { "picks:veto-override", "Drafter" },
                    { "picks:veto-override", "SuperAdministrator" },
                    { "roles:read", "SuperAdministrator" },
                    { "roles:update", "SuperAdministrator" },
                    { "studios:search", "Administrator" },
                    { "studios:search", "Drafter" },
                    { "studios:search", "Guest" },
                    { "studios:search", "Host" },
                    { "studios:search", "SuperAdministrator" },
                    { "users:read", "Administrator" },
                    { "users:read", "Drafter" },
                    { "users:read", "Guest" },
                    { "users:read", "Host" },
                    { "users:read", "SuperAdministrator" },
                    { "users:update", "Administrator" },
                    { "users:update", "Drafter" },
                    { "users:update", "Guest" },
                    { "users:update", "Host" },
                    { "users:update", "SuperAdministrator" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_role_name",
                schema: "users",
                table: "role_permissions",
                column: "role_name");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_user_id",
                schema: "users",
                table: "user_roles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "users",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_identity_id",
                schema: "users",
                table: "users",
                column: "identity_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "users");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "users");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "users");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "users");

            migrationBuilder.DropTable(
                name: "users",
                schema: "users");
        }
    }
}

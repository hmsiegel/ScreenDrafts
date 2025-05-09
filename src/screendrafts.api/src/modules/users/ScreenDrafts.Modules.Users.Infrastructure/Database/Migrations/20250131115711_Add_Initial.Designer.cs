﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ScreenDrafts.Modules.Users.Infrastructure.Database.Migrations
{
    [DbContext(typeof(UsersDbContext))]
    [Migration("20250131115711_Add_Initial")]
    partial class Add_Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("users")
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("PermissionRole", b =>
                {
                    b.Property<string>("PermissionCode")
                        .HasColumnType("character varying(100)")
                        .HasColumnName("permission_code");

                    b.Property<string>("RoleName")
                        .HasColumnType("character varying(50)")
                        .HasColumnName("role_name");

                    b.HasKey("PermissionCode", "RoleName")
                        .HasName("pk_role_permissions");

                    b.HasIndex("RoleName")
                        .HasDatabaseName("ix_role_permissions_role_name");

                    b.ToTable("role_permissions", "users");

                    b.HasData(
                        new
                        {
                            PermissionCode = "users:read",
                            RoleName = "Guest"
                        },
                        new
                        {
                            PermissionCode = "drafts:search",
                            RoleName = "Guest"
                        },
                        new
                        {
                            PermissionCode = "users:update",
                            RoleName = "Guest"
                        },
                        new
                        {
                            PermissionCode = "movies:search",
                            RoleName = "Guest"
                        },
                        new
                        {
                            PermissionCode = "actors:search",
                            RoleName = "Guest"
                        },
                        new
                        {
                            PermissionCode = "crew:search",
                            RoleName = "Guest"
                        },
                        new
                        {
                            PermissionCode = "genres:search",
                            RoleName = "Guest"
                        },
                        new
                        {
                            PermissionCode = "studios:search",
                            RoleName = "Guest"
                        },
                        new
                        {
                            PermissionCode = "users:read",
                            RoleName = "Host"
                        },
                        new
                        {
                            PermissionCode = "drafts:search",
                            RoleName = "Host"
                        },
                        new
                        {
                            PermissionCode = "users:update",
                            RoleName = "Host"
                        },
                        new
                        {
                            PermissionCode = "drafts:read",
                            RoleName = "Host"
                        },
                        new
                        {
                            PermissionCode = "movies:search",
                            RoleName = "Host"
                        },
                        new
                        {
                            PermissionCode = "actors:search",
                            RoleName = "Host"
                        },
                        new
                        {
                            PermissionCode = "crew:search",
                            RoleName = "Host"
                        },
                        new
                        {
                            PermissionCode = "genres:search",
                            RoleName = "Host"
                        },
                        new
                        {
                            PermissionCode = "studios:search",
                            RoleName = "Host"
                        },
                        new
                        {
                            PermissionCode = "users:read",
                            RoleName = "Drafter"
                        },
                        new
                        {
                            PermissionCode = "users:update",
                            RoleName = "Drafter"
                        },
                        new
                        {
                            PermissionCode = "drafts:search",
                            RoleName = "Drafter"
                        },
                        new
                        {
                            PermissionCode = "picks:add",
                            RoleName = "Drafter"
                        },
                        new
                        {
                            PermissionCode = "picks:veto",
                            RoleName = "Drafter"
                        },
                        new
                        {
                            PermissionCode = "picks:veto-override",
                            RoleName = "Drafter"
                        },
                        new
                        {
                            PermissionCode = "movies:search",
                            RoleName = "Drafter"
                        },
                        new
                        {
                            PermissionCode = "actors:search",
                            RoleName = "Drafter"
                        },
                        new
                        {
                            PermissionCode = "crew:search",
                            RoleName = "Drafter"
                        },
                        new
                        {
                            PermissionCode = "genres:search",
                            RoleName = "Drafter"
                        },
                        new
                        {
                            PermissionCode = "studios:search",
                            RoleName = "Drafter"
                        },
                        new
                        {
                            PermissionCode = "users:read",
                            RoleName = "Administrator"
                        },
                        new
                        {
                            PermissionCode = "users:update",
                            RoleName = "Administrator"
                        },
                        new
                        {
                            PermissionCode = "drafts:read",
                            RoleName = "Administrator"
                        },
                        new
                        {
                            PermissionCode = "drafts:create",
                            RoleName = "Administrator"
                        },
                        new
                        {
                            PermissionCode = "drafts:search",
                            RoleName = "Administrator"
                        },
                        new
                        {
                            PermissionCode = "drafts:update",
                            RoleName = "Administrator"
                        },
                        new
                        {
                            PermissionCode = "picks:add",
                            RoleName = "Administrator"
                        },
                        new
                        {
                            PermissionCode = "picks:veto",
                            RoleName = "Administrator"
                        },
                        new
                        {
                            PermissionCode = "picks:veto-override",
                            RoleName = "Administrator"
                        },
                        new
                        {
                            PermissionCode = "drafters:add",
                            RoleName = "Administrator"
                        },
                        new
                        {
                            PermissionCode = "drafters:remove",
                            RoleName = "Administrator"
                        },
                        new
                        {
                            PermissionCode = "drafters:update",
                            RoleName = "Administrator"
                        },
                        new
                        {
                            PermissionCode = "drafters:read",
                            RoleName = "Administrator"
                        },
                        new
                        {
                            PermissionCode = "hosts:add",
                            RoleName = "Administrator"
                        },
                        new
                        {
                            PermissionCode = "hosts:remove",
                            RoleName = "Administrator"
                        },
                        new
                        {
                            PermissionCode = "hosts:update",
                            RoleName = "Administrator"
                        },
                        new
                        {
                            PermissionCode = "hosts:read",
                            RoleName = "Administrator"
                        },
                        new
                        {
                            PermissionCode = "movies:search",
                            RoleName = "Administrator"
                        },
                        new
                        {
                            PermissionCode = "actors:search",
                            RoleName = "Administrator"
                        },
                        new
                        {
                            PermissionCode = "crew:search",
                            RoleName = "Administrator"
                        },
                        new
                        {
                            PermissionCode = "genres:search",
                            RoleName = "Administrator"
                        },
                        new
                        {
                            PermissionCode = "studios:search",
                            RoleName = "Administrator"
                        },
                        new
                        {
                            PermissionCode = "users:read",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "users:update",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "drafts:read",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "drafts:create",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "drafts:search",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "drafts:update",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "picks:add",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "picks:veto",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "picks:veto-override",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "drafters:add",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "drafters:remove",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "drafters:update",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "drafters:read",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "hosts:add",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "hosts:remove",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "hosts:update",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "hosts:read",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "movies:search",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "actors:search",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "crew:search",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "genres:search",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "studios:search",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "roles:read",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "roles:update",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "permissions:read",
                            RoleName = "SuperAdministrator"
                        },
                        new
                        {
                            PermissionCode = "permissions:update",
                            RoleName = "SuperAdministrator"
                        });
                });

            modelBuilder.Entity("RoleUser", b =>
                {
                    b.Property<string>("RolesName")
                        .HasColumnType("character varying(50)")
                        .HasColumnName("role_name");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.HasKey("RolesName", "UserId")
                        .HasName("pk_user_roles");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_user_roles_user_id");

                    b.ToTable("user_roles", "users");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Users.Domain.Users.Permission", b =>
                {
                    b.Property<string>("Code")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("code");

                    b.HasKey("Code")
                        .HasName("pk_permissions");

                    b.ToTable("permissions", "users");

                    b.HasData(
                        new
                        {
                            Code = "users:read"
                        },
                        new
                        {
                            Code = "users:update"
                        },
                        new
                        {
                            Code = "drafts:read"
                        },
                        new
                        {
                            Code = "drafts:create"
                        },
                        new
                        {
                            Code = "drafts:search"
                        },
                        new
                        {
                            Code = "drafts:update"
                        },
                        new
                        {
                            Code = "picks:add"
                        },
                        new
                        {
                            Code = "picks:veto"
                        },
                        new
                        {
                            Code = "picks:veto-override"
                        },
                        new
                        {
                            Code = "drafters:add"
                        },
                        new
                        {
                            Code = "drafters:remove"
                        },
                        new
                        {
                            Code = "drafters:update"
                        },
                        new
                        {
                            Code = "drafters:read"
                        },
                        new
                        {
                            Code = "roles:read"
                        },
                        new
                        {
                            Code = "roles:update"
                        },
                        new
                        {
                            Code = "permissions:read"
                        },
                        new
                        {
                            Code = "permissions:update"
                        },
                        new
                        {
                            Code = "hosts:add"
                        },
                        new
                        {
                            Code = "hosts:remove"
                        },
                        new
                        {
                            Code = "hosts:update"
                        },
                        new
                        {
                            Code = "hosts:read"
                        },
                        new
                        {
                            Code = "movies:search"
                        },
                        new
                        {
                            Code = "actors:search"
                        },
                        new
                        {
                            Code = "crew:search"
                        },
                        new
                        {
                            Code = "genres:search"
                        },
                        new
                        {
                            Code = "studios:search"
                        });
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Users.Domain.Users.Role", b =>
                {
                    b.Property<string>("Name")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("name");

                    b.HasKey("Name")
                        .HasName("pk_roles");

                    b.ToTable("roles", "users");

                    b.HasData(
                        new
                        {
                            Name = "SuperAdministrator"
                        },
                        new
                        {
                            Name = "Administrator"
                        },
                        new
                        {
                            Name = "Guest"
                        },
                        new
                        {
                            Name = "Host"
                        },
                        new
                        {
                            Name = "Drafter"
                        });
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Users.Domain.Users.User", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("email");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("first_name");

                    b.Property<string>("IdentityId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("identity_id");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("last_name");

                    b.Property<string>("MiddleName")
                        .HasColumnType("text")
                        .HasColumnName("middle_name");

                    b.HasKey("Id")
                        .HasName("pk_users");

                    b.HasIndex("Email")
                        .IsUnique()
                        .HasDatabaseName("ix_users_email");

                    b.HasIndex("IdentityId")
                        .IsUnique()
                        .HasDatabaseName("ix_users_identity_id");

                    b.ToTable("users", "users");
                });

            modelBuilder.Entity("PermissionRole", b =>
                {
                    b.HasOne("ScreenDrafts.Modules.Users.Domain.Users.Permission", null)
                        .WithMany()
                        .HasForeignKey("PermissionCode")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_role_permissions_permissions_permission_code");

                    b.HasOne("ScreenDrafts.Modules.Users.Domain.Users.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_role_permissions_roles_role_name");
                });

            modelBuilder.Entity("RoleUser", b =>
                {
                    b.HasOne("ScreenDrafts.Modules.Users.Domain.Users.Role", null)
                        .WithMany()
                        .HasForeignKey("RolesName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_user_roles_roles_roles_name");

                    b.HasOne("ScreenDrafts.Modules.Users.Domain.Users.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_user_roles_users_user_id");
                });
#pragma warning restore 612, 618
        }
    }
}

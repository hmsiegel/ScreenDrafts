﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ScreenDrafts.Modules.Drafts.Infrastructure.Database;

#nullable disable

namespace ScreenDrafts.Modules.Drafts.Infrastructure.Database.Migrations
{
    [DbContext(typeof(DraftsDbContext))]
    [Migration("20250214201400_AddMultipleReleaseDates")]
    partial class AddMultipleReleaseDates
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("drafts")
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("DraftDrafter", b =>
                {
                    b.Property<Guid>("DraftersId")
                        .HasColumnType("uuid")
                        .HasColumnName("drafters_id");

                    b.Property<Guid>("DraftsId")
                        .HasColumnType("uuid")
                        .HasColumnName("drafts_id");

                    b.HasKey("DraftersId", "DraftsId")
                        .HasName("pk_draft_drafter");

                    b.HasIndex("DraftsId")
                        .HasDatabaseName("ix_draft_drafter_drafts_id");

                    b.ToTable("draft_drafter", "drafts");
                });

            modelBuilder.Entity("DraftHost", b =>
                {
                    b.Property<Guid>("HostedDraftsId")
                        .HasColumnType("uuid")
                        .HasColumnName("hosted_drafts_id");

                    b.Property<Guid>("HostsId")
                        .HasColumnType("uuid")
                        .HasColumnName("hosts_id");

                    b.HasKey("HostedDraftsId", "HostsId")
                        .HasName("pk_draft_host");

                    b.HasIndex("HostsId")
                        .HasDatabaseName("ix_draft_host_hosts_id");

                    b.ToTable("draft_host", "drafts");
                });

            modelBuilder.Entity("ScreenDrafts.Common.Infrastructure.Inbox.InboxMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("jsonb")
                        .HasColumnName("content");

                    b.Property<string>("Error")
                        .HasColumnType("text")
                        .HasColumnName("error");

                    b.Property<DateTime>("OccurredOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("occurred_on_utc");

                    b.Property<DateTime?>("ProcessedOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("processed_on_utc");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_inbox_messages");

                    b.ToTable("inbox_messages", "drafts");
                });

            modelBuilder.Entity("ScreenDrafts.Common.Infrastructure.Inbox.InboxMessageConsumer", b =>
                {
                    b.Property<Guid>("InboxMessageId")
                        .HasColumnType("uuid")
                        .HasColumnName("inbox_message_id");

                    b.Property<string>("Name")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("name");

                    b.HasKey("InboxMessageId", "Name")
                        .HasName("pk_inbox_message_consumers");

                    b.ToTable("inbox_message_consumers", "drafts");
                });

            modelBuilder.Entity("ScreenDrafts.Common.Infrastructure.Outbox.OutboxMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("jsonb")
                        .HasColumnName("content");

                    b.Property<string>("Error")
                        .HasColumnType("text")
                        .HasColumnName("error");

                    b.Property<DateTime>("OccurredOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("occurred_on_utc");

                    b.Property<DateTime?>("ProcessedOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("processed_on_utc");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_outbox_messages");

                    b.ToTable("outbox_messages", "drafts");
                });

            modelBuilder.Entity("ScreenDrafts.Common.Infrastructure.Outbox.OutboxMessageConsumer", b =>
                {
                    b.Property<Guid>("OutboxMessageId")
                        .HasColumnType("uuid")
                        .HasColumnName("outbox_message_id");

                    b.Property<string>("Name")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("name");

                    b.HasKey("OutboxMessageId", "Name")
                        .HasName("pk_outbox_message_consumers");

                    b.ToTable("outbox_message_consumers", "drafts");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafters.Drafter", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("name");

                    b.Property<int>("ReadableId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("readable_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ReadableId"));

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_drafters");

                    b.ToTable("drafters", "drafts");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities.RolloverVeto", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("DrafterId")
                        .HasColumnType("uuid")
                        .HasColumnName("drafter_id");

                    b.Property<Guid>("FromDraftId")
                        .HasColumnType("uuid")
                        .HasColumnName("from_draft_id");

                    b.Property<bool>("IsUsed")
                        .HasColumnType("boolean")
                        .HasColumnName("is_used");

                    b.Property<Guid?>("ToDraftId")
                        .HasColumnType("uuid")
                        .HasColumnName("to_draft_id");

                    b.HasKey("Id")
                        .HasName("pk_rollover_vetoes");

                    b.HasIndex("DrafterId")
                        .IsUnique()
                        .HasDatabaseName("ix_rollover_vetoes_drafter_id");

                    b.ToTable("rollover_vetoes", "drafts");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities.RolloverVetoOverride", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("DrafterId")
                        .HasColumnType("uuid")
                        .HasColumnName("drafter_id");

                    b.Property<Guid>("FromDraftId")
                        .HasColumnType("uuid")
                        .HasColumnName("from_draft_id");

                    b.Property<bool>("IsUsed")
                        .HasColumnType("boolean")
                        .HasColumnName("is_used");

                    b.Property<Guid?>("ToDraftId")
                        .HasColumnType("uuid")
                        .HasColumnName("to_draft_id");

                    b.HasKey("Id")
                        .HasName("pk_rollover_veto_overrides");

                    b.HasIndex("DrafterId")
                        .IsUnique()
                        .HasDatabaseName("ix_rollover_veto_overrides_drafter_id");

                    b.ToTable("rollover_veto_overrides", "drafts");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities.Veto", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid?>("DrafterId")
                        .HasColumnType("uuid")
                        .HasColumnName("drafter_id");

                    b.Property<bool>("IsUsed")
                        .HasColumnType("boolean")
                        .HasColumnName("is_used");

                    b.HasKey("Id")
                        .HasName("pk_vetoes");

                    b.HasIndex("DrafterId")
                        .HasDatabaseName("ix_vetoes_drafter_id");

                    b.ToTable("vetoes", "drafts");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities.VetoOverride", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid?>("DrafterId")
                        .HasColumnType("uuid")
                        .HasColumnName("drafter_id");

                    b.Property<bool>("IsUsed")
                        .HasColumnType("boolean")
                        .HasColumnName("is_used");

                    b.Property<Guid>("VetoId")
                        .HasColumnType("uuid")
                        .HasColumnName("veto_id");

                    b.HasKey("Id")
                        .HasName("pk_veto_overrides");

                    b.HasIndex("DrafterId")
                        .HasDatabaseName("ix_veto_overrides_drafter_id");

                    b.HasIndex("VetoId")
                        .IsUnique()
                        .HasDatabaseName("ix_veto_overrides_veto_id");

                    b.ToTable("veto_overrides", "drafts");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafts.Draft", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at_utc");

                    b.Property<int>("DraftStatus")
                        .HasColumnType("integer")
                        .HasColumnName("draft_status");

                    b.Property<int>("DraftType")
                        .HasColumnType("integer")
                        .HasColumnName("draft_type");

                    b.Property<string>("EpisodeNumber")
                        .HasColumnType("text")
                        .HasColumnName("episode_number");

                    b.Property<bool?>("IsPatreonOnly")
                        .HasColumnType("boolean")
                        .HasColumnName("is_patreon_only");

                    b.Property<int>("ReadableId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("readable_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ReadableId"));

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("title");

                    b.Property<int>("TotalDrafters")
                        .HasColumnType("integer")
                        .HasColumnName("total_drafters");

                    b.Property<int>("TotalHosts")
                        .HasColumnType("integer")
                        .HasColumnName("total_hosts");

                    b.Property<int>("TotalPicks")
                        .HasColumnType("integer")
                        .HasColumnName("total_picks");

                    b.Property<DateTime?>("UpdatedAtUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at_utc");

                    b.HasKey("Id")
                        .HasName("pk_drafts");

                    b.HasIndex("ReadableId")
                        .IsUnique()
                        .HasDatabaseName("ix_drafts_readable_id");

                    b.ToTable("drafts", "drafts");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.DraftPosition", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid?>("DrafterId")
                        .HasColumnType("uuid")
                        .HasColumnName("drafter_id");

                    b.Property<Guid>("GameBoardId")
                        .HasColumnType("uuid")
                        .HasColumnName("game_board_id");

                    b.Property<bool>("HasBonusVeto")
                        .HasColumnType("boolean")
                        .HasColumnName("has_bonus_veto");

                    b.Property<bool>("HasBonusVetoOverride")
                        .HasColumnType("boolean")
                        .HasColumnName("has_bonus_veto_override");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("name");

                    b.Property<string>("Picks")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("picks");

                    b.HasKey("Id")
                        .HasName("pk_draft_positions");

                    b.HasIndex("DrafterId")
                        .HasDatabaseName("ix_draft_positions_drafter_id");

                    b.HasIndex("GameBoardId")
                        .HasDatabaseName("ix_draft_positions_game_board_id");

                    b.ToTable("draft_positions", "drafts");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.DraftReleaseDate", b =>
                {
                    b.Property<Guid>("DraftId")
                        .HasColumnType("uuid")
                        .HasColumnName("draft_id");

                    b.Property<DateOnly>("ReleaseDate")
                        .HasColumnType("date")
                        .HasColumnName("release_date");

                    b.HasKey("DraftId", "ReleaseDate")
                        .HasName("pk_draft_release_date");

                    b.ToTable("draft_release_date", "drafts");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.DrafterDraftStats", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("DrafterId")
                        .HasColumnType("uuid")
                        .HasColumnName("drafter_id");

                    b.Property<Guid>("DraftId")
                        .HasColumnType("uuid")
                        .HasColumnName("draft_id");

                    b.Property<int>("RolloversApplied")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(0)
                        .HasColumnName("rollovers_applied");

                    b.Property<int>("StartingVetoOverrides")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(0)
                        .HasColumnName("starting_veto_overrides");

                    b.Property<int>("StartingVetoes")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(1)
                        .HasColumnName("starting_vetoes");

                    b.Property<int>("TriviaVetoOverrides")
                        .HasColumnType("integer")
                        .HasColumnName("trivia_veto_overrides");

                    b.Property<int>("TriviaVetoes")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(0)
                        .HasColumnName("trivia_vetoes");

                    b.HasKey("Id", "DrafterId", "DraftId")
                        .HasName("pk_drafter_draft_stats");

                    b.HasIndex("DraftId")
                        .HasDatabaseName("ix_drafter_draft_stats_draft_id");

                    b.HasIndex("DrafterId")
                        .HasDatabaseName("ix_drafter_draft_stats_drafter_id");

                    b.ToTable("drafter_draft_stats", "drafts");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.GameBoard", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("DraftId")
                        .HasColumnType("uuid")
                        .HasColumnName("draft_id");

                    b.HasKey("Id")
                        .HasName("pk_game_boards");

                    b.HasIndex("DraftId")
                        .IsUnique()
                        .HasDatabaseName("ix_game_boards_draft_id");

                    b.ToTable("game_boards", "drafts");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.Host", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("HostName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("host_name");

                    b.Property<int>("ReadableId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("readable_id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ReadableId"));

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_hosts");

                    b.ToTable("hosts", "drafts");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.Movie", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("MovieTitle")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("movie_title");

                    b.HasKey("Id")
                        .HasName("pk_movies");

                    b.ToTable("movies", "drafts");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.Pick", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid?>("DraftId")
                        .HasColumnType("uuid")
                        .HasColumnName("draft_id");

                    b.Property<Guid>("DrafterId")
                        .HasColumnType("uuid")
                        .HasColumnName("drafter_id");

                    b.Property<Guid>("MovieId")
                        .HasColumnType("uuid")
                        .HasColumnName("movie_id");

                    b.Property<Guid?>("VetoId")
                        .HasColumnType("uuid")
                        .HasColumnName("veto_id");

                    b.HasKey("Id")
                        .HasName("pk_picks");

                    b.HasIndex("DraftId")
                        .HasDatabaseName("ix_picks_draft_id");

                    b.HasIndex("DrafterId")
                        .IsUnique()
                        .HasDatabaseName("ix_picks_drafter_id");

                    b.HasIndex("MovieId")
                        .IsUnique()
                        .HasDatabaseName("ix_picks_movie_id");

                    b.HasIndex("VetoId")
                        .IsUnique()
                        .HasDatabaseName("ix_picks_veto_id");

                    b.ToTable("picks", "drafts");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.TriviaResult", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("DraftId")
                        .HasColumnType("uuid")
                        .HasColumnName("draft_id");

                    b.Property<Guid>("DrafterId")
                        .HasColumnType("uuid")
                        .HasColumnName("drafter_id");

                    b.Property<int>("Position")
                        .HasColumnType("integer")
                        .HasColumnName("position");

                    b.Property<int>("QuestionsWon")
                        .HasColumnType("integer")
                        .HasColumnName("questions_won");

                    b.HasKey("Id")
                        .HasName("pk_trivia_results");

                    b.HasIndex("DraftId")
                        .HasDatabaseName("ix_trivia_results_draft_id");

                    b.HasIndex("DrafterId")
                        .HasDatabaseName("ix_trivia_results_drafter_id");

                    b.ToTable("trivia_results", "drafts");
                });

            modelBuilder.Entity("DraftDrafter", b =>
                {
                    b.HasOne("ScreenDrafts.Modules.Drafts.Domain.Drafters.Drafter", null)
                        .WithMany()
                        .HasForeignKey("DraftersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_draft_drafter_drafters_drafters_id");

                    b.HasOne("ScreenDrafts.Modules.Drafts.Domain.Drafts.Draft", null)
                        .WithMany()
                        .HasForeignKey("DraftsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_draft_drafter_drafts_drafts_id");
                });

            modelBuilder.Entity("DraftHost", b =>
                {
                    b.HasOne("ScreenDrafts.Modules.Drafts.Domain.Drafts.Draft", null)
                        .WithMany()
                        .HasForeignKey("HostedDraftsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_draft_host_drafts_hosted_drafts_id");

                    b.HasOne("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.Host", null)
                        .WithMany()
                        .HasForeignKey("HostsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_draft_host_hosts_hosts_id");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities.RolloverVeto", b =>
                {
                    b.HasOne("ScreenDrafts.Modules.Drafts.Domain.Drafters.Drafter", "Drafter")
                        .WithOne("RolloverVeto")
                        .HasForeignKey("ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities.RolloverVeto", "DrafterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_rollover_vetoes_drafters_drafter_id");

                    b.Navigation("Drafter");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities.RolloverVetoOverride", b =>
                {
                    b.HasOne("ScreenDrafts.Modules.Drafts.Domain.Drafters.Drafter", "Drafter")
                        .WithOne("RolloverVetoOverride")
                        .HasForeignKey("ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities.RolloverVetoOverride", "DrafterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_rollover_veto_overrides_drafters_drafter_id");

                    b.Navigation("Drafter");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities.Veto", b =>
                {
                    b.HasOne("ScreenDrafts.Modules.Drafts.Domain.Drafters.Drafter", null)
                        .WithMany("Vetoes")
                        .HasForeignKey("DrafterId")
                        .HasConstraintName("fk_vetoes_drafters_drafter_id");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities.VetoOverride", b =>
                {
                    b.HasOne("ScreenDrafts.Modules.Drafts.Domain.Drafters.Drafter", null)
                        .WithMany("VetoOverrides")
                        .HasForeignKey("DrafterId")
                        .HasConstraintName("fk_veto_overrides_drafters_drafter_id");

                    b.HasOne("ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities.Veto", "Veto")
                        .WithOne("VetoOverride")
                        .HasForeignKey("ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities.VetoOverride", "VetoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_veto_overrides_vetoes_veto_id");

                    b.Navigation("Veto");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.DraftPosition", b =>
                {
                    b.HasOne("ScreenDrafts.Modules.Drafts.Domain.Drafters.Drafter", "Drafter")
                        .WithMany()
                        .HasForeignKey("DrafterId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("fk_draft_positions_drafters_drafter_id");

                    b.HasOne("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.GameBoard", "GameBoard")
                        .WithMany("DraftPositions")
                        .HasForeignKey("GameBoardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_draft_positions_game_boards_game_board_id");

                    b.Navigation("Drafter");

                    b.Navigation("GameBoard");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.DraftReleaseDate", b =>
                {
                    b.HasOne("ScreenDrafts.Modules.Drafts.Domain.Drafts.Draft", "Draft")
                        .WithMany("ReleaseDates")
                        .HasForeignKey("DraftId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_draft_release_date_drafts_draft_id");

                    b.Navigation("Draft");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.DrafterDraftStats", b =>
                {
                    b.HasOne("ScreenDrafts.Modules.Drafts.Domain.Drafts.Draft", "Draft")
                        .WithMany("DrafterStats")
                        .HasForeignKey("DraftId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_drafter_draft_stats_drafts_draft_id");

                    b.HasOne("ScreenDrafts.Modules.Drafts.Domain.Drafters.Drafter", "Drafter")
                        .WithMany("DraftStats")
                        .HasForeignKey("DrafterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_drafter_draft_stats_drafters_drafter_id");

                    b.Navigation("Draft");

                    b.Navigation("Drafter");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.GameBoard", b =>
                {
                    b.HasOne("ScreenDrafts.Modules.Drafts.Domain.Drafts.Draft", "Draft")
                        .WithOne("GameBoard")
                        .HasForeignKey("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.GameBoard", "DraftId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_game_boards_drafts_draft_id");

                    b.Navigation("Draft");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.Pick", b =>
                {
                    b.HasOne("ScreenDrafts.Modules.Drafts.Domain.Drafts.Draft", null)
                        .WithMany("Picks")
                        .HasForeignKey("DraftId")
                        .HasConstraintName("fk_picks_drafts_draft_id");

                    b.HasOne("ScreenDrafts.Modules.Drafts.Domain.Drafters.Drafter", "Drafter")
                        .WithOne("Pick")
                        .HasForeignKey("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.Pick", "DrafterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_picks_drafters_drafter_id");

                    b.HasOne("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.Movie", "Movie")
                        .WithOne("Pick")
                        .HasForeignKey("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.Pick", "MovieId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_picks_movies_movie_id");

                    b.HasOne("ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities.Veto", "Veto")
                        .WithOne("Pick")
                        .HasForeignKey("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.Pick", "VetoId")
                        .HasConstraintName("fk_picks_vetoes_veto_id");

                    b.Navigation("Drafter");

                    b.Navigation("Movie");

                    b.Navigation("Veto");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.TriviaResult", b =>
                {
                    b.HasOne("ScreenDrafts.Modules.Drafts.Domain.Drafts.Draft", "Draft")
                        .WithMany("TriviaResults")
                        .HasForeignKey("DraftId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_trivia_results_drafts_draft_id");

                    b.HasOne("ScreenDrafts.Modules.Drafts.Domain.Drafters.Drafter", "Drafter")
                        .WithMany()
                        .HasForeignKey("DrafterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_trivia_results_drafters_drafter_id");

                    b.Navigation("Draft");

                    b.Navigation("Drafter");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafters.Drafter", b =>
                {
                    b.Navigation("DraftStats");

                    b.Navigation("Pick")
                        .IsRequired();

                    b.Navigation("RolloverVeto");

                    b.Navigation("RolloverVetoOverride");

                    b.Navigation("VetoOverrides");

                    b.Navigation("Vetoes");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities.Veto", b =>
                {
                    b.Navigation("Pick")
                        .IsRequired();

                    b.Navigation("VetoOverride")
                        .IsRequired();
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafts.Draft", b =>
                {
                    b.Navigation("DrafterStats");

                    b.Navigation("GameBoard");

                    b.Navigation("Picks");

                    b.Navigation("ReleaseDates");

                    b.Navigation("TriviaResults");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.GameBoard", b =>
                {
                    b.Navigation("DraftPositions");
                });

            modelBuilder.Entity("ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities.Movie", b =>
                {
                    b.Navigation("Pick");
                });
#pragma warning restore 612, 618
        }
    }
}

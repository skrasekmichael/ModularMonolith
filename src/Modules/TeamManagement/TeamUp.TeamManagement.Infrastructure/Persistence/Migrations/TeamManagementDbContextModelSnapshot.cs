﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TeamUp.TeamManagement.Infrastructure;

#nullable disable

namespace TeamUp.TeamManagement.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(TeamManagementDbContext))]
    partial class TeamManagementDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("TeamManagement")
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("TeamUp.Common.Infrastructure.Processing.Inbox.InboxMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Assembly")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Error")
                        .HasColumnType("text");

                    b.Property<DateTime?>("ProcessedUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("InboxMessages", "TeamManagement");
                });

            modelBuilder.Entity("TeamUp.Common.Infrastructure.Processing.Outbox.OutboxMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Assembly")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Error")
                        .HasColumnType("text");

                    b.Property<DateTime?>("ProcessedUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("OutboxMessages", "TeamManagement");
                });

            modelBuilder.Entity("TeamUp.TeamManagement.Domain.Aggregates.Events.Event", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("EventTypeId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("FromUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<TimeSpan>("MeetTime")
                        .HasColumnType("interval");

                    b.Property<TimeSpan>("ReplyClosingTimeBeforeMeetTime")
                        .HasColumnType("interval");

                    b.Property<uint>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<Guid>("TeamId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("ToUtc")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("EventTypeId");

                    b.HasIndex("TeamId");

                    b.ToTable("Events", "TeamManagement");
                });

            modelBuilder.Entity("TeamUp.TeamManagement.Domain.Aggregates.Events.EventResponse", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<Guid>("EventId")
                        .HasColumnType("uuid");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<int>("ReplyType")
                        .HasColumnType("integer");

                    b.Property<Guid>("TeamMemberId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("TimeStampUtc")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("EventId");

                    b.HasIndex("TeamMemberId");

                    b.HasIndex("EventId", "TeamMemberId")
                        .IsUnique();

                    b.ToTable("EventResponse", "TeamManagement");
                });

            modelBuilder.Entity("TeamUp.TeamManagement.Domain.Aggregates.Invitations.Invitation", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("RecipientId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("TeamId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("RecipientId");

                    b.HasIndex("TeamId");

                    b.HasIndex("TeamId", "RecipientId")
                        .IsUnique();

                    b.ToTable("Invitations", "TeamManagement");
                });

            modelBuilder.Entity("TeamUp.TeamManagement.Domain.Aggregates.Teams.EventType", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("TeamId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("TeamId");

                    b.ToTable("EventType", "TeamManagement");
                });

            modelBuilder.Entity("TeamUp.TeamManagement.Domain.Aggregates.Teams.Team", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<int>("NumberOfMembers")
                        .HasColumnType("integer");

                    b.Property<uint>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("Id");

                    b.ToTable("Teams", "TeamManagement");
                });

            modelBuilder.Entity("TeamUp.TeamManagement.Domain.Aggregates.Teams.TeamMember", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("Nickname")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<int>("Role")
                        .HasColumnType("integer");

                    b.Property<uint>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.Property<Guid>("TeamId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("TeamId");

                    b.HasIndex("UserId");

                    b.ToTable("TeamMember", "TeamManagement");
                });

            modelBuilder.Entity("TeamUp.TeamManagement.Domain.Aggregates.Users.User", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<int>("NumberOfOwnedTeams")
                        .HasColumnType("integer");

                    b.Property<uint>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Users", "TeamManagement");
                });

            modelBuilder.Entity("TeamUp.TeamManagement.Domain.Aggregates.Events.Event", b =>
                {
                    b.HasOne("TeamUp.TeamManagement.Domain.Aggregates.Teams.EventType", null)
                        .WithMany()
                        .HasForeignKey("EventTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TeamUp.TeamManagement.Domain.Aggregates.Teams.Team", null)
                        .WithMany()
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TeamUp.TeamManagement.Domain.Aggregates.Events.EventResponse", b =>
                {
                    b.HasOne("TeamUp.TeamManagement.Domain.Aggregates.Events.Event", null)
                        .WithMany("EventResponses")
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TeamUp.TeamManagement.Domain.Aggregates.Teams.TeamMember", null)
                        .WithMany()
                        .HasForeignKey("TeamMemberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TeamUp.TeamManagement.Domain.Aggregates.Invitations.Invitation", b =>
                {
                    b.HasOne("TeamUp.TeamManagement.Domain.Aggregates.Users.User", null)
                        .WithMany()
                        .HasForeignKey("RecipientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TeamUp.TeamManagement.Domain.Aggregates.Teams.Team", null)
                        .WithMany()
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TeamUp.TeamManagement.Domain.Aggregates.Teams.EventType", b =>
                {
                    b.HasOne("TeamUp.TeamManagement.Domain.Aggregates.Teams.Team", null)
                        .WithMany("EventTypes")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TeamUp.TeamManagement.Domain.Aggregates.Teams.TeamMember", b =>
                {
                    b.HasOne("TeamUp.TeamManagement.Domain.Aggregates.Teams.Team", "Team")
                        .WithMany("Members")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TeamUp.TeamManagement.Domain.Aggregates.Users.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Team");
                });

            modelBuilder.Entity("TeamUp.TeamManagement.Domain.Aggregates.Events.Event", b =>
                {
                    b.Navigation("EventResponses");
                });

            modelBuilder.Entity("TeamUp.TeamManagement.Domain.Aggregates.Teams.Team", b =>
                {
                    b.Navigation("EventTypes");

                    b.Navigation("Members");
                });
#pragma warning restore 612, 618
        }
    }
}

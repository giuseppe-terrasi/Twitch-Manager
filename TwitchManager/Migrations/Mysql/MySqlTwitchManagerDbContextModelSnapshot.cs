﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TwitchManager.Data.DbContexts;

#nullable disable

namespace TwitchManager.Migrations.Mysql
{
    [DbContext(typeof(MySqlTwitchManagerDbContext))]
    partial class MySqlTwitchManagerDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("TwitchManager.Data.Domains.Clip", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("BroadcasterId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("BroadcasterName")
                        .HasColumnType("longtext");

                    b.Property<string>("ClipId")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("CreatorId")
                        .HasColumnType("longtext");

                    b.Property<string>("CreatorName")
                        .HasColumnType("longtext");

                    b.Property<double>("Duration")
                        .HasColumnType("double");

                    b.Property<string>("EmbedUrl")
                        .HasColumnType("longtext");

                    b.Property<string>("GameId")
                        .HasColumnType("varchar(255)");

                    b.Property<bool>("IsFeatured")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Language")
                        .HasColumnType("longtext");

                    b.Property<string>("ThumbnailUrl")
                        .HasColumnType("longtext");

                    b.Property<string>("Title")
                        .HasColumnType("longtext");

                    b.Property<string>("Url")
                        .HasColumnType("longtext");

                    b.Property<string>("VideoId")
                        .HasColumnType("longtext");

                    b.Property<int>("ViewCount")
                        .HasColumnType("int");

                    b.Property<int?>("VodOffset")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BroadcasterId");

                    b.HasIndex("GameId");

                    b.ToTable("Clips", (string)null);
                });

            modelBuilder.Entity("TwitchManager.Data.Domains.Game", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("BoxArtUrl")
                        .HasColumnType("longtext");

                    b.Property<string>("IgdbId")
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Games", (string)null);
                });

            modelBuilder.Entity("TwitchManager.Data.Domains.Streamer", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("BroadcasterType")
                        .HasColumnType("longtext");

                    b.Property<string>("CreatedAt")
                        .HasColumnType("longtext");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<string>("DisplayName")
                        .HasColumnType("longtext");

                    b.Property<string>("Login")
                        .HasColumnType("longtext");

                    b.Property<string>("OfflineImageUrl")
                        .HasColumnType("longtext");

                    b.Property<string>("ProfileImageUrl")
                        .HasColumnType("longtext");

                    b.Property<string>("Type")
                        .HasColumnType("longtext");

                    b.Property<int>("ViewCount")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Streamers", (string)null);
                });

            modelBuilder.Entity("TwitchManager.Data.Domains.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("TwitchId")
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("TwitchId");

                    b.ToTable("Users", (string)null);
                });

            modelBuilder.Entity("TwitchManager.Data.Domains.Clip", b =>
                {
                    b.HasOne("TwitchManager.Data.Domains.Streamer", "Streamer")
                        .WithMany("Clips")
                        .HasForeignKey("BroadcasterId");

                    b.HasOne("TwitchManager.Data.Domains.Game", "Game")
                        .WithMany("Clips")
                        .HasForeignKey("GameId");

                    b.Navigation("Game");

                    b.Navigation("Streamer");
                });

            modelBuilder.Entity("TwitchManager.Data.Domains.Game", b =>
                {
                    b.Navigation("Clips");
                });

            modelBuilder.Entity("TwitchManager.Data.Domains.Streamer", b =>
                {
                    b.Navigation("Clips");
                });
#pragma warning restore 612, 618
        }
    }
}

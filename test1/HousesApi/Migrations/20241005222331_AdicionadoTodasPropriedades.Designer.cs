﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace HousesApi.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20241005222331_AdicionadoTodasPropriedades")]
    partial class AdicionadoTodasPropriedades
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.8");

            modelBuilder.Entity("House", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AncestralWeapons")
                        .HasColumnType("TEXT");

                    b.Property<string>("CadetBranches")
                        .HasColumnType("TEXT");

                    b.Property<string>("CoatOfArms")
                        .HasColumnType("TEXT");

                    b.Property<string>("CurrentLord")
                        .HasColumnType("TEXT");

                    b.Property<string>("DiedOut")
                        .HasColumnType("TEXT");

                    b.Property<string>("Founded")
                        .HasColumnType("TEXT");

                    b.Property<string>("Founder")
                        .HasColumnType("TEXT");

                    b.Property<string>("Heir")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("OverLord")
                        .HasColumnType("TEXT");

                    b.Property<string>("Region")
                        .HasColumnType("TEXT");

                    b.Property<string>("Seats")
                        .HasColumnType("TEXT");

                    b.Property<string>("SwornMembers")
                        .HasColumnType("TEXT");

                    b.Property<string>("Titles")
                        .HasColumnType("TEXT");

                    b.Property<string>("Words")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Houses");
                });
#pragma warning restore 612, 618
        }
    }
}
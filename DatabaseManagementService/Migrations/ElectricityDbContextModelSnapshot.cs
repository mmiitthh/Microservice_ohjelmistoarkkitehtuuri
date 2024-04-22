﻿// <auto-generated />
using System;
using DatabaseManagementService.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DatabaseManagementService.Migrations
{
    [DbContext(typeof(ElectricityDbContext))]
    partial class ElectricityDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("DatabaseManagementService.Models.ElectricityPrice", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("datetime2")
                        .HasAnnotation("Relational:JsonPropertyName", "endDate");

                    b.Property<double>("Price")
                        .HasColumnType("float")
                        .HasAnnotation("Relational:JsonPropertyName", "price");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2")
                        .HasAnnotation("Relational:JsonPropertyName", "startDate");

                    b.HasKey("Id");

                    b.ToTable("ElectricityPrices");
                });
#pragma warning restore 612, 618
        }
    }
}

﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using OwlApi;

namespace Warehouses.Migrations
{
    [DbContext(typeof(OwlApiContext))]
    [Migration("20200531080246_OptimapiPlanName")]
    partial class OptimapiPlanName
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Warehouses.Models.Availability", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<TimeSpan>("MinimumNotice")
                        .HasColumnType("interval");

                    b.HasKey("Id");

                    b.ToTable("availabilities");
                });

            modelBuilder.Entity("Warehouses.Models.Door", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("AvailabilityId")
                        .HasColumnType("integer");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Properties")
                        .HasColumnType("jsonb");

                    b.Property<int>("WarehouseId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("AvailabilityId");

                    b.HasIndex("WarehouseId");

                    b.ToTable("doors");
                });

            modelBuilder.Entity("Warehouses.Models.File", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Path")
                        .HasColumnType("text");

                    b.Property<int?>("ReservationId")
                        .HasColumnType("integer");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ReservationId");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("files");
                });

            modelBuilder.Entity("Warehouses.Models.OptimapiPlan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("Finished")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("optimapi_plans");
                });

            modelBuilder.Entity("Warehouses.Models.OptimapiServer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Password")
                        .HasColumnType("text");

                    b.Property<short>("Status")
                        .HasColumnType("smallint");

                    b.Property<string>("StopsSettings")
                        .HasColumnType("text");

                    b.Property<string>("Url")
                        .HasColumnType("text");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.Property<string>("Username")
                        .HasColumnType("text");

                    b.Property<string>("VehiclesSettings")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("optimapi_servers");
                });

            modelBuilder.Entity("Warehouses.Models.OptimapiSolution", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("Final")
                        .HasColumnType("boolean");

                    b.Property<int>("Iteration")
                        .HasColumnType("integer");

                    b.Property<int>("PlanId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("PlanId");

                    b.ToTable("optimapi_solutions");
                });

            modelBuilder.Entity("Warehouses.Models.OptimapiSolutionFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<byte[]>("Data")
                        .HasColumnType("bytea");

                    b.Property<int>("SolutionId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("SolutionId")
                        .IsUnique();

                    b.ToTable("optimapi_solution_files");
                });

            modelBuilder.Entity("Warehouses.Models.Permission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("CarrierId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<short>("Status")
                        .HasColumnType("smallint");

                    b.Property<int>("WarehouseId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CarrierId");

                    b.HasIndex("WarehouseId");

                    b.ToTable("permissions");
                });

            modelBuilder.Entity("Warehouses.Models.Reservation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("CarrierId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Data")
                        .HasColumnType("jsonb");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("DoorId")
                        .HasColumnType("integer");

                    b.Property<TimeSpan>("End")
                        .HasColumnType("interval");

                    b.Property<TimeSpan>("Start")
                        .HasColumnType("interval");

                    b.HasKey("Id");

                    b.HasIndex("CarrierId");

                    b.HasIndex("DoorId");

                    b.ToTable("reservations");
                });

            modelBuilder.Entity("Warehouses.Models.TimeWindow", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("AvailabilityId")
                        .HasColumnType("integer");

                    b.Property<TimeSpan>("End")
                        .HasColumnType("interval");

                    b.Property<TimeSpan>("Start")
                        .HasColumnType("interval");

                    b.HasKey("Id");

                    b.HasIndex("AvailabilityId");

                    b.ToTable("time_windows");
                });

            modelBuilder.Entity("Warehouses.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime?>("Confirmed")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<DateTime?>("EmailConfirmed")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PasswordReset")
                        .HasColumnType("text");

                    b.Property<DateTime?>("PasswordResetTimeout")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<DateTime>("RegisteredAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<short>("Type")
                        .HasColumnType("smallint");

                    b.HasKey("Id");

                    b.ToTable("users");
                });

            modelBuilder.Entity("Warehouses.Models.Warehouse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("AvailabilityId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<int>("OwnerId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("AvailabilityId");

                    b.HasIndex("OwnerId");

                    b.ToTable("warehouses");
                });

            modelBuilder.Entity("Warehouses.Models.Door", b =>
                {
                    b.HasOne("Warehouses.Models.Availability", "Availability")
                        .WithMany()
                        .HasForeignKey("AvailabilityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Warehouses.Models.Warehouse", "Warehouse")
                        .WithMany("Doors")
                        .HasForeignKey("WarehouseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Warehouses.Models.File", b =>
                {
                    b.HasOne("Warehouses.Models.Reservation", "Reservation")
                        .WithMany("Files")
                        .HasForeignKey("ReservationId");

                    b.HasOne("Warehouses.Models.User", "User")
                        .WithOne("Image")
                        .HasForeignKey("Warehouses.Models.File", "UserId");
                });

            modelBuilder.Entity("Warehouses.Models.OptimapiPlan", b =>
                {
                    b.HasOne("Warehouses.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Warehouses.Models.OptimapiServer", b =>
                {
                    b.HasOne("Warehouses.Models.User", "User")
                        .WithOne("Server")
                        .HasForeignKey("Warehouses.Models.OptimapiServer", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Warehouses.Models.OptimapiSolution", b =>
                {
                    b.HasOne("Warehouses.Models.OptimapiPlan", "Plan")
                        .WithMany("Solutions")
                        .HasForeignKey("PlanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Warehouses.Models.OptimapiSolutionFile", b =>
                {
                    b.HasOne("Warehouses.Models.OptimapiSolution", "Solution")
                        .WithOne("File")
                        .HasForeignKey("Warehouses.Models.OptimapiSolutionFile", "SolutionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Warehouses.Models.Permission", b =>
                {
                    b.HasOne("Warehouses.Models.User", "Carrier")
                        .WithMany("Permisisons")
                        .HasForeignKey("CarrierId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Warehouses.Models.Warehouse", "Warehouse")
                        .WithMany("Permissions")
                        .HasForeignKey("WarehouseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Warehouses.Models.Reservation", b =>
                {
                    b.HasOne("Warehouses.Models.User", "Carrier")
                        .WithMany("Reservations")
                        .HasForeignKey("CarrierId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Warehouses.Models.Door", "Door")
                        .WithMany("Reservations")
                        .HasForeignKey("DoorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Warehouses.Models.TimeWindow", b =>
                {
                    b.HasOne("Warehouses.Models.Availability", "Availability")
                        .WithMany("TimeWindows")
                        .HasForeignKey("AvailabilityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Warehouses.Models.Warehouse", b =>
                {
                    b.HasOne("Warehouses.Models.Availability", "Availability")
                        .WithMany()
                        .HasForeignKey("AvailabilityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Warehouses.Models.User", "Owner")
                        .WithMany("Warehouses")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}

﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using OwlApi;
using OwlApi.Models;

#nullable disable

namespace Warehouses.Migrations
{
    [DbContext(typeof(OwlApiContext))]
    [Migration("20220612122708_AnonymousReservations1")]
    partial class AnonymousReservations1
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Warehouses.Models.AppLanguage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("name")
                        .HasColumnType("text");

                    b.Property<string>("subdomain")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("app_languages", (string)null);
                });

            modelBuilder.Entity("Warehouses.Models.Availability", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("GranularityMinutes")
                        .HasColumnType("integer");

                    b.Property<TimeSpan>("MinimumNotice")
                        .HasColumnType("interval");

                    b.HasKey("Id");

                    b.ToTable("availabilities", (string)null);
                });

            modelBuilder.Entity("Warehouses.Models.Company", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("FirstSyncedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("LastSyncedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("MailSendingData")
                        .HasColumnType("jsonb");

                    b.Property<string>("MailSendingTexts")
                        .HasColumnType("jsonb");

                    b.Property<string>("MailSubjectOrder")
                        .HasColumnType("jsonb");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("RealmName")
                        .HasColumnType("text");

                    b.Property<DateTime>("SyncDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("companies", (string)null);
                });

            modelBuilder.Entity("Warehouses.Models.ContactMail", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("CompanyId")
                        .HasColumnType("integer");

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CompanyId");

                    b.HasIndex("UserId");

                    b.ToTable("contact_mails", (string)null);
                });

            modelBuilder.Entity("Warehouses.Models.Door", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

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

                    b.ToTable("doors", (string)null);
                });

            modelBuilder.Entity("Warehouses.Models.File", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Path")
                        .HasColumnType("text");

                    b.Property<string>("PdfToken")
                        .HasColumnType("text");

                    b.Property<int?>("RecurringReservationId")
                        .HasColumnType("integer");

                    b.Property<int?>("ReservationId")
                        .HasColumnType("integer");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.Property<int?>("WarehouseId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("RecurringReservationId");

                    b.HasIndex("ReservationId");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.HasIndex("WarehouseId")
                        .IsUnique();

                    b.ToTable("files", (string)null);
                });

            modelBuilder.Entity("Warehouses.Models.OptimapiPlan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("Finished")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("optimapi_plans", (string)null);
                });

            modelBuilder.Entity("Warehouses.Models.OptimapiServer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

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

                    b.HasIndex("UserId");

                    b.ToTable("optimapi_servers", (string)null);
                });

            modelBuilder.Entity("Warehouses.Models.OptimapiSolution", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("Final")
                        .HasColumnType("boolean");

                    b.Property<int>("Iteration")
                        .HasColumnType("integer");

                    b.Property<int>("PlanId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("PlanId");

                    b.ToTable("optimapi_solutions", (string)null);
                });

            modelBuilder.Entity("Warehouses.Models.OptimapiSolutionFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<byte[]>("Data")
                        .HasColumnType("bytea");

                    b.Property<int>("SolutionId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("SolutionId")
                        .IsUnique();

                    b.ToTable("optimapi_solution_files", (string)null);
                });

            modelBuilder.Entity("Warehouses.Models.Permission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("CarrierId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<short>("Status")
                        .HasColumnType("smallint");

                    b.Property<int>("WarehouseId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CarrierId");

                    b.HasIndex("WarehouseId");

                    b.ToTable("permissions", (string)null);
                });

            modelBuilder.Entity("Warehouses.Models.RecurringReservation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("CarrierId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Data")
                        .HasColumnType("jsonb");

                    b.Property<int?>("DoorId")
                        .HasColumnType("integer");

                    b.Property<TimeSpan>("End")
                        .HasColumnType("interval");

                    b.Property<DateTime?>("FromDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("RecurrenceRule")
                        .HasColumnType("text");

                    b.Property<TimeSpan>("Start")
                        .HasColumnType("interval");

                    b.Property<DateTime?>("ToDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("WarehouseId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CarrierId");

                    b.HasIndex("DoorId");

                    b.HasIndex("WarehouseId");

                    b.ToTable("recurring_reservations", (string)null);
                });

            modelBuilder.Entity("Warehouses.Models.Reservation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("CarrierId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Data")
                        .HasColumnType("jsonb");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("DoorId")
                        .HasColumnType("integer");

                    b.Property<TimeSpan?>("End")
                        .HasColumnType("interval");

                    b.Property<TimeSpan?>("Start")
                        .HasColumnType("interval");

                    b.Property<int?>("WarehouseId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CarrierId");

                    b.HasIndex("DoorId");

                    b.HasIndex("WarehouseId");

                    b.ToTable("reservations", (string)null);
                });

            modelBuilder.Entity("Warehouses.Models.ReservationField", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Default")
                        .HasColumnType("text");

                    b.Property<int?>("DoorId")
                        .HasColumnType("integer");

                    b.Property<bool>("HideField")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsMultiLine")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsPalletsField")
                        .HasColumnType("boolean");

                    b.Property<int?>("Max")
                        .HasColumnType("integer");

                    b.Property<int?>("Min")
                        .HasColumnType("integer");

                    b.Property<bool>("Required")
                        .HasColumnType("boolean");

                    b.Property<SelectValuesData>("SelectValues")
                        .HasColumnType("jsonb");

                    b.Property<int>("SequenceNumber")
                        .HasColumnType("integer");

                    b.Property<bool>("ShowInMail")
                        .HasColumnType("boolean");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.Property<int?>("WarehouseId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("DoorId");

                    b.HasIndex("UserId");

                    b.HasIndex("WarehouseId");

                    b.ToTable("reservation_fields", (string)null);
                });

            modelBuilder.Entity("Warehouses.Models.ReservationFieldName", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("fieldId")
                        .HasColumnType("integer");

                    b.Property<int>("languageId")
                        .HasColumnType("integer");

                    b.Property<string>("name")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("fieldId");

                    b.HasIndex("languageId");

                    b.ToTable("reservation_field_names", (string)null);
                });

            modelBuilder.Entity("Warehouses.Models.ReservationStatusUpdate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("ReservationId")
                        .HasColumnType("integer");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.Property<int>("status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ReservationId");

                    b.HasIndex("UserId");

                    b.ToTable("reservation_status_update", (string)null);
                });

            modelBuilder.Entity("Warehouses.Models.TimeWindow", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AvailabilityId")
                        .HasColumnType("integer");

                    b.Property<TimeSpan>("End")
                        .HasColumnType("interval");

                    b.Property<TimeSpan>("Start")
                        .HasColumnType("interval");

                    b.HasKey("Id");

                    b.HasIndex("AvailabilityId");

                    b.ToTable("time_windows", (string)null);
                });

            modelBuilder.Entity("Warehouses.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<int?>("CompanyId")
                        .HasColumnType("integer");

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<DateTime>("FirstSyncedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("KeycloakId")
                        .HasColumnType("text");

                    b.Property<DateTime>("LastSyncedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("MailSendingData")
                        .HasColumnType("jsonb");

                    b.Property<string>("MailSendingTexts")
                        .HasColumnType("jsonb");

                    b.Property<string>("MailSubjectOrder")
                        .HasColumnType("jsonb");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<List<string>>("Roles")
                        .HasColumnType("jsonb");

                    b.Property<DateTime>("SyncDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.Property<int>("dataTableFieldNamesDisplayMode")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CompanyId");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("Warehouses.Models.Warehouse", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<int>("AvailabilityId")
                        .HasColumnType("integer");

                    b.Property<int>("CompanyId")
                        .HasColumnType("integer");

                    b.Property<string>("ContactEmail")
                        .HasColumnType("text");

                    b.Property<string>("ContactPhone")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("CreatedById")
                        .HasColumnType("integer");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<bool>("canCarrierCreateAnonymousReservation")
                        .HasColumnType("boolean");

                    b.Property<bool>("canCarrierDeleteReservation")
                        .HasColumnType("boolean");

                    b.Property<bool>("canCarrierEditReservation")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("AvailabilityId");

                    b.HasIndex("CompanyId");

                    b.HasIndex("CreatedById");

                    b.ToTable("warehouses", (string)null);
                });

            modelBuilder.Entity("Warehouses.Models.ContactMail", b =>
                {
                    b.HasOne("Warehouses.Models.Company", "Company")
                        .WithMany()
                        .HasForeignKey("CompanyId");

                    b.HasOne("Warehouses.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("Company");

                    b.Navigation("User");
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

                    b.Navigation("Availability");

                    b.Navigation("Warehouse");
                });

            modelBuilder.Entity("Warehouses.Models.File", b =>
                {
                    b.HasOne("Warehouses.Models.RecurringReservation", "RecurringReservation")
                        .WithMany("Files")
                        .HasForeignKey("RecurringReservationId");

                    b.HasOne("Warehouses.Models.Reservation", "Reservation")
                        .WithMany("Files")
                        .HasForeignKey("ReservationId");

                    b.HasOne("Warehouses.Models.User", "User")
                        .WithOne("Image")
                        .HasForeignKey("Warehouses.Models.File", "UserId");

                    b.HasOne("Warehouses.Models.Warehouse", "Warehouse")
                        .WithOne("Image")
                        .HasForeignKey("Warehouses.Models.File", "WarehouseId");

                    b.Navigation("RecurringReservation");

                    b.Navigation("Reservation");

                    b.Navigation("User");

                    b.Navigation("Warehouse");
                });

            modelBuilder.Entity("Warehouses.Models.OptimapiPlan", b =>
                {
                    b.HasOne("Warehouses.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Warehouses.Models.OptimapiServer", b =>
                {
                    b.HasOne("Warehouses.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Warehouses.Models.OptimapiSolution", b =>
                {
                    b.HasOne("Warehouses.Models.OptimapiPlan", "Plan")
                        .WithMany("Solutions")
                        .HasForeignKey("PlanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Plan");
                });

            modelBuilder.Entity("Warehouses.Models.OptimapiSolutionFile", b =>
                {
                    b.HasOne("Warehouses.Models.OptimapiSolution", "Solution")
                        .WithOne("File")
                        .HasForeignKey("Warehouses.Models.OptimapiSolutionFile", "SolutionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Solution");
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

                    b.Navigation("Carrier");

                    b.Navigation("Warehouse");
                });

            modelBuilder.Entity("Warehouses.Models.RecurringReservation", b =>
                {
                    b.HasOne("Warehouses.Models.User", "Carrier")
                        .WithMany()
                        .HasForeignKey("CarrierId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Warehouses.Models.Door", "Door")
                        .WithMany()
                        .HasForeignKey("DoorId");

                    b.HasOne("Warehouses.Models.Warehouse", "Warehouse")
                        .WithMany()
                        .HasForeignKey("WarehouseId");

                    b.Navigation("Carrier");

                    b.Navigation("Door");

                    b.Navigation("Warehouse");
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
                        .HasForeignKey("DoorId");

                    b.HasOne("Warehouses.Models.Warehouse", "Warehouse")
                        .WithMany()
                        .HasForeignKey("WarehouseId");

                    b.Navigation("Carrier");

                    b.Navigation("Door");

                    b.Navigation("Warehouse");
                });

            modelBuilder.Entity("Warehouses.Models.ReservationField", b =>
                {
                    b.HasOne("Warehouses.Models.Door", "Door")
                        .WithMany("ReservationFields")
                        .HasForeignKey("DoorId");

                    b.HasOne("Warehouses.Models.User", "User")
                        .WithMany("ReservationFields")
                        .HasForeignKey("UserId");

                    b.HasOne("Warehouses.Models.Warehouse", "Warehouse")
                        .WithMany("ReservationFields")
                        .HasForeignKey("WarehouseId");

                    b.Navigation("Door");

                    b.Navigation("User");

                    b.Navigation("Warehouse");
                });

            modelBuilder.Entity("Warehouses.Models.ReservationFieldName", b =>
                {
                    b.HasOne("Warehouses.Models.ReservationField", "reservationField")
                        .WithMany("reservationFieldNames")
                        .HasForeignKey("fieldId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Warehouses.Models.AppLanguage", "language")
                        .WithMany()
                        .HasForeignKey("languageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("language");

                    b.Navigation("reservationField");
                });

            modelBuilder.Entity("Warehouses.Models.ReservationStatusUpdate", b =>
                {
                    b.HasOne("Warehouses.Models.Reservation", "Reservation")
                        .WithMany("ReservationStatusUpdates")
                        .HasForeignKey("ReservationId");

                    b.HasOne("Warehouses.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("Reservation");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Warehouses.Models.TimeWindow", b =>
                {
                    b.HasOne("Warehouses.Models.Availability", "Availability")
                        .WithMany("TimeWindows")
                        .HasForeignKey("AvailabilityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Availability");
                });

            modelBuilder.Entity("Warehouses.Models.User", b =>
                {
                    b.HasOne("Warehouses.Models.Company", "Company")
                        .WithMany()
                        .HasForeignKey("CompanyId");

                    b.Navigation("Company");
                });

            modelBuilder.Entity("Warehouses.Models.Warehouse", b =>
                {
                    b.HasOne("Warehouses.Models.Availability", "Availability")
                        .WithMany()
                        .HasForeignKey("AvailabilityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Warehouses.Models.Company", "Company")
                        .WithMany("Warehouses")
                        .HasForeignKey("CompanyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Warehouses.Models.User", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById");

                    b.Navigation("Availability");

                    b.Navigation("Company");

                    b.Navigation("CreatedBy");
                });

            modelBuilder.Entity("Warehouses.Models.Availability", b =>
                {
                    b.Navigation("TimeWindows");
                });

            modelBuilder.Entity("Warehouses.Models.Company", b =>
                {
                    b.Navigation("Warehouses");
                });

            modelBuilder.Entity("Warehouses.Models.Door", b =>
                {
                    b.Navigation("ReservationFields");

                    b.Navigation("Reservations");
                });

            modelBuilder.Entity("Warehouses.Models.OptimapiPlan", b =>
                {
                    b.Navigation("Solutions");
                });

            modelBuilder.Entity("Warehouses.Models.OptimapiSolution", b =>
                {
                    b.Navigation("File");
                });

            modelBuilder.Entity("Warehouses.Models.RecurringReservation", b =>
                {
                    b.Navigation("Files");
                });

            modelBuilder.Entity("Warehouses.Models.Reservation", b =>
                {
                    b.Navigation("Files");

                    b.Navigation("ReservationStatusUpdates");
                });

            modelBuilder.Entity("Warehouses.Models.ReservationField", b =>
                {
                    b.Navigation("reservationFieldNames");
                });

            modelBuilder.Entity("Warehouses.Models.User", b =>
                {
                    b.Navigation("Image");

                    b.Navigation("Permisisons");

                    b.Navigation("ReservationFields");

                    b.Navigation("Reservations");
                });

            modelBuilder.Entity("Warehouses.Models.Warehouse", b =>
                {
                    b.Navigation("Doors");

                    b.Navigation("Image");

                    b.Navigation("Permissions");

                    b.Navigation("ReservationFields");
                });
#pragma warning restore 612, 618
        }
    }
}

using Microsoft.EntityFrameworkCore;
using OwlApi.Models;

namespace OwlApi
{
    public class OwlApiContext : DbContext
    {
        public OwlApiContext(DbContextOptions<OwlApiContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Door> Doors { get; set; }
        public DbSet<DoorFieldsFilter> DoorFieldsFilters { get; set; }
        public DbSet<TimeWindowFieldsFilter> TimeWindowFieldsFilters { get; set; }
        public DbSet<Availability> Availabilities { get; set; }
        public DbSet<TimeWindow> TimeWindows { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<PermissionForDoor> PermissionsForDoor { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<OptimapiServer> OptimapiServers { get; set; }
        public DbSet<OptimapiPlan> OptimapiPlans { get; set; }
        public DbSet<OptimapiSolution> OptimapiSolutions { get; set; }
        public DbSet<OptimapiSolutionFile> OptimapiSolutionFiles { get; set; }
        public DbSet<RecurringReservation> RecurringReservations { get; set; }
        public DbSet<ReservationStatusUpdate> ReservationStatusUpdates { get; set; }
        public DbSet<ContactMail> ContactMails { get; set; }
        public DbSet<ReservationField> ReservationFields { get; set; }
        public DbSet<ReservationFieldName> ReservationFieldNames { get; set; }
        public DbSet<AppLanguage> AppLanguages { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<Holiday> Holidays { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Company>().ToTable("companies");
            modelBuilder.Entity<Permission>().ToTable("permissions");
            modelBuilder.Entity<PermissionForDoor>().ToTable("permissions_for_door");
            modelBuilder.Entity<Door>().ToTable("doors");
            modelBuilder.Entity<DoorFieldsFilter>().ToTable("door_fields_filter");
            modelBuilder.Entity<Warehouse>().ToTable("warehouses");
            modelBuilder.Entity<Availability>().ToTable("availabilities");
            modelBuilder.Entity<TimeWindow>().ToTable("time_windows");
            modelBuilder.Entity<Reservation>().ToTable("reservations");
            modelBuilder.Entity<File>().ToTable("files");
            modelBuilder.Entity<OptimapiServer>().ToTable("optimapi_servers");
            modelBuilder.Entity<OptimapiPlan>().ToTable("optimapi_plans");
            modelBuilder.Entity<OptimapiSolution>().ToTable("optimapi_solutions");
            modelBuilder.Entity<OptimapiSolutionFile>().ToTable("optimapi_solution_files");
            modelBuilder.Entity<RecurringReservation>().ToTable("recurring_reservations");
            modelBuilder.Entity<ReservationStatusUpdate>().ToTable("reservation_status_update");
            modelBuilder.Entity<ContactMail>().ToTable("contact_mails");
            modelBuilder.Entity<ReservationField>().ToTable("reservation_fields");
            modelBuilder.Entity<ReservationFieldName>().ToTable("reservation_field_names");
            modelBuilder.Entity<AppLanguage>().ToTable("app_languages");
            modelBuilder.Entity<Country>().ToTable("countries");
            modelBuilder.Entity<EmailTemplate>().ToTable("email_templates");
            modelBuilder.Entity<Holiday>().ToTable("holidays");
        }
    }
}

using GroupProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GroupProject
{
    public partial class DB_Context : DbContext
    {
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Instructor> Instructors { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<ClassPackage> ClassPackages { get; set; }
        public virtual DbSet<ClassType> ClassTypes { get; set; }
        public virtual DbSet<RoomLayout> RoomLayouts { get; set; }
        public virtual DbSet<ClassSchedule> ClassSchedules { get; set; }
        public virtual DbSet<Class> Classes { get; set; }
        public virtual DbSet<Reservation> Reservations { get; set; }

        public static readonly LoggerFactory MyLoggerFactory = new LoggerFactory(new[] { new ConsoleLoggerProvider((_, __) => true, true) });
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseLoggerFactory(MyLoggerFactory);
                optionsBuilder.UseMySql("Server=localhost;Port=3306;Uid=root;Pwd=;Database=bookings");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                modelBuilder.Entity<User>().ToTable("users")
                .Property(u => u.UserID)
                .ValueGeneratedOnAdd();
                modelBuilder.Entity<User>().ToTable("users")
                .Property(e => e._Gender).HasColumnName("Gender");
                modelBuilder.Entity<User>().ToTable("users")
                .Property(e => e._UserRole).HasColumnName("UserRole");
                modelBuilder.Entity<User>().ToTable("users")
                .Property(e => e.DateOfBirth).HasColumnType("Date");

                modelBuilder.Entity<Instructor>().ToTable("classes")
               .HasOne(c => c.Location)
               .WithMany(c => c.Instructors)
               .HasForeignKey(c => c.LocationID).IsRequired(false);

                modelBuilder.Entity<User>().ToTable("users")
                .HasDiscriminator<string>("Discriminator")
                .HasValue<User>("NormalUser")
                .HasValue<Instructor>("SystemUser");

            });

            modelBuilder.Entity<Location>(entity =>
            {
                modelBuilder.Entity<Location>().ToTable("locations")
                .Property(l => l.LocationID)
                .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<ClassPackage>(entity =>
            {
                modelBuilder.Entity<ClassPackage>().ToTable("class_packages")
                .Property(c => c.ClassPackageID)
                .ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<ClassType>(entity =>
            {
                modelBuilder.Entity<ClassType>().ToTable("class_types")
                .Property(c => c.ClassTypeID)
                .ValueGeneratedOnAdd();

                modelBuilder.Entity<ClassType>().ToTable("class_types")
                .HasOne(p => p.Location)
                .WithMany(b => b.ClassTypes)
                .HasForeignKey(p => p.LocationID)
                .HasConstraintName("FK_ClassType_Location_LocationID");

                modelBuilder.Entity<ClassType>().ToTable("class_types")
                .HasOne(p => p.Instructor)
                .WithMany(b => b.ClassTypes)
                .HasForeignKey(p => p.InstructorID)
                .HasConstraintName("FK_ClassType_User_UserID");

            });

            modelBuilder.Entity<RoomLayout>(entity =>
            {
                modelBuilder.Entity<RoomLayout>().ToTable("room_layouts")
                .Property(c => c.RoomLayoutID)
                .ValueGeneratedOnAdd();

                modelBuilder.Entity<RoomLayout>().ToTable("room_layouts")
                .HasOne(p => p.Location)
                .WithMany(b => b.RoomLayouts)
                .HasForeignKey(p => p.LocationID)
                .HasConstraintName("FK_RoomLayout_Location_LocationID");
            });

            modelBuilder.Entity<ClassSchedule>(entity =>
            {
                modelBuilder.Entity<ClassSchedule>().ToTable("class_schedules")
                .Property(c => c.ClassScheduleID)
                .ValueGeneratedOnAdd();

                modelBuilder.Entity<ClassSchedule>().ToTable("class_schedules")
                .Property(e => e.StartDate).HasColumnType("Date");

                modelBuilder.Entity<ClassSchedule>().ToTable("class_schedules")
                .Property(e => e.EndDate).HasColumnType("Date");

                modelBuilder.Entity<ClassSchedule>().ToTable("class_schedules")
                .Property(e => e.StartTime).HasColumnType("Time");

                modelBuilder.Entity<ClassSchedule>().ToTable("class_schedules")
                .Property(e => e.EndTime).HasColumnType("Time");

                modelBuilder.Entity<ClassSchedule>().ToTable("class_schedules")
                .HasOne(c => c.ClassType)
                .WithMany(c => c.ClassSchedules)
                .HasForeignKey(c => c.ClassTypeID)
                .HasConstraintName("FK_ClassSchedule_ClassType_ClassTypeID");

                modelBuilder.Entity<ClassSchedule>().ToTable("class_schedules")
                .HasOne(c => c.RoomLayout)
                .WithMany(c => c.ClassSchedules)
                .HasForeignKey(c => c.RoomLayoutID).IsRequired(false);
                //.HasConstraintName("FK_ClassSchedule_RoomLayout_RoomLayoutID");

                modelBuilder.Entity<ClassSchedule>().ToTable("class_schedules")
                .Ignore(p => p.SelectedDays);
            });

            modelBuilder.Entity<Class>(entity =>
            {
                modelBuilder.Entity<Class>().ToTable("classes")
               .Property(c => c.ClassID)
               .ValueGeneratedOnAdd();

                modelBuilder.Entity<Class>().ToTable("classes")
                .Ignore(p => p.DisabledForView);

                modelBuilder.Entity<Class>().ToTable("classes")
                .Property(e => e.StartDate).HasColumnType("Date");

                modelBuilder.Entity<Class>().ToTable("classes")
                .Property(e => e.EndDate).HasColumnType("Date");

                modelBuilder.Entity<Class>().ToTable("classes")
                .Property(e => e.StartTime).HasColumnType("Time");

                modelBuilder.Entity<Class>().ToTable("classes")
                .Property(e => e.EndTime).HasColumnType("Time");

                modelBuilder.Entity<Class>().ToTable("classes")
                .HasOne(c => c.ClassType)
                .WithMany(c => c.Classes)
                .HasForeignKey(c => c.ClassTypeID);

                modelBuilder.Entity<Class>().ToTable("classes")
                .HasOne(c => c.ClassSchedule)
                .WithMany(c => c.Classes)
                .HasForeignKey(c => c.ClassScheduleID);

                modelBuilder.Entity<Class>().ToTable("classes")
                .HasOne(c => c.SubstituteInstructor)
                .WithMany(c => c.Classes)
                .HasForeignKey(c => c.InstructorID).IsRequired(false);
            });

            modelBuilder.Entity<Reservation>(entity =>
            {
                modelBuilder.Entity<Reservation>().ToTable("reservations")
                .Property(c => c.ReservationID)
                .ValueGeneratedOnAdd();

                modelBuilder.Entity<Reservation>().ToTable("reservations")
               .Property(e => e.ReservationDate).HasColumnType("DateTime");

                modelBuilder.Entity<Reservation>().ToTable("reservations")
                .HasOne(c => c.Class)
                .WithMany(c => c.Reservations)
                .HasForeignKey(c => c.ClassID);

                modelBuilder.Entity<Reservation>().ToTable("reservations")
                .HasOne(c => c.User)
                .WithMany(c => c.Reservations)
                .HasForeignKey(c => c.UserID).IsRequired(false);

                modelBuilder.Entity<Reservation>().ToTable("reservations")
                .HasOne(c => c.RoomLayout)
                .WithMany(c => c.Reservations)
                .HasForeignKey(c => c.RoomLayoutID);
            });
        }
    }
}

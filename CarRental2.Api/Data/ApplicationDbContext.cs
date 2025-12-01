// Dans CarRental.Api/Data/ApplicationDbContext.cs

using Microsoft.EntityFrameworkCore;
using CarRental2.Core.Entities;
using System;

namespace CarRental.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // =================================================================
        // Définition des DbSet (Mappage Entités <-> Tables)
        // =================================================================

        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleImage> VehicleImages { get; set; }
        public DbSet<Tariff> Tariffs { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Maintenance> Maintenances { get; set; }


        // =================================================================
        // Configuration des Relations et Contraintes (API Fluent)
        // =================================================================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- VEHICULE ET FLOTTE ---

            // Contrainte d'unicité sur le numéro de plaque
            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.PlateNumber)
                .IsUnique();

            // Relation Vehicle <-> VehicleType (1 Type a N Véhicules)
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.VehicleType)
                .WithMany(vt => vt.Vehicles)
                .HasForeignKey(v => v.VehicleTypeId);

            // Relation VehicleImage <-> Vehicle (1 Véhicule a N Images)
            modelBuilder.Entity<VehicleImage>()
                .HasOne(vi => vi.Vehicle)
                .WithMany(v => v.Images)
                .HasForeignKey(vi => vi.VehicleId);

            // Relation Tariff <-> VehicleType (1 Type a N Tarifs)
            modelBuilder.Entity<Tariff>()
                .HasOne(t => t.VehicleType)
                .WithMany(vt => vt.Tariffs)
                .HasForeignKey(t => t.VehicleTypeId);

            // Relation Maintenance <-> Vehicle (1 Véhicule a N Maintenances)
            modelBuilder.Entity<Maintenance>()
                .HasOne(m => m.Vehicle)
                .WithMany(v => v.Maintenances)
                .HasForeignKey(m => m.VehicleId);


            // --- RESERVATION ET TRANSACTIONS ---

            // Relation Reservation <-> Client (1 Client a N Réservations)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Client)
                .WithMany(c => c.Reservations)
                .HasForeignKey(r => r.ClientId);

            // Relation Reservation <-> Vehicle (1 Véhicule a N Réservations)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Vehicle)
                .WithMany(v => v.Reservations)
                .HasForeignKey(r => r.VehicleId)
                .IsRequired(false); // VehicleId est nullable

            // Relation Reservation <-> User (1 User a N Réservations créées)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.CreatedByUser)
                .WithMany(u => u.CreatedReservations)
                .HasForeignKey(r => r.CreatedByUserId)
                .IsRequired(false); // CreatedByUserId est nullable

            // Relation Payment <-> Reservation (1 Réservation a N Paiements)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Reservation)
                .WithMany(r => r.Payments)
                .HasForeignKey(p => p.ReservationId);

            // --- CONTRAINTE CLIENT/USER ---

            // Contrainte d'unicité sur l'email des Clients
            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Email)
                .IsUnique();

            // Contrainte d'unicité sur l'email des Users
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Contrainte d'unicité sur le DriverLicenseNumber des Clients
            modelBuilder.Entity<Client>()
                .HasIndex(c => c.DriverLicenseNumber)
                .IsUnique();
        }
    }
}
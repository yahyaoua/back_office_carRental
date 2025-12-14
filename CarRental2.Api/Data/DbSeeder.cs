using CarRental2.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Api.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            // Assure que la DB est bien à jour (migrations)
            await db.Database.MigrateAsync();

            // ✅ Déjà seedé ? on sort
            if (await db.VehicleTypes.AnyAsync() || await db.Vehicles.AnyAsync())
                return;

            // 1) VehicleTypes
            var sedan = new VehicleType { VehicleTypeId = Guid.NewGuid(), Name = "Sedan", Description = "Berline" };
            var suv = new VehicleType { VehicleTypeId = Guid.NewGuid(), Name = "SUV", Description = "Sport Utility Vehicle" };
            var util = new VehicleType { VehicleTypeId = Guid.NewGuid(), Name = "Utility", Description = "Utilitaire" };

            db.VehicleTypes.AddRange(sedan, suv, util);

            // 2) Vehicles
            db.Vehicles.AddRange(
                new Vehicle
                {
                    VehicleId = Guid.NewGuid(),
                    PlateNumber = "AA-123-AA",
                    Make = "Toyota",
                    Model = "Corolla",
                    Year = 2022,
                    VehicleTypeId = sedan.VehicleTypeId,
                    Mileage = 45000,
                    Status = "Available",
                    BaseRatePerDay = 45m,
                    NextMaintenanceDate = DateTime.UtcNow.AddMonths(2),
                    CreatedAt = DateTime.UtcNow
                },
                new Vehicle
                {
                    VehicleId = Guid.NewGuid(),
                    PlateNumber = "BB-456-BB",
                    Make = "Honda",
                    Model = "Civic",
                    Year = 2021,
                    VehicleTypeId = sedan.VehicleTypeId,
                    Mileage = 60000,
                    Status = "Available",
                    BaseRatePerDay = 42m,
                    NextMaintenanceDate = DateTime.UtcNow.AddMonths(1),
                    CreatedAt = DateTime.UtcNow
                },
                new Vehicle
                {
                    VehicleId = Guid.NewGuid(),
                    PlateNumber = "CC-789-CC",
                    Make = "Hyundai",
                    Model = "Tucson",
                    Year = 2023,
                    VehicleTypeId = suv.VehicleTypeId,
                    Mileage = 12000,
                    Status = "Available",
                    BaseRatePerDay = 70m,
                    NextMaintenanceDate = DateTime.UtcNow.AddMonths(3),
                    CreatedAt = DateTime.UtcNow
                }
            );

            await db.SaveChangesAsync();
        }
    }
}

using CarRental.Api.Data;
using CarRental.Api.ViewModels;
using CarRental2.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CarRental.Api.Controllers
{
    // Controller MVC pour pages Razor (PAS ApiController)
    [Route("Reservation")]
    public class ReservationPageController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ReservationPageController(ApplicationDbContext db)
        {
            _db = db;
        }




        // ✅ GET: /Reservation/Test
        [HttpGet("Test")]
        public IActionResult Test()
        {
            return Content("OK ReservationPageController");
        }






        // GET: /Reservation/Create/{id}
        [HttpGet("Create/{id:guid}")]
        public async Task<IActionResult> Create(Guid id)
        {
            var vehicle = await _db.Vehicles
                .Include(v => v.VehicleType)
                .FirstOrDefaultAsync(v => v.VehicleId == id);

            if (vehicle == null)
                return NotFound();

            var vm = new ReservationViewModel
            {
                VehicleId = vehicle.VehicleId,
                VehicleName = $"{vehicle.Make} {vehicle.Model} ({vehicle.Year})"
            };

            return View("~/Views/Reservation/Create.cshtml", vm);
        }



        [HttpPost("Create/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid id, ReservationViewModel model)
        {
            // sécurité : l’id route = véhicule choisi
            model.VehicleId = id;

            // Recharge le nom véhicule si jamais ModelState invalid
            var vehicle = await _db.Vehicles
                .Include(v => v.VehicleType)
                .FirstOrDefaultAsync(v => v.VehicleId == id);

            if (vehicle == null) return NotFound();
            model.VehicleName = $"{vehicle.Make} {vehicle.Model} ({vehicle.Year})";

            if (!ModelState.IsValid)
                return View("~/Views/Reservation/Create.cshtml", model);

            // 1) Trouver client par email
            var existingClient = await _db.Clients
                .FirstOrDefaultAsync(c => c.Email == model.ClientEmail);

            Client client;
            if (existingClient != null)
            {
                client = existingClient;
            }
            else
            {
                // 2) Créer client (IMPORTANT: champs Required dans Client.cs)
                var parts = (model.ClientName ?? "").Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var firstName = parts.Length > 0 ? parts[0] : "Unknown";
                var lastName = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : "Unknown";

                client = new Client
                {
                    ClientId = Guid.NewGuid(),
                    FirstName = firstName,
                    LastName = lastName,
                    Email = model.ClientEmail,
                    Phone = model.ClientPhone,

                    // champs obligatoires dans ton entity Client
                    DriverLicenseNumber = "TEMP-" + Guid.NewGuid().ToString("N").Substring(0, 10),
                    BirthDate = DateTime.UtcNow.Date, // temporaire
                    CreatedAt = DateTime.UtcNow
                };

                _db.Clients.Add(client);
                await _db.SaveChangesAsync(); // ✅ pour obtenir un ClientId validé
            }

            // 3) Créer réservation
            var reservation = new Reservation
            {
                ReservationId = Guid.NewGuid(),
                ClientId = client.ClientId,
                VehicleId = id,

                RequestedStart = model.RequestedStart,
                RequestedEnd = model.RequestedEnd,

                Status = "Pending",
                TotalAmount = 0m,
                DepositAmount = 0m,
                CreatedByUserId = null
            };

            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync(); // ✅ INSERT dbo.Reservations

            TempData["Success"] = "Reservation created!";
            return RedirectToAction("List", "Vehicle");
        }


    }
}













using CarRental.Api.Data;
using CarRental.Api.Services; // 📧
using CarRental.Api.ViewModels;
using CarRental2.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental.Api.Controllers
{
    [Route("Reservation")]
    public class ReservationPageController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly EmailService _emailService; // 📧

        public ReservationPageController(
            ApplicationDbContext db,
            EmailService emailService) // 📧
        {
            _db = db;
            _emailService = emailService;
        }

        // ============================
        // TEST
        // ============================
        [HttpGet("Test")]
        public IActionResult Test()
        {
            return Content("OK ReservationPageController");
        }

        // ============================
        // GET: /Reservation/Create/{id}
        // ============================
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

        // ============================
        // POST: /Reservation/Create/{id}
        // ============================
        [HttpPost("Create/{id:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid id, ReservationViewModel model)
        {
            Console.WriteLine(">>> POST Create Reservation called <<<");

            model.VehicleId = id;

            var vehicle = await _db.Vehicles
                .FirstOrDefaultAsync(v => v.VehicleId == id);

            if (vehicle == null)
                return NotFound();

            model.VehicleName = $"{vehicle.Make} {vehicle.Model} ({vehicle.Year})";

            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"❌ Model error on {entry.Key}: {error.ErrorMessage}");
                    }
                }

                return View("~/Views/Reservation/Create.cshtml", model);
            }

            // ============================
            // 1) CLIENT
            // ============================

            var existingClient = await _db.Clients
                .FirstOrDefaultAsync(c => c.Email == model.ClientEmail);

            Client client;

            if (existingClient != null)
            {
                client = existingClient;
            }
            else
            {
                client = new Client
                {
                    ClientId = Guid.NewGuid(),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.ClientEmail,
                    Phone = model.ClientPhone,
                    Address = model.Address,
                    DriverLicenseNumber = model.DriverLicenseNumber,
                    BirthDate = model.BirthDate,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Clients.Add(client);
                await _db.SaveChangesAsync();
            }

            // ============================
            // 2) RESERVATION
            // ============================

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
            await _db.SaveChangesAsync();

            // ============================
            // 📧 3) EMAIL CONFIRMATION
            // ============================

            await _emailService.SendAsync(
                model.ClientEmail,
                "Confirmation de votre réservation",
                $@"
                <h2>Réservation confirmée</h2>
                <p>Bonjour {model.FirstName} {model.LastName},</p>

                <p>
                    Votre réservation pour le véhicule
                    <b>{model.VehicleName}</b> a bien été enregistrée.
                </p>

                <p>
                    📅 Du <b>{model.RequestedStart:dd/MM/yyyy}</b>
                    au <b>{model.RequestedEnd:dd/MM/yyyy}</b>
                </p>

                <p>Merci pour votre confiance.</p>
                <p><b>CarRental Team</b></p>
                "
            );

            TempData["Success"] = "Reservation created successfully!";
            return RedirectToAction("List", "Vehicle");
        }

        // ============================
        // GET: /Reservation/MyReservations
        // ============================
        [Authorize]
        [HttpGet("MyReservations")]
        public async Task<IActionResult> MyReservations()
        {
            var clientEmail = User.Identity?.Name;

            if (string.IsNullOrEmpty(clientEmail))
                return RedirectToAction("Login", "Account");

            var client = await _db.Clients
                .FirstOrDefaultAsync(c => c.Email == clientEmail);

            if (client == null)
                return View("~/Views/Reservation/MyReservations.cshtml",
                    new MyReservationsViewModel());

            var reservations = await _db.Reservations
                .Include(r => r.Vehicle)
                .Where(r => r.ClientId == client.ClientId)
                .OrderByDescending(r => r.RequestedStart)
                .ToListAsync();

            var model = new MyReservationsViewModel
            {
                ClientName = $"{client.FirstName} {client.LastName}",
                Reservations = reservations.Select(r => new MyReservationItemViewModel
                {
                    ReservationId = r.ReservationId,
                    VehicleName = $"{r.Vehicle.Make} {r.Vehicle.Model}",
                    StartDate = r.RequestedStart,
                    EndDate = r.RequestedEnd,
                    TotalAmount = r.TotalAmount,
                    Status = r.Status
                }).ToList()
            };

            return View("~/Views/Reservation/MyReservations.cshtml", model);
        }

        // ============================
        // ❌ POST: /Reservation/Cancel
        // ============================
        [Authorize]
        [HttpPost("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid reservationId)
        {
            var clientEmail = User.Identity?.Name;

            if (string.IsNullOrEmpty(clientEmail))
                return RedirectToAction("Login", "Account");

            var client = await _db.Clients
                .FirstOrDefaultAsync(c => c.Email == clientEmail);

            if (client == null)
                return Unauthorized();

            var reservation = await _db.Reservations
                .FirstOrDefaultAsync(r =>
                    r.ReservationId == reservationId &&
                    r.ClientId == client.ClientId);

            if (reservation == null)
                return NotFound();

            if (reservation.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Cette réservation ne peut plus être annulée.";
                return RedirectToAction("MyReservations");
            }

            reservation.Status = "Cancelled";
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Réservation annulée avec succès.";

            return RedirectToAction("MyReservations");
        }
    }
}

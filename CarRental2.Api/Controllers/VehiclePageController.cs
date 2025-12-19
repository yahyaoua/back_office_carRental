using CarRental.Api.Data;
using CarRental.Api.ViewModels;
using CarRental2.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental.Api.Controllers
{
    // Ce controller sert UNIQUEMENT aux pages Razor (/Vehicle/List)
    [Route("Vehicle")]
    
    public class VehiclePageController : Controller
    {
        private readonly ApplicationDbContext _db;

        public VehiclePageController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Vehicle/List
        [HttpGet("List")]
        public async Task<IActionResult> List()
        {
            var vehicles = await _db.Vehicles
                .Include(v => v.VehicleType)
                .Include(v => v.Images)
                .ToListAsync();

            var vm = vehicles.Select(v => new VehicleViewModel
            {
                VehicleId = v.VehicleId,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                VehicleType = v.VehicleType != null ? v.VehicleType.Name : "N/A",

                // adapte selon ton ViewModel (DailyRate ou DailyRatePerDay)
                DailyRate = v.BaseRatePerDay,

                // Image principale si elle existe, sinon placeholder
                ImageUrl = (v.Images != null && v.Images.Any())
                    ? v.Images.OrderBy(i => i.VehicleImageId).First().ImagePath
                    : "\\image\\vehicles\\04671cbf55caeda3f3e12ebc8b8a1f36.jpg"
                     
            }).ToList();

            return View("~/Views/Vehicle/List.cshtml", vm);
        }
    }
}

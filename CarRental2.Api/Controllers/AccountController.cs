using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using CarRental.Api.ViewModels;
using CarRental2.Core.Entities;

// 🔐 AUTH
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AccountController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // ============================
    // LOGIN
    // ============================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(Guid? vehicleId)
    {
        ViewData["VehicleId"] = vehicleId;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        Guid? vehicleId,
        string returnUrl = null)
    {
        Console.WriteLine($"[LOGIN POST] vehicleId = {vehicleId}");

        if (!ModelState.IsValid)
        {
            ViewData["VehicleId"] = vehicleId;
            return View(model);
        }

        var client = _httpClientFactory.CreateClient();

        var payload = new
        {
            Identifier = model.Email,
            Password = model.Password
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        var response = await client.PostAsync(
            "http://localhost:5228/api/auth/login/client",
            content
        );

        if (!response.IsSuccessStatusCode)
        {
            var apiMessage = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", apiMessage);
            ViewData["VehicleId"] = vehicleId;
            return View(model);
        }

        // ============================
        // 🔐 AUTHENTIFICATION OK
        // ============================

        // (1) Session — conservée
        HttpContext.Session.SetString("IsAuthenticated", "true");

        // (2) Cookie Auth
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, model.Email)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true
            }
        );

        // ============================
        // REDIRECT (LOGIQUE INCHANGÉE)
        // ============================

        if (vehicleId.HasValue)
        {
            return RedirectToAction(
                "Create",
                "Reservation",
                new { id = vehicleId.Value }
            );
        }

        if (!string.IsNullOrEmpty(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    // ============================
    // REGISTER CLIENT
    // ============================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(Guid? vehicleId)
    {
        ViewData["VehicleId"] = vehicleId;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterClientViewModel model, Guid? vehicleId)
    {
        if (!ModelState.IsValid)
        {
            ViewData["VehicleId"] = vehicleId;
            return View(model);
        }

        var client = _httpClientFactory.CreateClient("ApiClient");

        var newClient = new Client
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Phone = model.Phone,
            Address = model.Address,
            DriverLicenseNumber = model.DriverLicenseNumber,
            BirthDate = model.BirthDate
        };

        var content = new StringContent(
            JsonSerializer.Serialize(newClient),
            Encoding.UTF8,
            "application/json"
        );

        var response = await client.PostAsync(
            $"http://localhost:5228/api/auth/register/client?password={model.Password}",
            content
        );

        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Account creation failed. Email may already exist.";
            ViewData["VehicleId"] = vehicleId;
            return View(model);
        }

        TempData["SuccessMessage"] = "Account created successfully. Please login.";

        return RedirectToAction("Login", new { vehicleId });
    }

    // ============================
    // LOGOUT
    // ============================

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        // 🔐 Supprimer le cookie
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        // 🧹 Nettoyage session
        HttpContext.Session.Clear();

        return RedirectToAction("Index", "Home");
    }
}

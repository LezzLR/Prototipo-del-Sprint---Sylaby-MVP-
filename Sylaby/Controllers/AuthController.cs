using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sylaby.Models;
using System.Security.Claims;

namespace Sylaby.Controllers
{
    public class AuthController : Controller
    {
        private readonly SylabyDbContext _context;

        public AuthController(SylabyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "El correo electrónico o la contraseña son incorrectos.");
                return View(model);
            }

            var passwordHasher = new PasswordHasher<User>();
            var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "El correo electrónico o la contraseña son incorrectos.");
                return View(model);
            }

            // Create user claims for cookie authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validar que el correo pertenezca al dominio institucional "@usmp.pe"
            if (!model.Email.EndsWith("@usmp.pe", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Email", "El correo electrónico debe pertenecer al dominio institucional '@usmp.pe'.");
                return View(model);
            }

            // Validar si el usuario ya existe
            var existingUser = await _context.Users.AnyAsync(u => u.Email.ToLower() == model.Email.ToLower());
            if (existingUser)
            {
                ModelState.AddModelError("Email", "Este correo electrónico ya se encuentra registrado.");
                return View(model);
            }

            var passwordHasher = new PasswordHasher<User>();
            var newUser = new User
            {
                Email = model.Email,
                Role = model.Role
            };
            newUser.PasswordHash = passwordHasher.HashPassword(newUser, model.Password);

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Auto sign-in after successful registration for a premium UX
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, newUser.Email),
                new Claim(ClaimTypes.Role, newUser.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

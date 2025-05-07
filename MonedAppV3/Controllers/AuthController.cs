using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using NugetMonedAppV2.Models;
using NugetMonedAppV2.DTOs;
using MonedAppV3.Services;

namespace MonedAppV3.Controllers
{
    public class AuthController : Controller
    {
        private ServiceMonedApp service;

        public AuthController(ServiceMonedApp service, IConfiguration configuration) {
            this.service = service;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            string token;
            string nombreUsuario;

            try {
                (token, nombreUsuario) = await this.service.GetTokenAsync(model.Email, model.Password);

                if (token == null) {
                    TempData["ErrorMessage"] = "Credenciales incorrectas.";
                    return View();
                }
            }
            catch (Exception ex) {
                TempData["ErrorMessage"] = $"Error al obtener el token: {ex.Message}";
                return View();
            }

            ClaimsIdentity identity = new ClaimsIdentity(
                CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Name, ClaimTypes.NameIdentifier
            );

            Claim claimUserName = new Claim(ClaimTypes.Name, nombreUsuario);
            Claim claimUserId = new Claim(ClaimTypes.NameIdentifier, token);

            identity.AddClaim(claimUserName);
            identity.AddClaim(claimUserId);

            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    ExpiresUtc = DateTime.Now.AddDays(1),
                }
            );

            return RedirectToAction("Index", "Dashboard");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string nombre, string dni, string email, string password) {
            RegisterDTO dto = new RegisterDTO
            {
                Nombre = nombre,
                Dni = dni,
                Email = email,
                Password = password
            };

            bool result = await this.service.RegisterUsuarioAsync(dto);

            if (result) {
                TempData["SuccessMessage"] = "Usuario registrado correctamente.";
            }
            else {
                TempData["ErrorMessage"] = "Error al registrar usuario.";
            }
            
            return View();
        }

        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}

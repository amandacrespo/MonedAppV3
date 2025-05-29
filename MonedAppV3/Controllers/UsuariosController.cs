using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MonedAppV3.Services;
using NugetMonedAppAws.DTOs;
using MonedAppV3.Filters;

namespace MonedAppV3.Controllers
{
    public class UsuariosController : Controller
    {
        private ServiceMonedApp service;

        public UsuariosController(ServiceMonedApp service) {
            this.service = service;
        }

        private string GetToken() {
            return HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }

        [AuthorizeUsers]
        public async Task<IActionResult> Index() {
            string token = this.GetToken();
            var (perfil, estadisticas) = await this.service.GetPerfilUsuarioAsync(token);
            ViewBag.Estadisticas = estadisticas;
            return View(perfil); 
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> ActualizarPerfil([FromBody] ActualizarPerfilDTO perfilActualizar) {
            if (perfilActualizar == null || string.IsNullOrEmpty(perfilActualizar.Nombre) || string.IsNullOrEmpty(perfilActualizar.CorreoElectronico)) {
                TempData["Mensaje"] = "Datos incompletos o inválidos.";
                TempData["MensajeTipo"] = "error";
                return Json(new { success = false });
            }

            try {
                string token = this.GetToken();
                bool success = await this.service.ActualizarPerfilUsuarioAsync(perfilActualizar, token);

                if (success) {
                    // Actualizar los claims de autenticación
                    var identity = (ClaimsIdentity)HttpContext.User.Identity;
                    if (identity != null) {
                        var currentNameClaim = identity.FindFirst(ClaimTypes.Name);
                        if (currentNameClaim != null) {
                            identity.RemoveClaim(currentNameClaim);
                        }
                        identity.AddClaim(new Claim(ClaimTypes.Name, perfilActualizar.Nombre));

                        var principal = new ClaimsPrincipal(identity);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    }

                    TempData["Mensaje"] = "Perfil actualizado correctamente.";
                    TempData["MensajeTipo"] = "success";
                    return Json(new { success = true });
                }

                TempData["Mensaje"] = "Error al actualizar el perfil.";
                TempData["MensajeTipo"] = "error";
                return Json(new { success = false });
            }
            catch (Exception ex) {
                TempData["Mensaje"] = ex.Message;
                TempData["MensajeTipo"] = "error";
                return Json(new { success = false });
            }
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> EliminarPerfil() {
            try {
                string token = this.GetToken();
                bool resultado = await this.service.EliminarPerfilUsuarioAsync(token);

                if (resultado) {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return Json(new { success = true, redirectUrl = "/Dashboard/Index" });
                }

                return Json(new { success = false });
            }
            catch {
                return Json(new { success = false });
            }
        }
    }
}

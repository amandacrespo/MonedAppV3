using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MonedAppV3.Filters;
using NugetMonedAppV2.DTOs;
using MonedAppV3.Services;

namespace MonedAppV3.Controllers
{
    public class CuentasController : Controller
    {
        private ServiceMonedApp service;

        public CuentasController(ServiceMonedApp service) {
            this.service = service;
        }

        private string GetToken() {
            return HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }

        [AuthorizeUsers]
        public async Task<IActionResult> Index() {
            ViewData["ActivePage"] = "IndexCU";

            string token = GetToken();
            var cuentasConMiembros = await this.service.GetCuentasAsync(token);

            return View(cuentasConMiembros);
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> CrearCuenta([FromBody] CuentaNuevaDTO cuenta) {
            try {
                if (cuenta == null) {
                    return Json(new { success = false, message = "Datos inválidos." });
                }

                string token = GetToken();
                bool success = await this.service.CrearCuentaAsync(cuenta, token);

                return Json(new { success, message = success ? "Cuenta creada con éxito." : "Error al crear la cuenta." });
            }
            catch (Exception ex) {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [AuthorizeUsers]
        public async Task<IActionResult> EditarCuenta(int idCuenta) {
            string token = GetToken();
            var cuenta = await this.service.FindCuentaAsync(idCuenta, token);

            if (cuenta == null) {
                return NotFound();
            }

            return Ok(cuenta);
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> EditarCuenta([FromBody] ActualizarCuentaDTO request) {
            try {
                string token = GetToken();
                var miembros = request.Miembros ?? new List<MiembroDTO>();
                request.Miembros = miembros;
                bool success = await this.service.EditarCuentaAsync((int)request.IdCuenta, request, token);

                return Json(new { success, message = success ? "Cuenta actualizada correctamente." : "Error al actualizar la cuenta." });
            }
            catch (Exception ex) {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [AuthorizeUsers]
        public async Task<IActionResult> ActivarCuenta(int id) {
            try {
                string token = GetToken();
                bool success = await this.service.ActivarCuentaAsync(id, token);
            }
            catch (Exception ex) {
                TempData["Mensaje"] = "Error al activar la cuenta: " + ex.Message;
                TempData["MensajeTipo"] = "error";
            }

            return RedirectToAction("Index");
        }

        [AuthorizeUsers]
        public async Task<IActionResult> DesactivarCuenta(int id) {
            try {
                string token = GetToken();
                bool success = await this.service.DesactivarCuentaAsync(id, token);
            }
            catch (Exception ex) {
                TempData["Mensaje"] = "Error al desactivar la cuenta: " + ex.Message;
                TempData["MensajeTipo"] = "error";
            }

            return RedirectToAction("Index");
        }

        [AuthorizeUsers]
        public async Task<IActionResult> EliminarCuenta(int id) {
            try {
                string token = GetToken();
                bool success = await this.service.EliminarCuentaAsync(id, token);
                TempData["Mensaje"] = success ? "Cuenta eliminada correctamente." : "Error al eliminar la cuenta.";
                TempData["MensajeTipo"] = success ? "success" : "error";
            }
            catch (Exception ex) {
                TempData["Mensaje"] = "Error al eliminar la cuenta: " + ex.Message;
                TempData["MensajeTipo"] = "error";
            }

            return RedirectToAction("Index");
        }

        [AuthorizeUsers]
        public async Task<IActionResult> CambiarRol(int idcuenta) {
            try {
                string token = GetToken();
                bool success = await this.service.CambiarRolAsync(idcuenta, token);

                if (success) {
                    HttpContext.Session.SetString("Solicitado", "true");
                    TempData["Mensaje"] = "Solicitud enviada correctamente.";
                    TempData["MensajeTipo"] = "success";
                }
                else {
                    TempData["Mensaje"] = "Error al enviar la solicitud.";
                    TempData["MensajeTipo"] = "error";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex) {
                TempData["Mensaje"] = "Error al enviar la solicitud: " + ex.Message;
                TempData["MensajeTipo"] = "error";
                return RedirectToAction("Index");
            }
        }
    }
}

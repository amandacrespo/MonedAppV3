using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MonedAppV3.Filters;
using NugetMonedAppV2.DTOs;
using MonedAppV3.Services;

namespace MonedAppV3.Controllers
{
    public class FondosReservaController : Controller
    {
        private ServiceMonedApp service;

        public FondosReservaController(ServiceMonedApp service) {
            this.service = service;
        }

        private string GetToken() {
            return HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }

        [AuthorizeUsers]
        public async Task<IActionResult> Index()
        {
            return await CargarVistaFondos(null);
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> Index(int idCuenta)
        {
            return await CargarVistaFondos(idCuenta);
        }

        private async Task<IActionResult> CargarVistaFondos(int? idCuenta) {
            ViewData["ActivePage"] = "IndexFR";

            string token = this.GetToken();

            // Obtener cuentas como administrador
            List<CuentaDTO> cuentasAdmin = await this.service.GetCuentasAdmiAsync(token);

            // Obtener cuentas como miembro
            List<CuentaConMiembrosDTO> cuentasUsuario = await this.service.GetCuentasAsync(token);

            if (!cuentasAdmin.Any() && cuentasUsuario.Any() || !cuentasUsuario.Any()) {
                return RedirectToAction("AccesoDenegado", "Auth");
            } 

            if (idCuenta == null && cuentasAdmin.Any()) {
                idCuenta = cuentasAdmin.First().IdCuenta;
            }

            FondosDTO datos = await this.service.GetFondosReservaAsync(token, idCuenta.Value);

            ViewBag.CuentaSeleccionada = idCuenta;
            ViewBag.Cuentas = cuentasAdmin;
            return View("Index", datos);
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> CreateFondo([FromBody] CrearFondoDTO model) {
            if (model == null)
                return BadRequest(new { success = false, message = "Datos inválidos." });

            string token = this.GetToken();

            try {
                bool resultado = await this.service.CrearFondoAsync(model, token);
                return Ok(new { success = resultado });
            }
            catch (Exception ex) {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [AuthorizeUsers]
        public async Task<IActionResult> ReactivarFondo(int id, int cuenta) {
            try {
                string token = this.GetToken();
                await this.service.ReactivarFondoAsync(id, token);
            }
            catch (Exception ex) {
                TempData["Mensaje"] = "Error al activar la cuenta: " + ex.Message;
                TempData["MensajeTipo"] = "error";
            }

            return await CargarVistaFondos(cuenta);
        }

        [AuthorizeUsers]
        public async Task<IActionResult> CompletarFondo(int id, int cuenta) {
            try {
                string token = this.GetToken();
                await this.service.CompletarFondoAsync(id, token);
            }
            catch (Exception ex) {
                TempData["Mensaje"] = "Error al desactivar la cuenta: " + ex.Message;
                TempData["MensajeTipo"] = "error";
            }

            return await CargarVistaFondos(cuenta);
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> EliminarFondo([FromBody] EliminarFondoRequest request) {
            try {
                string token = this.GetToken();
                await service.EliminarFondoAsync(request.IdFondo, token);
                return Json(new { success = true });
            }
            catch (Exception ex) {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class EliminarFondoRequest
        {
            public int IdFondo { get; set; }
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> CreateSubcategoria([FromBody] CrearSubcategoriaFondoDTO model) {
            try {
                string token = this.GetToken();
                var subcategoriaId = await this.service.CrearSubcategoriaAsync(model, token);
                return Json(new { success = true });
            }
            catch (Exception ex) {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> EliminarSubcategoria([FromBody] EliminarSubRequest request) {

            try {
                string token = this.GetToken();
                await service.EliminarSubcategoriaAsync(request.IdSubcategoria, token);
                return Json(new { success = true });
            }
            catch (Exception ex) {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class EliminarSubRequest
        {
            public int IdSubcategoria { get; set; }
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> ActualizarSubcategoria([FromBody] ActualizarSubcategoriaFondoDTO model) {
            if (model.MontoAsignado < 0) {
                return Json(new { success = false, message = "El monto asignado no puede ser negativo." });
            }

            string token = this.GetToken();

            try { 
                await service.ActualizarSubcategoriaAsync(model, token);
                return Json(new { success = true });
            }
            catch (Exception ex) {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> ActualizarMontoUtilizado([FromBody] ActualizarMontoSubcategoriaDTO model) {
            try {
                string token = this.GetToken();
                await service.ActualizarMontoUtilizadoAsync(model, token);
                return Json(new { success = true });
            }
            catch (Exception ex) {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

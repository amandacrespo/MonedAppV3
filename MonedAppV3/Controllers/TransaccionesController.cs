using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MonedAppV3.Filters;
using MonedAppV3.Services;
using NugetMonedAppAws.DTOs;

namespace MonedAppV3.Controllers
{
    public class TransaccionesController : Controller
    {

        private ServiceMonedApp service;

        public TransaccionesController(ServiceMonedApp service) {
            this.service = service;
        }

        private string GetToken() {
            return HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }

        [AuthorizeUsers]
        public async Task<IActionResult> Index()
        {
            return await CargarVistaTransacciones(null);
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> Index(int idCuenta)
        {
            return await CargarVistaTransacciones(idCuenta);
        }

        private async Task<IActionResult> CargarVistaTransacciones(int? idCuenta) {
            ViewData["ActivePage"] = "IndexTN";

            string token = this.GetToken();

            var datosDTO = await this.service.GetTransaccionesPorCuentaAsync(token, idCuenta);

            if (datosDTO == null || datosDTO.Cuentas.Count == 0) {
                return RedirectToAction("AccesoDenegado", "Auth");
            }

            ViewBag.Cuentas = datosDTO.Cuentas;
            ViewBag.Categorias = datosDTO.Categorias;
            ViewBag.CuentaSeleccionada = datosDTO.CuentaSeleccionada;
            ViewBag.FechaCreacion = datosDTO.FechaCreacion;

            return View("Index", datosDTO.Transacciones);
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> ActualizarTransaccion([FromBody] TransaccionDTO transaccion) {
            try {
                string token = this.GetToken();
                bool actualizado = await this.service.ActualizarTransaccionAsync(transaccion, token);
                return Json(new { success = actualizado });
            }
            catch (Exception ex) {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> CrearTransaccion([FromBody] CrearTransaccionDTO nuevaTransaccion) {
            if (nuevaTransaccion == null || nuevaTransaccion.Monto <= 0) {
                return Json(new { success = false, message = "Datos inválidos." });
            }

            try {
                string token = this.GetToken();
                bool creado = await this.service.CrearTransaccionAsync(nuevaTransaccion, token);
                return Json(new { success = creado, message = "Transacción agregada con éxito" });
            }
            catch (Exception ex) {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [AuthorizeUsers]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id) {
            try {
                string token = this.GetToken();
                bool eliminado = await this.service.EliminarTransaccionAsync(id, token);
                return Json(new { success = eliminado, message = "Registro eliminado correctamente." });
            }
            catch (Exception ex) {
                return Json(new { success = false, message = "Hubo un error al eliminar la transacción: " + ex.Message });
            }
        }
    }
}

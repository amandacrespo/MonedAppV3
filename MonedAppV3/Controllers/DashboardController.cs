using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MonedAppV3.Filters;
using MonedAppV3.Services;
using NugetMonedAppAws.DTOs;

namespace MonedAppV3.Controllers
{
    public class DashboardController : Controller
    {
        private ServiceMonedApp service;

        public DashboardController(ServiceMonedApp service) {
            this.service = service;
        }

        [AuthorizeUsers]
        public async Task<IActionResult> Index() {
            ViewData["ActivePage"] = "IndexDS";

            string token = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Llamar a la API sin idCuenta (usará la cuenta por defecto)
            DashboardInfoDTO info = await this.service.GetDashboardAsync(token);

            if (info != null) {
                ViewBag.CuentaSeleccionada = info.IdCuenta;
                ViewData["DashboardInfo"] = info;
                return View();
            }
            else {
                return RedirectToAction("Index", "Cuentas");
            }
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> Index(int idCuenta) {
            ViewData["ActivePage"] = "IndexDS";

            string token = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            ViewBag.CuentaSeleccioanda = idCuenta;

            // Llamar a la API con el ID de la cuenta seleccionada
            DashboardInfoDTO info = await this.service.GetDashboardAsync(token, idCuenta);

            if (info != null) {
                ViewBag.CuentaSeleccionada = info.IdCuenta;
                ViewData["DashboardInfo"] = info;
                return View();
            }
            else {
                return RedirectToAction("Index", "Cuentas");
            }
        }

        [AuthorizeUsers]
        public async Task<IActionResult> ObtenerDatosGrafico(int idCuenta) {
            string token = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var datosGrafico = await this.service.GetDatosGraficoAsync(idCuenta, token);

            if (datosGrafico == null || !datosGrafico.Success) {
                return Json(new { success = false, message = "No hay transacciones disponibles para esta cuenta." });
            }

            return Json(datosGrafico);
        }
    }
}

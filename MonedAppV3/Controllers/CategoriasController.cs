using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MonedAppV3.Filters;
using MonedAppV3.Services;
using NugetMonedAppV2.DTOs;

namespace MonedAppV3.Controllers
{
    public class CategoriasController : Controller
    {

        private ServiceMonedApp service;

        public CategoriasController(ServiceMonedApp service) {
            this.service = service;
        }

        private string GetToken() {
            return HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }

        [AuthorizeUsers]
        public async Task<IActionResult> Index() {
            ViewData["ActivePage"] = "IndexCT";

            string token = this.GetToken();

            // Obtener cuentas como administrador
            List<CuentaDTO> cuentasAdmin = await this.service.GetCuentasAdmiAsync(token);
            // Obtener cuentas como miembro
            List<CuentaConMiembrosDTO> cuentasUsuario = await this.service.GetCuentasAsync(token);

            if (!cuentasAdmin.Any() && cuentasUsuario.Any() || !cuentasUsuario.Any()) {
                return RedirectToAction("AccesoDenegado", "Auth");
            }
            else {
                CategoriasUsuarioDTO categoriasUsuarioDTO = await this.service.GetCategoriasUsuarioAsync(token);

                List<CategoriaDTO> categoriasPredefinidas = categoriasUsuarioDTO.Predefinidas;
                List<CategoriaConCuentaDTO> categoriasPersonalizadas = categoriasUsuarioDTO.Personalizadas;

                ViewBag.Personalizadas = categoriasPersonalizadas;
                ViewBag.Cuentas = cuentasAdmin;

                return View(categoriasPredefinidas);
            }
        }

        [AuthorizeUsers]
        [HttpGet]
        public async Task<IActionResult> ObtenerCategoria(int id) {
            string token = this.GetToken();
            var categoria = await this.service.GetCategoriaByIdAsync(id, token);

            if (categoria == null) {
                return NotFound(new { success = false, message = "Categoría no encontrada." });
            }

            return Ok(new
            {
                success = true,
                idCategoria = categoria.IdCategoria,
                nombre = categoria.Nombre,
                descripcion = categoria.Descripcion,
                tipo = categoria.Tipo
            });
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> CrearCategoria([FromBody] CategoriaConCuentaDTO nuevaCategoria) {
            if (nuevaCategoria == null || string.IsNullOrEmpty(nuevaCategoria.Nombre) || string.IsNullOrEmpty(nuevaCategoria.Tipo)) {
                return Json(new { success = false, message = "Datos inválidos" });
            }

            string token = this.GetToken();

            try {
                await this.service.CrearCategoriaAsync(nuevaCategoria, token);
                return Json(new { success = true, message = "Categoría creada correctamente" });
            }
            catch (Exception ex) {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [AuthorizeUsers]
        [HttpPost]
        public async Task<IActionResult> EstadisticasCategorias([FromBody] CuentaRequest request) {
            if (request.IdCuenta < 0) {
                return BadRequest("Se requiere una cuenta válida.");
            }

            string token = this.GetToken();
            List<TransaccionesPorMesDTO> transacciones = await this.service.GetEstadisticasCategoriaAsync(request.IdCuenta, token);

            return Ok(transacciones);
        }

        public class CuentaRequest
        {
            public int IdCuenta { get; set; }
        }

        [AuthorizeUsers]
        [HttpPut]
        public async Task<IActionResult> EditarCategoria([FromBody] CategoriaDTO categoria) {
            try {
                string token = this.GetToken();
                await this.service.EditarCategoriaAsync(categoria, token);
                return Ok(new { success = true, message = "Categoría editada correctamente." });
            }
            catch (Exception ex) {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [AuthorizeUsers]
        [HttpDelete]
        public async Task<IActionResult> Eliminar(int id) {
            try {
                string token = this.GetToken();
                await this.service.EliminarCategoriaAsync(id, token);
                return Ok(new { message = "Categoría eliminada correctamente." });
            }
            catch (Exception ex) {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}

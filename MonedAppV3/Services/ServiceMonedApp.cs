using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using NugetMonedAppAws.Models;
using NugetMonedAppAws.DTOs;

namespace MonedAppV3.Services
{
    public class ServiceMonedApp {
        private string UrlApi;
        private MediaTypeWithQualityHeaderValue Header;
        private IHttpContextAccessor ContextAccesor;

        public ServiceMonedApp(IConfiguration configuration, IHttpContextAccessor contextAccesor) {
            UrlApi = configuration.GetSection("ApiUrls:ApiAWS").Value;
            Header = new MediaTypeWithQualityHeaderValue("application/json");
            ContextAccesor = contextAccesor;
        }

        public async Task<(string token, string nombreUsuario)> GetTokenAsync(string email, string password) {
            LoginModel model = new LoginModel
            {
                Email = email,
                Password = password
            };

            using (HttpClient client = new HttpClient()) {
                string request = "api/auth/login";
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);

                string json = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);

                if (response.IsSuccessStatusCode) {
                    string data = await response.Content.ReadAsStringAsync();
                    JObject jObject = JObject.Parse(data);
                    string token = jObject.GetValue("token").ToString();
                    string nombreUsuario = jObject.GetValue("nombreUsuario").ToString();
                    return (token, nombreUsuario);
                }
                else {
                    return (null, "Petición incorrecta: " + response.StatusCode);
                }
            }
        }

        private async Task<T> CallApiAsync<T>(string request) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(UrlApi);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);

                HttpResponseMessage response = await client.GetAsync(request);

                if (response.IsSuccessStatusCode) {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else {
                    return default(T);
                }
            }
        }

        private async Task<T> CallApiAsync<T>(string request, string token) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(UrlApi);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

                HttpResponseMessage response = await client.GetAsync(request);

                if (response.IsSuccessStatusCode) {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else {
                    return default(T);
                }
            }
        }

        private async Task<bool> PostApiAsync(string request, object data, string token = null) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(UrlApi);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(Header);

                if (!string.IsNullOrEmpty(token)) {
                    client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
                }

                string json = JsonConvert.SerializeObject(data);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(request, content);

                return response.IsSuccessStatusCode;
            }
        }

        private async Task<bool> PutApiAsync(string request, object data, string token = null) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(UrlApi);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(Header);

                if (!string.IsNullOrEmpty(token)) {
                    client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
                }

                string json = JsonConvert.SerializeObject(data);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PutAsync(request, content);

                return response.IsSuccessStatusCode;
            }
        }

        private async Task<bool> PatchApiAsync(string request, string token = null) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(UrlApi);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(Header);

                if (!string.IsNullOrEmpty(token)) {
                    client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
                }

                HttpResponseMessage response = await client.PatchAsync(request, null);
                return response.IsSuccessStatusCode;
            }
        }

        private async Task<bool> DeleteApiAsync(string request, string token = null) {
            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(UrlApi);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(Header);

                if (!string.IsNullOrEmpty(token)) {
                    client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
                }

                HttpResponseMessage response = await client.DeleteAsync(request);
                return response.IsSuccessStatusCode;
            }
        }

        #region DASHBOARD

        public async Task<DashboardInfoDTO> GetDashboardAsync(string token, int? idCuenta = null) {
            try {
                string request = "api/dashboard";

                // Agregar idCuenta a la URL si se proporciona
                if (idCuenta.HasValue) {
                    request += $"?idCuenta={idCuenta.Value}";
                }

                var response = await CallApiAsync<DashboardInfoDTO>(request, token);

                return response;
            }
            catch (Exception ex) {
                return null;
            }
        }

        public async Task<DatosGraficoDTO> GetDatosGraficoAsync(int idCuenta, string token) {
            try {
                string request = $"api/dashboard/obtenerdatosgrafico/{idCuenta}";
                var response = await CallApiAsync<DatosGraficoDTO>(request, token);
                return response;
            }
            catch (Exception ex) {
                return null;
            }
        }

        #endregion

        #region CUENTAS

        public async Task<List<CuentaConMiembrosDTO>> GetCuentasAsync(string token) {
            var response = await CallApiAsync<dynamic>("api/cuentas", token);
            return response.ToObject<List<CuentaConMiembrosDTO>>();
        }

        public async Task<List<CuentaDTO>> GetCuentasAdmiAsync(string token) {
            var response = await CallApiAsync<dynamic>("api/cuentas/admin", token);
            return response.ToObject<List<CuentaDTO>>();
        }

        public async Task<CuentaDTO> FindCuentaAsync(int idCuenta, string token) {
            return await CallApiAsync<CuentaDTO>($"api/cuentas/{idCuenta}", token);
        }

        public async Task<bool> CrearCuentaAsync(CuentaNuevaDTO cuenta, string token) {
            return await PostApiAsync("api/cuentas", cuenta, token);
        }

        public async Task<bool> EditarCuentaAsync(int idCuenta, ActualizarCuentaDTO cuenta, string token) {
            return await PutApiAsync($"api/cuentas/{idCuenta}", cuenta, token);
        }

        public async Task<bool> CambiarRolAsync(int idCuenta, string token) {
            return await PostApiAsync($"api/cuentas/{idCuenta}/CambiarRol", new { }, token);
        }

        public async Task<bool> ActivarCuentaAsync(int id, string token) {
            return await PatchApiAsync($"api/cuentas/{id}/activar", token);
        }

        public async Task<bool> DesactivarCuentaAsync(int id, string token) {
            return await PatchApiAsync($"api/cuentas/{id}/desactivar", token);
        }

        public async Task<bool> EliminarCuentaAsync(int id, string token) {
            return await DeleteApiAsync($"api/cuentas/{id}", token);
        }

        #endregion

        #region USUARIOS

        public async Task<bool> RegisterUsuarioAsync(RegisterDTO usuario) {
            string request = "api/auth/register";
            return await this.PostApiAsync(request, usuario);
        }

        public async Task<(UsuarioDTO perfil, EstadisticasPerfilDTO estadisticas)> GetPerfilUsuarioAsync(string token) {
            string request = "api/usuarios/perfil";

            JObject jObject = await CallApiAsync<JObject>(request, token);

            if (jObject != null) {
                var perfil = jObject["usuarioPerfil"].ToObject<UsuarioDTO>();
                var estadisticas = jObject["estadisticas"].ToObject<EstadisticasPerfilDTO>();
                return (perfil, estadisticas);
            }

            return (null, null);
        }

        public async Task<bool> ActualizarPerfilUsuarioAsync(ActualizarPerfilDTO perfil, string token) {
            string request = "api/usuarios";
            return await PutApiAsync(request, perfil, token);
        }

        public async Task<bool> EliminarPerfilUsuarioAsync(string token) {
            string request = "api/usuarios";
            return await DeleteApiAsync(request, token);
        }

        #endregion

        #region TRANSACCIONES

        public async Task<DatosTransaccionesDTO> GetTransaccionesPorCuentaAsync(string token, int? idCuenta = null) {
            string request = "api/transacciones/cuenta";

            if (idCuenta.HasValue) {
                request += $"?idCuenta={idCuenta.Value}";
            }

            return await CallApiAsync<DatosTransaccionesDTO>(request, token);
        }

        public async Task<TransaccionDTO> FindTransaccionAsync(int idTransaccion, string token) {
            string request = $"api/transacciones/{idTransaccion}";
            return await CallApiAsync<TransaccionDTO>(request, token);
        }

        public async Task<bool> CrearTransaccionAsync(CrearTransaccionDTO transaccion, string token) {
            string request = "api/transacciones";
            return await PostApiAsync(request, transaccion, token);
        }

        public async Task<bool> ActualizarTransaccionAsync(TransaccionDTO transaccion, string token) {
            string request = "api/transacciones";
            return await PutApiAsync(request, transaccion, token);
        }

        public async Task<bool> EliminarTransaccionAsync(int idTransaccion, string token) {
            string request = $"api/transacciones/{idTransaccion}";
            return await DeleteApiAsync(request, token);
        }

        #endregion

        #region CATEGORIAS

        public async Task<CategoriasUsuarioDTO> GetCategoriasUsuarioAsync(string token) {
            string request = "api/categorias/usuario";
            return await CallApiAsync<CategoriasUsuarioDTO>(request, token);
        }

        public async Task<CategoriaDTO> GetCategoriaByIdAsync(int idCategoria, string token) {
            string request = $"api/categorias/{idCategoria}";
            return await CallApiAsync<CategoriaDTO>(request, token);
        }

        public async Task<List<TransaccionesPorMesDTO>> GetEstadisticasCategoriaAsync(int idCuenta, string token) {
            string request = $"api/categorias/estadisticas/{idCuenta}";
            return await CallApiAsync<List<TransaccionesPorMesDTO>>(request, token);
        }

        public async Task<bool> CrearCategoriaAsync(CategoriaConCuentaDTO categoria, string token) {
            string request = "api/categorias";
            return await PostApiAsync(request, categoria, token);
        }

        public async Task<bool> EditarCategoriaAsync(CategoriaDTO categoria, string token) {
            string request = "api/categorias";
            return await PutApiAsync(request, categoria, token);
        }

        public async Task<bool> EliminarCategoriaAsync(int idCategoria, string token) {
            string request = $"api/categorias/{idCategoria}";
            return await DeleteApiAsync(request, token);
        }

        #endregion

        #region FONDOS DE RESERVA

        public async Task<FondosDTO> GetFondosReservaAsync(string token, int? idCuenta = null) {
            string request = "api/fondosreserva/cuenta";
            if (idCuenta.HasValue) {
                request += $"?idCuenta={idCuenta.Value}";
            }
            return await CallApiAsync<FondosDTO>(request, token);
        }

        public async Task<bool> CrearFondoAsync(CrearFondoDTO fondo, string token) {
            string request = "api/fondosreserva";
            return await PostApiAsync(request, fondo, token);
        }

        public async Task<bool> CrearSubcategoriaAsync(CrearSubcategoriaFondoDTO subcategoria, string token) {
            string request = "api/fondosreserva/subcategoria";
            return await PostApiAsync(request, subcategoria, token);
        }

        public async Task<bool> ActualizarSubcategoriaAsync(ActualizarSubcategoriaFondoDTO subcategoria, string token) {
            string request = "api/fondosreserva/subcategoria";
            return await PutApiAsync(request, subcategoria, token);
        }

        public async Task<bool> ActualizarMontoUtilizadoAsync(ActualizarMontoSubcategoriaDTO model, string token) {
            string request = "api/fondosreserva/subcategoria/monto";

            using (HttpClient client = new HttpClient()) {
                client.BaseAddress = new Uri(UrlApi);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(Header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

                string json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PatchAsync(request, content);

                return response.IsSuccessStatusCode;
            }
        }

        public async Task<bool> ReactivarFondoAsync(int idFondo, string token) {
            string request = $"api/fondosreserva/reactivar/{idFondo}";
            return await PatchApiAsync(request, token);
        }

        public async Task<bool> CompletarFondoAsync(int idFondo, string token) {
            string request = $"api/fondosreserva/completar/{idFondo}";
            return await PatchApiAsync(request, token);
        }

        public async Task<bool> EliminarFondoAsync(int idFondo, string token) {
            string request = $"api/fondosreserva/{idFondo}";
            return await DeleteApiAsync(request, token);
        }

        public async Task<bool> EliminarSubcategoriaAsync(int idSubcategoria, string token) {
            string request = $"api/fondosreserva/subcategoria/{idSubcategoria}";
            return await DeleteApiAsync(request, token);
        }

        #endregion
    }
}

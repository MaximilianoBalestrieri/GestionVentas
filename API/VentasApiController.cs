using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionVentas.Models;

namespace GestionVentas.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentasApiController : ControllerBase
    {
        private ConexionDB conexion = new ConexionDB();

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult ObtenerVentas()
        {
            var usuario = User.Identity.Name;

            var ventas = conexion.ObtenerVentasPorUsuario(usuario);

            return Ok(ventas);
        }
    }
}

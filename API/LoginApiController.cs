using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestionVentas.Models;

namespace GestionVentas.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginApiController : ControllerBase
    {
        private readonly ConexionDB _conexion;
        private readonly string _jwtKey;

        public LoginApiController(ConexionDB conexion, IConfiguration config)
        {
            _conexion = conexion;

            // 1. Intentamos leer la clave desde variables de entorno (Render)
            // 2. Si no existe, usamos una clave local para desarrollo
            _jwtKey =
                Environment.GetEnvironmentVariable("JWT_KEY") ??
                config["Jwt:Key"] ??
                "clave_local_para_desarrollo_999";
        }

        [HttpPost]
        public IActionResult LoginApi([FromBody] UsuarioLogin login)
        {
            var user = _conexion.BuscarUsuario(login.Usuario, login.Contraseña);

            if (user == null)
                return Unauthorized("Usuario o contraseña incorrectos");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.NombreyApellido),
                new Claim("NombreyApellido", user.NombreyApellido),
                new Claim(ClaimTypes.Role, user.Rol)
            };

            var keyBytes = Encoding.UTF8.GetBytes(_jwtKey);
            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "GestionVentas",
                audience: "GestionVentas",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }
    }

    public class UsuarioLogin
    {
        public string Usuario { get; set; }
        public string Contraseña { get; set; }
    }
}

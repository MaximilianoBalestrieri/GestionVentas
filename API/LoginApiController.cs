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
        private ConexionDB conexion = new ConexionDB();

        [HttpPost]
        public IActionResult LoginApi([FromBody] UsuarioLogin login)
        {
            var user = conexion.BuscarUsuario(login.Usuario, login.Contraseña);

            if (user == null)
                return Unauthorized("Usuario o contraseña incorrectos");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.NombreyApellido),
                new Claim("NombreyApellido", user.NombreyApellido),
                new Claim(ClaimTypes.Role, user.Rol)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("clave_super_secreta_para_token123!"));
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

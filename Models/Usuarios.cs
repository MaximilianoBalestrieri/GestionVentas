
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace GestionVentas.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string UsuarioNombre { get; set; }
        public string Contraseña { get; set; }  // Esto exige setearla sí o sí
        public string Rol { get; set; }
        public string NombreyApellido { get; set; }
        public string? FotoPerfil { get; set; }
        [NotMapped]
        public IFormFile? FotoSubida { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionVentas.Models
{
    [Table("clientes")] // Forzamos el nombre de la tabla en la DB
    public class Cliente
    {
        [Key] // Indicamos que es la clave primaria
        public int IdCliente { get; set; }
        public string DniCliente { get; set; }
        public string NombreCliente { get; set; }
        public string Domicilio { get; set; }
        public string Localidad { get; set; }
        public string TelefonoCliente { get; set; }
        
        // Propiedad vital para la Cuenta Corriente
        public decimal SaldoActual { get; set; } 
    }
}
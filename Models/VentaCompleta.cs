using System.Collections.Generic;

namespace GestionVentas.Models
{
    public class VentaCompleta
    {
        public int IdCliente { get; set; }
        public decimal MontoVenta { get; set; }
        public string Vendedor { get; set; }
        public string MedioPago { get; set; }
        public string TipoVenta { get; set; } 
        public List<FacturaItem> Items { get; set; }
    }


}
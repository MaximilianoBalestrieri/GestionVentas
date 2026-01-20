namespace GestionVentas.Models
{
    public class Factura
    {
        public int IdFactura { get; set; }
        public DateTime DiaVenta { get; set; }
        public decimal MontoVenta { get; set; }
        public string Vendedor { get; set; }
        
        // Relación con la tabla Clientes
        public int IdCliente { get; set; } // Aquí guardaremos el '8' para Varios
        public string NombreCliente { get; set; }
        public string DniCliente { get; set; }
        public string Domicilio { get; set; }
        public string Localidad { get; set; }
        public string TelefonoCliente { get; set; }

        // --- NUEVOS CAMPOS PARA CAJA Y FIADO ---
        public string MedioPago { get; set; } // Efectivo, Transferencia, Tarjeta, CtaCte
        public string TipoVenta { get; set; } // Contado o Cuenta Corriente

        public List<FacturaItem> Items { get; set; } = new List<FacturaItem>();
    }

    public class FacturaItem
    {
        public int IdProducto { get; set; } // Te sugiero agregarlo para control de stock
        public string NombreProd { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Subtotal => Cantidad * Precio; // Campo calculado automático
    }
}

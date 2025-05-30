namespace GestionVentas.Models
{
    public class Factura
    {
        public int IdFactura { get; set; }
        public DateTime DiaVenta { get; set; }
        public decimal MontoVenta { get; set; }
        public string Vendedor { get; set; }
        public string NombreCliente { get; set; }
        public string DniCliente { get; set; }
        public string Domicilio { get; set; }
        public string Localidad { get; set; }
        public string TelefonoCliente { get; set; }


        public List<FacturaItem> Items { get; set; } = new List<FacturaItem>();
    }

    public class FacturaItem
    {
        public string NombreProd { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
    }

}

namespace GestionVentas.Models
{

    public class Presupuesto
    {
        public int IdPresupuesto { get; set; }
        public string NombreCliente { get; set; }
        public string? TelefonoCliente { get; set; }
        public DateTime Fecha { get; set; }

        public List<PresupuestoItem> Items { get; set; } = new List<PresupuestoItem>();
        public List<Producto> Productos { get; set; }
    }

    public class PresupuestoItem
    {
        public int IdItem { get; set; }
        public int IdPresupuesto { get; set; }
         public string Nombre { get; set; }
       
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal => Cantidad * PrecioUnitario;
    }

    public class Producto
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
}
}
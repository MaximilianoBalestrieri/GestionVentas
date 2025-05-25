namespace GestionVentas.Models
{

    public class Venta
    {
        public int IdFactura { get; set; }
        public DateTime DiaVenta { get; set; }
        public decimal MontoVenta { get; set; }
    }

public class VentaConProductos
{
    public List<ProductoVendido> Productos { get; set; }
    public decimal MontoVenta { get; set; }
}

}
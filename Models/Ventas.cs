namespace GestionVentas.Models
{

    public class Venta
    {
       public int IdFactura { get; set; }
    public DateTime DiaVenta { get; set; }
    public decimal MontoVenta { get; set; }
    public string Vendedor { get; set; }
    public int IdCliente { get; set; }
    }
public class ItemFactura
{
    public int IdProducto { get; set; }
    public string NombreProd { get; set; }
    public int Cantidad { get; set; }
    public decimal Precio { get; set; }
}

public class VentaConProductos
{
    public List<ProductoEnVenta> Productos { get; set; }
    public decimal MontoVenta { get; set; }
}

public class ProductoEnVenta
{
    public int IdProducto { get; set; }
    public string NombreProd { get; set; }
    public int Cantidad { get; set; }
    public decimal Precio { get; set; }
}

 public class VentaDTO
    {
        public int IdCliente { get; set; }
        public string Vendedor { get; set; }
        public decimal MontoVenta { get; set; }
        public List<ProductoEnVenta> Productos { get; set; }
    }
    public class FacturaDTO
    {
        public int ClienteId { get; set; }
        public decimal MontoVenta { get; set; }
        public string Vendedor { get; set; }
        public List<FacturaItemDTO> Productos { get; set; }
    }

    public class FacturaItemDTO
    {
        public int IdProducto { get; set; }
        public string NombreProd { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
    }





}
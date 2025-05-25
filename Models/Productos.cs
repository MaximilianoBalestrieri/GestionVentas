namespace GestionVentas.Models
{

    public class Productos
    {
        public int IdProducto { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public decimal PrecioCosto { get; set; }
       public decimal RecargoPorcentaje { get; set; }
        public decimal PrecioVenta
        {
            get
            {
                return PrecioCosto + (PrecioCosto * RecargoPorcentaje / 100);
            }
            
        }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public string Proveedor { get; set; }
        public string? Imagen { get; set; }

    }
}
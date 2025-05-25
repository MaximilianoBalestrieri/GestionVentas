namespace GestionVentas.Models
{
    public class ProductoVendido
    {
        // Debe coincidir con la propiedad que envías en el JSON:
        public int IdProducto { get; set; }
        public int CantidadVendida { get; set; }
    }
}
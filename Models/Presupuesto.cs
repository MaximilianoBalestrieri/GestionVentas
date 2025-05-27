namespace GestionVentas.Models
{

    public class Presupuesto
{
    public int IdPresupuesto { get; set; }
    public string NombreCliente { get; set; }
    public string? TelefonoCliente { get; set; }
    public DateTime Fecha { get; set; }

    public List<PresupuestoItem> Items { get; set; } = new List<PresupuestoItem>();
}

   public class PresupuestoItem
{
    public int IdItem { get; set; }
    public int IdPresupuesto { get; set; }
    public string Descripcion { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal => Cantidad * PrecioUnitario;
}

}
namespace GestionVentas.Models
{
public class MovimientoCtaCte
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Concepto { get; set; }
    public decimal Monto { get; set; }
    public decimal SaldoResultante { get; set; }

    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
}
}
namespace GestionVentas.Models
{

    public class Cliente
    {
        public int IdCliente { get; set; }
        public string DniCliente { get; set; }
        public string NombreCliente { get; set; }
        public string Domicilio { get; set; }
        public string Localidad { get; set; }
    }
}
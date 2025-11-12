namespace RegistroPacientesApp.Models
{
    public class Procedimiento
    {
        public int Id { get; set; }
        public int PacienteId { get; set; }
        public string Fecha { get; set; }
        public string Dia { get; set; }
        public string Actividad { get; set; }
        public decimal Valor { get; set; }
        public decimal Pago { get; set; }
        public decimal Saldo { get; set; }
    }
}

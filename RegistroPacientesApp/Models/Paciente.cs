namespace RegistroPacientesApp.Models
{
    public class Paciente
    {
        public int Id { get; set; }
        public string Cedula { get; set; }
        public string Nombres { get; set; }
        public string FechaNacimiento { get; set; }
        public int Edad { get; set; }
        public string EstadoCivil { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Ocupacion { get; set; }
        public string Antecedentes { get; set; }
    }
}

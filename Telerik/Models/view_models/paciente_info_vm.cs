namespace Telerik.Models.ViewModels
{
    public class PacienteInfoVm
    {
        public string NombreCompleto { get; set; }
        public string Edad { get; set; }
        public string Puesto { get; set; }
        public string Area { get; set; }
        public string Empresa { get; set; }
        public string Sexo { get; set; }
        public string Tipo { get; set; }
        public int? TipoServicioId { get; set; }
        public string TipoServicioDesc { get; set; }
        public string NumeroEmpleado { get; set; }
        
        // Datos Demográficos para Autocompletado
        public string FechaNacimiento { get; set; }
        public string Nss { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string EstadoCivil { get; set; }
        public string TipoSangre { get; set; }
        public string Rfc { get; set; }
        public string Curp { get; set; }
        public bool TieneHijos { get; set; }
        public string NumeroHijos { get; set; }
        public string Escolaridad { get; set; }
    }
}

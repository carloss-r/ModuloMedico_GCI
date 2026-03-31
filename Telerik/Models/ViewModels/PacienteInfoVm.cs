namespace Telerik.Models.ViewModels
{
    public class PacienteInfoVm
    {
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
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

        // Geographic FKs for pre-populating cascading dropdowns
        public int? FkPais { get; set; }
        public int? FkEstado { get; set; }
        public int? FkMunicipio { get; set; }
        public int? FkColonia { get; set; }
        public int? FkCP { get; set; }
        public string CPDesc { get; set; }
        public string Calle { get; set; }
        public string NumExterior { get; set; }
        public string NumInterior { get; set; }
        public int? FkEmpresa { get; set; }
    }
}

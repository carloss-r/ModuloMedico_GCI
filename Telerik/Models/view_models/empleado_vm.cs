using System;

namespace Telerik.Models.ViewModels
{
    public class EmpleadoVm
    {
        public int PkEmpleado { get; set; }
        public string Nombre { get; set; }
        public string APaterno { get; set; }
        public string AMaterno { get; set; }
        public string Rfc { get; set; }
        public string Curp { get; set; }
        public string Nss { get; set; }
        public string PuestoDesc { get; set; }
        public string ProyectoDesc { get; set; }
        public string Sexo { get; set; }
        public string Edad { get; set; }
        public  DateTime? FechaNacimiento { get; set; }
        public string AreaDesc { get; set; }
        public string EmpresaDesc { get; set; }
        
        // Relacionados a Evaluación Médica
        public string Telefono { get; set; }
        public string Calle { get; set; }
        public string NumExterior { get; set; }
        public string NumInterior { get; set; }
        public string EstadoCivil { get; set; }
        public string TipoSangre { get; set; }
        public string ColoniaDesc { get; set; }
        public string MunicipioDesc { get; set; }
        public string EstadoDesc { get; set; }
        public string PaisDesc { get; set; }
        public string CPDesc { get; set; }
        public bool TieneHijos { get; set; }
        public string NumeroHijosDesc { get; set; }
        public string EscolaridadDesc { get; set; }
        // Empleados does not seem to have LugarNacimiento directly recorded based on the schema, but we will pass what we have.

        public string NombreCompleto
        {
            get
            {
                return (Nombre + " " + APaterno + " " + AMaterno).Trim();
            }
        }
    }
}

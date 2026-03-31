using System;
using Telerik.Models.Entities;

namespace Telerik.Models.ViewModels
{
    public class EmpleadoVm
    {
        public int PkEmpleado { get; set; }
        public string Nombre { get; set; }
        public string APaterno { get; set; }
        public string AMaterno { get; set; }
        public string Nss { get; set; }
        public string Rfc { get; set; }
        public string Curp { get; set; }
        public string Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Sexo { get; set; }
        public string Edad { get; set; }

        // Puesto / Empresa / Proyecto
        public string PuestoDesc { get; set; }
        public string AreaDesc { get; set; }
        public string EmpresaDesc { get; set; }
        public string ProyectoDesc { get; set; }

        // Geographic FKs
        public int? FkPais { get; set; }
        public int? FkEstado { get; set; }
        public int? FkMunicipio { get; set; }
        public int? FkColonia { get; set; }
        public int? FkCP { get; set; }
        public string Calle { get; set; }
        public string NumExterior { get; set; }
        public string NumInterior { get; set; }

        // Geographic Descriptions
        public string PaisDesc { get; set; }
        public string EstadoDesc { get; set; }
        public string MunicipioDesc { get; set; }
        public string ColoniaDesc { get; set; }
        public string CPDesc { get; set; }

        // Otros
        public string EstadoCivil { get; set; }
        public string TipoSangre { get; set; }
        public bool TieneHijos { get; set; }
        public string NumeroHijosDesc { get; set; }
        public string EscolaridadDesc { get; set; }

        public string NombreCompleto
        {
            get
            {
                return (Nombre + " " + APaterno + " " + AMaterno).Trim();
            }
        }

        // Empleados does not seem to have LugarNacimiento directly recorded based on the schema, but we will pass what we have.
    }
}

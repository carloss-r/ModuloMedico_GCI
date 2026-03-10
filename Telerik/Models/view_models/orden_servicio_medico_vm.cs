using System;

namespace Telerik.Models.ViewModels
{
    public class OrdenServicioMedicoVm
    {
        public int PkOrdenMedico { get; set; }
        public int? FkEmpleado { get; set; }
        public int? FkCandidato { get; set; }
        public int? FkProyecto { get; set; }
        public int FkTipoServicio { get; set; }
        public int? FkEstatus { get; set; }
        public DateTime? FechaOrden { get; set; }

        // Display
        public string NombrePersona { get; set; }
        public string Modalidad { get; set; }
        public string TipoServicioDesc { get; set; }
        public string EstatusDesc { get; set; }
        public string ProyectoDesc { get; set; }
        public string EmpresaNombre { get; set; }   // Empresa padre del proyecto

        // Propiedad auxiliar interna para resolver EmpresaNombre en 2 pasos (evita JOIN encadenado nullable)
        public int? _FkEmpresa { get; set; }

        // Resultado Aptitud Medica
        public int? FkAptitudMedica { get; set; }
        public string AptitudDesc { 
            get {
                if (!FkAptitudMedica.HasValue) return "";
                switch (FkAptitudMedica.Value) {
                    case 1: return "APTO";
                    case 2: return "CON RESTRICCIONES";
                    case 3: return "NO APTO";
                    case 4: return "PENDIENTE";
                    default: return FkAptitudMedica.Value.ToString();
                }
            } 
        }

        // Candidate Specific
        public string PuestoCandidato { get; set; }
        public string AreaCandidato { get; set; }
        public string EmpresaCandidato { get; set; }
        public string SexoCandidato { get; set; }
        public string EdadCandidato { get; set; }

        public string FechaOrdenFormateada
        {
            get
            {
                return FechaOrden.HasValue ? FechaOrden.Value.ToString("dd/MM/yyyy") : "";
            }
        }

        public string FolioDisplay
        {
            get
            {
                return "SOL-SM-" + PkOrdenMedico.ToString("D4");
            }
        }
    }
}

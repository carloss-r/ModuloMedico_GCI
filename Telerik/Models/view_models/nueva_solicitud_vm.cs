namespace Telerik.Models.ViewModels
{
    public class NuevaSolicitudVm
    {
        public string Modalidad { get; set; } // "PERIODICO" o "INGRESO"
        public int FkTipoServicio { get; set; }
        public int? FkProyecto { get; set; }
        public int? FkEmpresa { get; set; }
        public int? FkPuesto { get; set; }

        // Periódico
        public int? NumeroEmpleado { get; set; }

        // Ingreso
        public string NombreCandidato { get; set; }
        public string ApellidoCandidato { get; set; }
        public string PuestoDeseado { get; set; }

        // Descripciones para el formato de impresión
        public string EmpresaDesc { get; set; }
        public string ProyectoDesc { get; set; }
        public string PuestoDesc { get; set; }
    }
}

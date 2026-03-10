using System;
using Telerik.Models.Entities;

namespace Telerik.Models.ViewModels
{
    public class AntidopingVm
    {
        public int PkOrdenMedico { get; set; }
        public string CodigoMuestra { get; set; }
        public bool ConsentimientoFirmado { get; set; }
        
        // Resultados (True = Positivo, False = Negativo)
        public bool ResultadoOpiaceos { get; set; }
        public bool ResultadoCocaina { get; set; }
        public bool ResultadoTHC { get; set; }
        public bool ResultadoMetanfetaminas { get; set; } // Added based on typical panels
        public bool ResultadoAnfetaminas { get; set; }    // Added based on typical panels
        
        public string VeredictoFinal { get; set; } // "APTO", "NO APTO"
        public string Comentarios { get; set; }
        public string UrlFotoEvidencia { get; set; }
        public System.Web.HttpPostedFileBase FileEvidencia { get; set; }
    }
}

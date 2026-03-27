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
        public bool ResultadoAlcohol { get; set; }
        public bool AplicaAlcohol { get; set; }
        public bool ResultadoCocaina { get; set; }
        public bool AplicaCocaina { get; set; }
        public bool ResultadoTHC { get; set; }
        public bool AplicaTHC { get; set; }
        public bool ResultadoAnfetaminas { get; set; }
        public bool AplicaAnfetaminas { get; set; }
        public bool ResultadoMetanfetaminas { get; set; }
        public bool AplicaMetanfetaminas { get; set; }
        public bool ResultadoOpiaceos { get; set; }
        public bool AplicaOpiaceos { get; set; }
        public bool ResultadoMetilfenidato { get; set; }
        public bool AplicaMetilfenidato { get; set; }
        public bool ResultadoFentanilo { get; set; }
        public bool AplicaFentanilo { get; set; }
        public bool ResultadoBenzodiacepinas { get; set; }
        public bool AplicaBenzodiacepinas { get; set; }
        
        public string VeredictoFinal { get; set; } // "APTO", "NO APTO"
        public string Comentarios { get; set; }
        public string UrlFotoEvidencia { get; set; }
        public System.Web.HttpPostedFileBase FileEvidencia { get; set; }
    }
}

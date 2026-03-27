using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models.Entities
{
    [Table("PruebasToxicologicas")]
    public class PruebaToxicologica
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("pkPrueba")]
        public int pkPruebaToxicologica { get; set; }

        public int fkOrdenMedico { get; set; }
        public string codigoMuestra { get; set; }
        public bool? consentimientoFirmado { get; set; }
        public bool? resultadoOpiaceos { get; set; }
        public bool? aplicaOpiaceos { get; set; }
        public bool? resultadoCocaina { get; set; }
        public bool? aplicaCocaina { get; set; }
        public bool? resultadoTHC { get; set; }
        public bool? aplicaTHC { get; set; }
        public bool? resultadoAlcohol { get; set; }
        public bool? aplicaAlcohol { get; set; }
        public bool? resultadoAnfetaminas { get; set; }
        public bool? aplicaAnfetaminas { get; set; }
        public bool? resultadoMetanfetaminas { get; set; }
        public bool? aplicaMetanfetaminas { get; set; }
        public bool? resultadoMetilfenidato { get; set; }
        public bool? aplicaMetilfenidato { get; set; }
        public bool? resultadoFentanilo { get; set; }
        public bool? aplicaFentanilo { get; set; }
        public bool? resultadoBenzodiacepinas { get; set; }
        public bool? aplicaBenzodiacepinas { get; set; }

        public string veredictoFinal { get; set; }
        public string comentarios { get; set; }
        public string urlFotoEvidencia { get; set; }
    }
}

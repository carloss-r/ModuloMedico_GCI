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
        public bool? resultadoCocaina { get; set; }
        public bool? resultadoTHC { get; set; }

        // NOTA: La BD tiene resultadoAlcohol (no anfetaminas/metanfetaminas)
        // Se eliminaron columnas inexistentes. Si se necesitan, agregar a la BD primero.
        public string veredictoFinal { get; set; }
        public string comentarios { get; set; }
        public string urlFotoEvidencia { get; set; }
    }
}

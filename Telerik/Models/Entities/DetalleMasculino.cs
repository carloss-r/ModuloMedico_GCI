using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models.Entities
{
    [Table("DetallesGenitourinariosMasc")]
    public class DetalleMasculino
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pkDetalleMasc { get; set; }

        public int fkEvaluacion { get; set; }
        public bool? prepucioRetractil { get; set; }
        public bool? testiculosDescendidos { get; set; }
        public bool? fimosis { get; set; }
        public bool? criptorquidia { get; set; }
        public bool? varicocele { get; set; }
        public bool? hidrocele { get; set; }
        public bool? hernia { get; set; }
        public string ivsa { get; set; }
        public string psa { get; set; }
        public string mpf { get; set; }
        public string edadesHijos { get; set; }

        [ForeignKey("fkEvaluacion")]
        public virtual EvaluacionClinica Evaluacion { get; set; }
    }
}

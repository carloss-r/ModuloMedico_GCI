using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models.Entities
{
    [Table("HistoriaMedica")]
    public class HistoriaMedica
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pkHistoria { get; set; }

        public int fkEvaluacion { get; set; }
        public string categoria { get; set; }
        public string nombreCondicion { get; set; }
        public bool? esPositivo { get; set; }
        public string detalles { get; set; }

        [ForeignKey("fkEvaluacion")]
        public virtual EvaluacionClinica Evaluacion { get; set; }
    }
}

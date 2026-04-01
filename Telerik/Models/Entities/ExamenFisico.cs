using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models.Entities
{
    [Table("OrdenExamenFisico")]
    public class OrdenExamenFisico
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pkOrdenExamenFisico { get; set; }

        public int fkEvaluacion { get; set; }
        public string sistemaCuerpo { get; set; }
        public bool esNormal { get; set; }
        public string hallazgos { get; set; }
        public string urlEvidencia { get; set; }

        [ForeignKey("fkEvaluacion")]
        public virtual EvaluacionClinica Evaluacion { get; set; }
    }
}

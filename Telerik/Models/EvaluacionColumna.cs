using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models
{
    [Table("EvaluacionColumna")]
    public class EvaluacionColumna
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pkEvaluacionColumna { get; set; }

        public int fkEvaluacion { get; set; }
        public byte? lordosisCervical { get; set; }
        public byte? lordosisDorsal { get; set; }
        public byte? lordosisLumbar { get; set; }
        public byte? cifosisCervical { get; set; }
        public byte? cifosisDorsal { get; set; }
        public byte? cifosisLumbar { get; set; }
        public bool? escoliosisDorsalDerecha { get; set; }
        public bool? escoliosisDorsalIzquierda { get; set; }
        public bool? escoliosisLumbarDerecha { get; set; }
        public bool? escoliosisLumbarIzquierda { get; set; }
        public bool? escoliosisDobleDerecha { get; set; }
        public bool? escoliosisDobleIzquierda { get; set; }
        public string observacionesColumna { get; set; }

        [ForeignKey("fkEvaluacion")]
        public virtual EvaluacionClinica Evaluacion { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models.Entities
{
    [Table("Vacunacion")]
    public class Vacunacion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pkVacuna { get; set; }

        public int fkEvaluacion { get; set; }
        public bool? tetanosDosis1 { get; set; }
        public bool? tetanosDosis2 { get; set; }
        public bool? tetanosDosis3 { get; set; }
        public bool? hepatitisDosis1 { get; set; }
        public bool? hepatitisDosis2 { get; set; }
        public bool? influenzaH1N1 { get; set; }
        public string observacionesVacunacion { get; set; }

        [ForeignKey("fkEvaluacion")]
        public virtual EvaluacionClinica Evaluacion { get; set; }
    }
}

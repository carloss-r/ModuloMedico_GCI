using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models.Entities
{
    [Table("AntecedentesLaborales")]
    public class AntecedenteLaboral
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pkLaboral { get; set; }

        public int fkEvaluacion { get; set; }
        public string empresa { get; set; }
        public string puesto { get; set; }
        public string tiempoLaborado { get; set; }
        public string agentesExpuestos { get; set; }
        public string accidentesPrevios { get; set; }

        [ForeignKey("fkEvaluacion")]
        public virtual EvaluacionClinica Evaluacion { get; set; }
    }
}

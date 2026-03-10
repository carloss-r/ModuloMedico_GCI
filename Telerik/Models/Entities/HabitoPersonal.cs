using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models.Entities
{
    [Table("HabitosPersonales")]
    public class HabitoPersonal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pkHabito { get; set; }

        public int fkEvaluacion { get; set; }
        public bool? fuma { get; set; }
        public int? anosFumando { get; set; }
        public int? cigarrosDiarios { get; set; }
        public bool? esExFumador { get; set; }
        public bool? bebeAlcohol { get; set; }
        public string frecuenciaAlcohol { get; set; }
        public bool? usaDrogas { get; set; }
        public string tipoDrogas { get; set; }
        public bool? haceDeporte { get; set; }
        public string descripcionTiempoLibre { get; set; }

        // Las vacunas están en HabitosPersonales directamente (no en tabla separada)
        public int? vacunaTetanos { get; set; }
        public int? vacunaHepatitis { get; set; }
        public bool? vacunaH1N1 { get; set; }

        [ForeignKey("fkEvaluacion")]
        public virtual EvaluacionClinica Evaluacion { get; set; }
    }
}

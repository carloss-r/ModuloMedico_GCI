using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models.Entities
{
    [Table("DetallesGinecoObstetricos")]
    public class DetalleGineco
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pkDetalleGineco { get; set; }

        public int fkEvaluacion { get; set; }
        public int? edadMenarca { get; set; }
        public DateTime? fechaUltimaMenstruacion { get; set; }
        public string ciclos { get; set; }
        public int? gestas { get; set; }
        public int? partos { get; set; }
        public int? abortos { get; set; }
        public int? cesareas { get; set; }
        public int? ivsa { get; set; }
        public string metodoPlanificacion { get; set; }
        public DateTime? fechaUltimoPapanicolau { get; set; }
        public string ets { get; set; }
        public string edadesHijos { get; set; }
        public string numeroHijosEdades { get; set; } // columna adicional en DATA_GCI

        [ForeignKey("fkEvaluacion")]
        public virtual EvaluacionClinica Evaluacion { get; set; }
    }
}

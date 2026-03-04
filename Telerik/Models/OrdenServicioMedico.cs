using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models
{
    [Table("OrdenServicioMedico")]
    public class OrdenServicioMedico
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pkOrdenMedico { get; set; }

        public int? fkEmpleado { get; set; }
        public int? fkCandidato { get; set; }
        public int? fkProyecto { get; set; }
        public int fkTipoServicio { get; set; }
        public int? fkEstatus { get; set; }
        public DateTime? fechaOrden { get; set; }

        // Propiedades de navegación
        [ForeignKey("fkEmpleado")]
        public virtual Empleado Empleado { get; set; }

        [ForeignKey("fkCandidato")]
        public virtual Candidato Candidato { get; set; }

        [ForeignKey("fkProyecto")]
        public virtual Proyecto Proyecto { get; set; }

        [ForeignKey("fkTipoServicio")]
        public virtual TipoServicio TipoServicio { get; set; }

        [ForeignKey("fkEstatus")]
        public virtual EstatusSolicitud Estatus { get; set; }
    }
}

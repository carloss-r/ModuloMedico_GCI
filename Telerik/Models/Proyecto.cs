using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models
{
    [Table("Proyectos")]
    public class Proyecto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pkProyecto { get; set; }

        public string descripcion { get; set; }
        public int fkEmpresa { get; set; }
        public int fkEstatusProyecto { get; set; }
        public string clave { get; set; }

        [ForeignKey("fkEmpresa")]
        public virtual Empresa Empresa { get; set; }
    }
}

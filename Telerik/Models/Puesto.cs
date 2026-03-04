using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models
{
    [Table("Puesto")]
    public class Puesto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pkPuesto { get; set; }

        public string descripcion { get; set; }
        public int fkEmpresa { get; set; }
        public int? fkArea { get; set; }

        [ForeignKey("fkEmpresa")]
        public virtual Empresa Empresa { get; set; }

        [ForeignKey("fkArea")]
        public virtual Area Area { get; set; }
    }
}

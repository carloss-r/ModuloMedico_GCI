using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models.Entities
{
    [Table("EstadoCivil")]
    public class EstadoCivil
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pkEstadoCivil { get; set; }
        
        [StringLength(50)]
        public string descripcion { get; set; }
        
        public int? fkParentesco { get; set; }
    }
}

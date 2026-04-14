using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models.Entities
{
    [Table("TipoSangre")]
    public class TipoSangre
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pkTipoSangre { get; set; }
        
        [StringLength(15)]
        public string descripcion { get; set; }
    }
}

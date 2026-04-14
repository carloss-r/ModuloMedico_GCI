using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models.Entities
{
    [Table("NivelEscolaridad")]
    public class NivelEscolaridad
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pkNivelEscolaridad { get; set; }
        
        [StringLength(50)]
        public string descripcion { get; set; }
    }
}

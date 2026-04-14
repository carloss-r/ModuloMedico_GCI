using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models.Entities
{
    [Table("Sexo")]
    public class Sexo
    {
        [Key]
        [StringLength(10)]
        public string pkSexo { get; set; }
        
        public string descripcion { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models.Entities
{
    [Table("Profesiones")]
    public class Profesion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pkProfesion { get; set; }
        
        [StringLength(250)]
        public string descripcion { get; set; }
    }
}

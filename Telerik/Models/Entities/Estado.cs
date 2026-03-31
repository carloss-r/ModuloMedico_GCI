using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models.Entities
{
    [Table("Estado")]
    public class Estado
    {
        [Key]
        public int pkEstado { get; set; }
        public string descripcion { get; set; }
        public int fkPais { get; set; }
    }
}

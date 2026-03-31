using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models.Entities
{
    [Table("Pais")]
    public class Pais
    {
        [Key]
        public int pkPais { get; set; }
        public string descripcion { get; set; }
    }
}

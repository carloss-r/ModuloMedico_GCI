using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models.Entities
{
    [Table("CP")]
    public class CodigoPostal
    {
        [Key]
        public int pkCP { get; set; }
        public string descripcion { get; set; }
        public int? fkColonia { get; set; }
    }
}

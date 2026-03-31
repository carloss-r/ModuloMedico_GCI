using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models.Entities
{
    [Table("Municipio")]
    public class Municipio
    {
        [Key]
        public int pkMunicipio { get; set; }
        public string descripcion { get; set; }
        public int fkEstado { get; set; }
    }
}

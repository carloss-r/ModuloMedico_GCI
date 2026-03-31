using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models.Entities
{
    [Table("Colonia")]
    public class Colonia
    {
        [Key]
        public int pkColonia { get; set; }
        public string descripcion { get; set; }
        public int fkMunicipio { get; set; }
    }
}

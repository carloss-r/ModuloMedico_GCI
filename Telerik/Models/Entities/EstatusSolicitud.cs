using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models.Entities
{
    [Table("EstatusSolicitud")]
    public class EstatusSolicitud
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pkEstatus { get; set; }

        public string descripcion { get; set; }
    }
}

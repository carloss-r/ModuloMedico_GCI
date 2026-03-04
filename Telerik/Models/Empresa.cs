using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models
{
    [Table("Empresas")]
    public class Empresa
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pkEmpresa { get; set; }

        public string nombre { get; set; }
        public string descCorta { get; set; }
    }
}

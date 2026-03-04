using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models
{
    [Table("Candidatos")]
    public class Candidato
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pkCandidato { get; set; }

        public string nombre { get; set; }
        public string aPaterno { get; set; }
        public string aMaterno { get; set; }
        public DateTime? fechaNacimiento { get; set; }
        public string fkSexo { get; set; }
        public string puestoDeseado { get; set; }
        public DateTime? fechaRegistro { get; set; }
        public string area { get; set; }
        public string empresa { get; set; }
        public string telefono { get; set; }
        public string email { get; set; }
        public string curp { get; set; }
        public string rfc { get; set; }
        public string nss { get; set; }
        public string manoDominante { get; set; }
        public bool? tieneHijos { get; set; }
        public int? fkNumeroHijos { get; set; }
        public int? fkEstadoCivil { get; set; }
        public int? fkTipoSangre { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models
{
    [Table("Empleados")]
    public class Empleado
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pkEmpleado { get; set; }
        
        public string nombre { get; set; }
        public string aPaterno { get; set; }
        public string aMaterno { get; set; }
        
        public string rfc { get; set; }
        public string curp { get; set; }
        public string numeroSeguroSocial { get; set; }
        public string edad { get; set; }
        public DateTime? fechaNacimiento { get; set; }
        public string fkSexo { get; set; }

        public int? fkPuesto { get; set; }
        public int? fkProyecto { get; set; }
        public int? fkEmpresa { get; set; }
        public int? fkEstadoCivil { get; set; }
        public int? fkTipoSangre { get; set; }
        
        public int? fkColonia { get; set; }
        public int? fkMunicipio { get; set; }
        public int? fkEstado { get; set; }
        public int? fkPais { get; set; }
        public int? fkCP { get; set; }
        public int? fkNumeroHijos { get; set; }
        
        public string telefono { get; set; }
        public string calle { get; set; }
        public string numExterior { get; set; }
        public string numInterior { get; set; }
        public bool? tieneHijos { get; set; }
        // Las llaves foráneas se resuelven aquí o directo en ViewModel como strings si se usa raw linq.
    }
}

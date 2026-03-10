using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerik.Models.Entities
{
    [Table("EvaluacionesClinicas")]
    public class EvaluacionClinica
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pkEvaluacion { get; set; }

        public int fkOrdenMedico { get; set; }
        public DateTime? fechaEvaluacion { get; set; }
        public string nombreMedico { get; set; }
        public decimal? pesoKg { get; set; }
        public decimal? alturaMetros { get; set; }
        public decimal? imc { get; set; }
        public int? presionSistolica { get; set; }
        public int? presionDiastolica { get; set; }
        public decimal? temperatura { get; set; }
        public int? frecuenciaCardiaca { get; set; }
        public int? frecuenciaRespiratoria { get; set; }
        public string aparatosSistemas { get; set; }
        public int? fkAptitudMedica { get; set; }
        public string observaciones { get; set; }
        public string sintomasPaciente { get; set; }
        public string nss { get; set; }
        public DateTime? fechaNacimiento { get; set; }
        public string lugarNacimiento { get; set; }
        public string estadoCivil { get; set; }
        public string manoDominante { get; set; }
        public string telefono { get; set; }
        public string domicilio { get; set; }
        public string escolaridad { get; set; }
        public string profesion { get; set; }
        public string alergias { get; set; }
        public string tipoSangre { get; set; }
        public string recomendaciones { get; set; } // columna presente en DATA_GCI.sql

        // Propiedades de navegación (uno a uno)
        public virtual HabitoPersonal Habitos { get; set; }
        public virtual EvaluacionColumna Columna { get; set; }
        public virtual DetalleGineco DetalleGineco { get; set; }
        public virtual DetalleMasculino DetalleMasculino { get; set; }

        // Propiedades de navegación (uno a muchos)
        public virtual ICollection<HistoriaMedica> HistoriaMedica { get; set; }
        public virtual ICollection<AntecedenteLaboral> AntecedentesLaborales { get; set; }
        public virtual ICollection<ExamenFisico> ExamenFisico { get; set; }
    }
}

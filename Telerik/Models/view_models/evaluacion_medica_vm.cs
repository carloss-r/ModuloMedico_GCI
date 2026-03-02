using System;
using System.Collections.Generic;

namespace Telerik.Models.ViewModels
{
    public class EvaluacionMedicaVm
    {
        public int PkOrdenMedico { get; set; }
        
        // Signos Vitales y Somatometría
        public decimal? PesoKg { get; set; }
        public decimal? AlturaMetros { get; set; }
        public decimal? Imc { get; set; }
        public int? PresionSistolica { get; set; }
        public int? PresionDiastolica { get; set; }
        public decimal? Temperatura { get; set; }
        public int? FrecuenciaCardiaca { get; set; }
        public int? FrecuenciaRespiratoria { get; set; }
        public string AparatosSistemas { get; set; }
        
        public int? FkAptitudMedica { get; set; }
        public string Observaciones { get; set; } // Map as Diagnosis
        public string Recomendaciones { get; set; } // Added for Recommendation step
        public string SintomasPaciente { get; set; }

        // New Demographic Fields
        public string Nss { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string LugarNacimiento { get; set; }
        public string EstadoCivil { get; set; }
        public string ManoDominante { get; set; }
        public string Telefono { get; set; }
        public string Domicilio { get; set; }
        public string Escolaridad { get; set; }
        public string Profesion { get; set; }
        public string Alergias { get; set; }
        public string TipoSangre { get; set; }

        // Secciones (Listas o Objetos anidados)
        public HabitosPersonalesVm Habitos { get; set; }
        public List<HistoriaMedicaVm> Antecedentes { get; set; }
        public List<ExamenFisicoVm> ExamenFisico { get; set; }
        public List<AntecedenteLaboralVm> AntecedentesLaborales { get; set; }
        public EvaluacionColumnaVm Columna { get; set; }
        
        // Específicos por sexo (solo uno vendrá lleno)
        public DetalleGinecoVm DetalleFemenino { get; set; }
        public DetalleGenitoMascVm DetalleMasculino { get; set; }

        // Optional: Candidate Info Update
        public string NombreCandidato { get; set; }
        public string PuestoCandidato { get; set; }
        public string AreaCandidato { get; set; }
        public string EmpresaCandidato { get; set; }
        public string SexoCandidato { get; set; }
    }

    public class HabitosPersonalesVm
    {
        public bool Fuma { get; set; }
        public int? AnosFumando { get; set; }
        public int? CigarrosDiarios { get; set; }
        public bool EsExFumador { get; set; }
        public bool BebeAlcohol { get; set; }
        public string FrecuenciaAlcohol { get; set; }
        public bool UsaDrogas { get; set; }
        public string TipoDrogas { get; set; }
        public bool HaceDeporte { get; set; }
        public string DescripcionTiempoLibre { get; set; }
        public int? VacunaTetanos { get; set; }
        public int? VacunaHepatitis { get; set; }
        public bool VacunaH1N1 { get; set; }
    }

    public class HistoriaMedicaVm
    {
        public string Categoria { get; set; } // "Patologico", "Quirurgico", etc.
        public string NombreCondicion { get; set; }
        public bool EsPositivo { get; set; }
        public string Detalles { get; set; }
    }

    public class AntecedenteLaboralVm
    {
        public string Empresa { get; set; }
        public string Puesto { get; set; }
        public string TiempoLaborado { get; set; }
        public string AgentesExpuesto { get; set; }
        public string AccidentesPrevios { get; set; }
    }

    public class ExamenFisicoVm
    {
        public string SistemaCuerpo { get; set; } // "Cabeza", "Ojos", etc.
        public bool EsNormal { get; set; }
        public string Hallazgos { get; set; }
    }

    public class EvaluacionColumnaVm
    {
        public int? LordosisCervical { get; set; }
        public int? LordosisDorsal { get; set; }
        public int? LordosisLumbar { get; set; }
        public int? CifosisCervical { get; set; }
        public int? CifosisDorsal { get; set; }
        public int? CifosisLumbar { get; set; }
        public bool EscoliosisDorsalDerecha { get; set; }
        public bool EscoliosisDorsalIzquierda { get; set; }
        public bool EscoliosisLumbarDerecha { get; set; }
        public bool EscoliosisLumbarIzquierda { get; set; }
        public bool EscoliosisDoboDerecha { get; set; } 
        public bool EscoliosisDoboIzquierda { get; set; }
        public string ObservacionesColumna { get; set; }
    }

    public class DetalleGinecoVm
    {
        public int? EdadMenarca { get; set; }
        public DateTime? FechaUltimaMenstruacion { get; set; }
        public string Ciclos { get; set; }
        public int? Gestas { get; set; }
        public int? Partos { get; set; }
        public int? Abortos { get; set; }
        public int? Cesareas { get; set; }
        public int? Ivsa { get; set; } // Inicio vida sexual activa (edad)
        public string MetodoPlanificacion { get; set; }
        public DateTime? FechaUltimoPapanicolau { get; set; }
        public string Ets { get; set; }
        public string NumeroHijosEdades { get; set; }
    }

    public class DetalleGenitoMascVm
    {
        public bool PrepucioRetractil { get; set; }
        public bool TesticulosDescendidos { get; set; }
        public bool Fimosis { get; set; }
        public bool Criptorquidia { get; set; }
        public bool Varicocele { get; set; }
        public bool Hidrocele { get; set; }
        public bool Hernia { get; set; }
        public string Ivsa { get; set; } // string in DB
        public string Psa { get; set; } // Antigeno Prostatico
    }
}

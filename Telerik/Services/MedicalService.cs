using System;
using System.Collections.Generic;
using System.Linq;
using Telerik.Models;
using Telerik.Models.Entities;
using Telerik.Models.ViewModels;
using Telerik.Models.DAL;

namespace Telerik.Services
{
    public class MedicalService
    {
        public string NormalizarSexo(string sexo)
        {
            if (string.IsNullOrEmpty(sexo)) return null; 
            string s = sexo.Trim().ToUpper();
            if (s == "HOMBRE" || s == "H" || s == "1" || s == "MASCULINO" || s == "MASC" || s == "M") return "M";
            if (s == "MUJER" || s == "2" || s == "FEMENINO" || s == "FEM" || s == "F") return "F";
            return null; 
        }

        public PacienteInfoVm ObtenerInfoPaciente(OrdenServicioMedicoVm orden)
        {
            Candidato cand = null;
            if (orden.FkCandidato.HasValue)
            {
                using (var db = new ApplicationDbContext())
                {
                    cand = db.Candidatos.Find(orden.FkCandidato.Value);
                }
            }

            if (orden.FkEmpleado.HasValue)
            {
                var emp = EmpleadoDal.BuscarPorNumero(orden.FkEmpleado.Value);
                if (emp != null)
                {
                    string edad = emp.Edad;
                    if (emp.FechaNacimiento.HasValue)
                    {
                         var hoy = DateTime.Today;
                         var nacimiento = emp.FechaNacimiento.Value;
                         var edadCalc = hoy.Year - nacimiento.Year;
                         if (nacimiento.Date > hoy.AddYears(-edadCalc)) edadCalc--;
                         edad = edadCalc.ToString();
                    }

                    return new PacienteInfoVm
                    {
                        Nombre         = emp.Nombre,
                        ApellidoPaterno = emp.APaterno,
                        ApellidoMaterno = emp.AMaterno,
                        NombreCompleto = emp.NombreCompleto,
                        Edad = edad,
                        Puesto = emp.PuestoDesc,
                        Area = emp.AreaDesc,
                        Empresa = emp.ProyectoDesc, // Project for employees
                        Sexo = NormalizarSexo(emp.Sexo),
                        Tipo = "EMPLEADO",  
                        TipoServicioId = orden.FkTipoServicio,
                        TipoServicioDesc = orden.TipoServicioDesc,
                        NumeroEmpleado = emp.PkEmpleado.ToString(),
                        
                        FechaNacimiento = emp.FechaNacimiento.HasValue ? emp.FechaNacimiento.Value.ToString("yyyy-MM-dd") : "",
                        Nss = emp.Nss,
                        Telefono = emp.Telefono,
                        Direccion = string.Join(", ", new string[] {
                            (!string.IsNullOrEmpty(emp.Calle) ? emp.Calle + " " + emp.NumExterior + (string.IsNullOrEmpty(emp.NumInterior) ? "" : " " + emp.NumInterior) : ""),
                            emp.ColoniaDesc,
                            emp.MunicipioDesc,
                            emp.EstadoDesc,
                            emp.PaisDesc,
                            (!string.IsNullOrEmpty(emp.CPDesc) ? "CP: " + emp.CPDesc : "")
                        }.Where(s => !string.IsNullOrEmpty(s))).Trim(),
                        EstadoCivil = emp.EstadoCivil,
                        TipoSangre = emp.TipoSangre,
                        Rfc = emp.Rfc,
                        Curp = "", // Retired as not needed in UI
                        TieneHijos = emp.TieneHijos,
                        NumeroHijos = emp.NumeroHijosDesc,
                        Escolaridad = emp.EscolaridadDesc,
                        // Geographic FKs
                        FkPais = emp.FkPais,
                        FkEstado = emp.FkEstado,
                        FkMunicipio = emp.FkMunicipio,
                        FkColonia = emp.FkColonia,
                        FkCP = emp.FkCP,
                        CPDesc = emp.CPDesc,
                        Calle = emp.Calle,
                        NumExterior = emp.NumExterior,
                        NumInterior = emp.NumInterior,
                        FkEmpresa = emp.FkPais.HasValue ? null : (int?)null // placeholder
                    };
                }
            }
            
            return new PacienteInfoVm
            {
                Nombre         = cand != null ? cand.nombre : null,
                ApellidoPaterno = cand != null ? cand.aPaterno : null,
                ApellidoMaterno = cand != null ? cand.aMaterno : null,
                NombreCompleto = (cand != null ? (cand.nombre + " " + cand.aPaterno + " " + cand.aMaterno) : (orden.NombrePersona ?? "")).Trim(),
                Edad = (cand != null && cand.fechaNacimiento.HasValue) ? 
                       (DateTime.Today.Year - cand.fechaNacimiento.Value.Year - (DateTime.Today < cand.fechaNacimiento.Value.AddYears(DateTime.Today.Year - cand.fechaNacimiento.Value.Year) ? 1 : 0)).ToString() : 
                       (orden.EdadCandidato ?? ""),
                Puesto = (cand != null ? cand.puestoDeseado : orden.PuestoCandidato) ?? "",
                Area = (cand != null ? cand.area : orden.AreaCandidato) ?? "",
                Empresa = cand != null ? cand.empresa : (!string.IsNullOrEmpty(orden.EmpresaCandidato) ? orden.EmpresaCandidato : (orden.ProyectoDesc ?? "")),
                Sexo = NormalizarSexo(cand != null ? cand.fkSexo : orden.SexoCandidato),
                Tipo = "CANDIDATO",
                TipoServicioId = orden.FkTipoServicio,
                TipoServicioDesc = orden.TipoServicioDesc,
                NumeroEmpleado = "N/A",

                // New fields for synchronization
                FechaNacimiento = cand != null && cand.fechaNacimiento.HasValue ? cand.fechaNacimiento.Value.ToString("yyyy-MM-dd") : "",
                Nss = cand != null ? cand.nss : "",
                Curp = "", // Retired as not needed in UI
                Telefono = cand != null ? cand.telefono : "",
                EstadoCivil = (cand != null && cand.fkEstadoCivil.HasValue) ? cand.fkEstadoCivil.Value.ToString() : "", 
                FkTipoSangre = cand != null ? cand.fkTipoSangre : null,
                Escolaridad = cand != null ? cand.escolaridad : "",
                
                // Address data
                FkPais = cand != null ? cand.fkPais : null,
                FkEstado = cand != null ? cand.fkEstado : null,
                FkMunicipio = cand != null ? cand.fkMunicipio : null,
                FkColonia = cand != null ? cand.fkColonia : null,
                FkCP = cand != null ? cand.fkCP : null,
                Calle = cand != null ? cand.calle : "",
                NumExterior = cand != null ? cand.numExterior : "",
                NumInterior = cand != null ? cand.numInterior : "",
                CPDesc = (cand != null && cand.fkCP.HasValue) ? cand.fkCP.Value.ToString() : ""
            };
        }

        public void GuardarEvaluacion(EvaluacionMedicaVm model)
        {
            EvaluacionDal.GuardarEvaluacion(model);
        }

        public string GenerarNombreArchivoAntidoping(int pkOrden, string extension)
        {
            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return string.Format("ANTI-SOL-SM-{0:D4}_{1}{2}", pkOrden, stamp, extension);
        }

        public void GuardarAntidoping(AntidopingVm model)
        {
            AntidopingDal.GuardarAntidoping(model);
        }

        public void CompletarOrden(int pkOrden)
        {
            OrdenServicioMedicoDal.ActualizarEstatus(pkOrden, 3); // 3 = Completada
        }
    }
}

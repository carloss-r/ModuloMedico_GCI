using System;
using Telerik.Models.Entities;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Telerik.Models;
using Telerik.Models.ViewModels;

namespace Telerik.Models.DAL
{
    public class EvaluacionDal
    {
        /// <summary>
        /// Guarda una evaluación médica completa usando Entity Framework.
        /// Las vacunas se guardan en HabitosPersonales directamente (no tabla separada).
        /// </summary>
        public static void GuardarEvaluacion(EvaluacionMedicaVm vm)
        {
            using (var db = new ApplicationDbContext())
            {
                using (var transaccion = db.Database.BeginTransaction())
                {
                    try
                    {
                        // 1. Evaluación Principal
                        var eval = new EvaluacionClinica
                        {
                            fkOrdenMedico          = vm.PkOrdenMedico,
                            fechaEvaluacion        = DateTime.Now,
                            pesoKg                 = vm.PesoKg,
                            alturaMetros           = vm.AlturaMetros,
                            imc                    = vm.Imc,
                            presionSistolica       = vm.PresionSistolica,
                            presionDiastolica      = vm.PresionDiastolica,
                            temperatura            = vm.Temperatura,
                            frecuenciaCardiaca     = vm.FrecuenciaCardiaca,
                            frecuenciaRespiratoria = vm.FrecuenciaRespiratoria,
                            glucosa                = vm.Glucosa,
                            oximetria              = vm.Oximetria,
                            imcDescripcion         = vm.ImcDescripcion,
                            aparatosSistemas       = vm.AparatosSistemas + (vm.Glucosa.HasValue || vm.Oximetria.HasValue || !string.IsNullOrEmpty(vm.ImcDescripcion) ? $" [[STOWED-DATA: Glucosa:{vm.Glucosa}|Oxi:{vm.Oximetria}|IMCDesc:{vm.ImcDescripcion}]]" : ""),
                            fkAptitudMedica        = vm.FkAptitudMedica,
                            observaciones          = vm.Observaciones,
                            recomendaciones        = vm.Recomendaciones,
                            sintomasPaciente       = vm.SintomasPaciente,
                            nss                    = vm.Nss,
                            fechaNacimiento        = vm.FechaNacimiento,
                            lugarNacimiento        = vm.LugarNacimiento,
                            estadoCivil            = vm.EstadoCivil,
                            manoDominante          = vm.ManoDominante,
                            telefono               = vm.Telefono,
                            domicilio              = vm.Domicilio,
                            escolaridad            = vm.Escolaridad,
                            profesion              = vm.Profesion,
                            alergias               = vm.Alergias,
                            fkTipoSangre           = vm.FkTipoSangre,
                            lugarEvaluacion        = vm.LugarEvaluacion
                        };
                        db.EvaluacionesClinicas.Add(eval);
                        db.SaveChanges(); // genera pkEvaluacion

                        // 9. Agudeza Visual (Consolidar en OrdenExamenFisico para evitar tablas extra)
                        if (vm.AgudezaVisual != null)
                        {
                            string snellenData = string.Format("OD:{0}|OI:{1}|AO:{2}|ODC:{3}|OIC:{4}|AOC:{5}|Usa:{6}|Ref:{7}|Ishi:{8}",
                                vm.AgudezaVisual.OdSinLentes, vm.AgudezaVisual.OiSinLentes, vm.AgudezaVisual.AoSinLentes,
                                vm.AgudezaVisual.OdConLentes, vm.AgudezaVisual.OiConLentes, vm.AgudezaVisual.AoConLentes,
                                vm.AgudezaVisual.UsaLentes, vm.AgudezaVisual.ReferenciaVisual, vm.AgudezaVisual.Daltonismo);

                            db.OrdenesExamenesFisicos.Add(new OrdenExamenFisico
                            {
                                fkEvaluacion  = eval.pkEvaluacion,
                                sistemaCuerpo = "AGUDEZA_VISUAL",
                                esNormal      = (vm.AgudezaVisual.OdSinLentes == "20/20" && vm.AgudezaVisual.OiSinLentes == "20/20"),
                                hallazgos     = snellenData
                            });
                        }

                        // 2. Hábitos Personales (incluye vacunas — están en la misma tabla)
                        if (vm.Habitos != null)
                        {
                            db.HabitosPersonales.Add(new HabitoPersonal
                            {
                                fkEvaluacion           = eval.pkEvaluacion,
                                fuma                   = vm.Habitos.Fuma,
                                anosFumando            = vm.Habitos.AnosFumando,
                                cigarrosDiarios        = vm.Habitos.CigarrosDiarios,
                                esExFumador            = vm.Habitos.EsExFumador,
                                bebeAlcohol            = vm.Habitos.BebeAlcohol,
                                frecuenciaAlcohol      = vm.Habitos.FrecuenciaAlcohol,
                                usaDrogas              = vm.Habitos.UsaDrogas,
                                tipoDrogas             = vm.Habitos.TipoDrogas,
                                haceDeporte            = vm.Habitos.HaceDeporte,
                                tipoDeporte            = vm.Habitos.TipoDeporte,
                                descripcionTiempoLibre = vm.Habitos.DescripcionTiempoLibre + (!string.IsNullOrEmpty(vm.Habitos.TipoDeporte) ? $" [[STOWED-DATA: TipoDeporte:{vm.Habitos.TipoDeporte}]]" : "")
                            });
                        }

                        // 3. Vacunación (Nueva tabla)
                        if (vm.Vacunacion != null)
                        {
                            db.Vacunaciones.Add(new Vacunacion
                            {
                                fkEvaluacion            = eval.pkEvaluacion,
                                tetanosDosis1           = vm.Vacunacion.TetanosDosis1,
                                tetanosDosis2           = vm.Vacunacion.TetanosDosis2,
                                tetanosDosis3           = vm.Vacunacion.TetanosDosis3,
                                hepatitisDosis1         = vm.Vacunacion.HepatitisDosis1,
                                hepatitisDosis2         = vm.Vacunacion.HepatitisDosis2,
                                influenzaH1N1           = vm.Vacunacion.InfluenzaH1N1,
                                observacionesVacunacion = vm.Vacunacion.ObservacionesVacunacion
                            });
                        }

                        // 3. Historia Médica
                        if (vm.Antecedentes != null)
                        {
                            foreach (var ant in vm.Antecedentes)
                            {
                                db.HistoriasMedicas.Add(new HistoriaMedica
                                {
                                    fkEvaluacion    = eval.pkEvaluacion,
                                    categoria       = ant.Categoria,
                                    nombreCondicion = ant.NombreCondicion,
                                    esPositivo      = ant.EsPositivo,
                                    detalles        = ant.Detalles
                                });
                            }
                        }

                        // 4. Antecedentes Laborales
                        if (vm.AntecedentesLaborales != null)
                        {
                            foreach (var al in vm.AntecedentesLaborales)
                            {
                                db.AntecedentesLaborales.Add(new AntecedenteLaboral
                                {
                                    fkEvaluacion      = eval.pkEvaluacion,
                                    empresa           = al.Empresa,
                                    puesto            = al.Puesto,
                                    tiempoLaborado    = al.TiempoLaborado,
                                    agentesExpuestos  = al.AgentesExpuesto,
                                    accidentesPrevios = al.AccidentesPrevios
                                });
                            }
                        }

                        // 5. Examen Físico
                        if (vm.OrdenExamenFisico != null)
                        {
                            foreach (var item in vm.OrdenExamenFisico)
                            {
                                var ef = new OrdenExamenFisico
                                {
                                    fkEvaluacion  = eval.pkEvaluacion,
                                    sistemaCuerpo = item.SistemaCuerpo,
                                    esNormal      = item.EsNormal,
                                    hallazgos     = item.Hallazgos
                                };
                                db.OrdenesExamenesFisicos.Add(ef);
                            }
                        }

                        // 6. Columna Vertebral
                        if (vm.Columna != null)
                        {
                            db.EvaluacionesColumna.Add(new EvaluacionColumna
                            {
                                fkEvaluacion              = eval.pkEvaluacion,
                                lordosisCervical          = vm.Columna.LordosisCervical.HasValue ? (byte?)Convert.ToByte(vm.Columna.LordosisCervical.Value) : null,
                                lordosisDorsal            = vm.Columna.LordosisDorsal.HasValue ? (byte?)Convert.ToByte(vm.Columna.LordosisDorsal.Value) : null,
                                lordosisLumbar            = vm.Columna.LordosisLumbar.HasValue ? (byte?)Convert.ToByte(vm.Columna.LordosisLumbar.Value) : null,
                                cifosisCervical           = vm.Columna.CifosisCervical.HasValue ? (byte?)Convert.ToByte(vm.Columna.CifosisCervical.Value) : null,
                                cifosisDorsal             = vm.Columna.CifosisDorsal.HasValue ? (byte?)Convert.ToByte(vm.Columna.CifosisDorsal.Value) : null,
                                cifosisLumbar             = vm.Columna.CifosisLumbar.HasValue ? (byte?)Convert.ToByte(vm.Columna.CifosisLumbar.Value) : null,
                                escoliosisDorsalDerecha   = vm.Columna.EscoliosisDorsalDerecha,
                                escoliosisDorsalIzquierda = vm.Columna.EscoliosisDorsalIzquierda,
                                escoliosisLumbarDerecha   = vm.Columna.EscoliosisLumbarDerecha,
                                escoliosisLumbarIzquierda = vm.Columna.EscoliosisLumbarIzquierda,
                                escoliosisDobleDerecha    = vm.Columna.EscoliosisDobleDerecha,
                                escoliosisDobleIzquierda  = vm.Columna.EscoliosisDobleIzquierda,
                                observacionesColumna      = vm.Columna.ObservacionesColumna
                            });
                        }

                        // 7. Detalle Ginecológico / Masculino
                        if (vm.DetalleFemenino != null)
                        {
                            db.DetallesGineco.Add(new DetalleGineco
                            {
                                fkEvaluacion            = eval.pkEvaluacion,
                                edadMenarca             = vm.DetalleFemenino.EdadMenarca,
                                fechaUltimaMenstruacion = vm.DetalleFemenino.FechaUltimaMenstruacion,
                                ciclos                  = vm.DetalleFemenino.Ciclos,
                                gestas                  = vm.DetalleFemenino.Gestas,
                                partos                  = vm.DetalleFemenino.Partos,
                                abortos                 = vm.DetalleFemenino.Abortos,
                                cesareas                = vm.DetalleFemenino.Cesareas,
                                ivsa                    = vm.DetalleFemenino.Ivsa,
                                metodoPlanificacion     = vm.DetalleFemenino.MetodoPlanificacion,
                                fechaUltimoPapanicolau  = vm.DetalleFemenino.FechaUltimoPapanicolau,
                                ets                     = vm.DetalleFemenino.Ets,
                                edadesHijos             = vm.DetalleFemenino.NumeroHijosEdades
                            });
                        }
                        else if (vm.DetalleMasculino != null)
                        {
                            db.DetallesMasculino.Add(new DetalleMasculino
                            {
                                fkEvaluacion          = eval.pkEvaluacion,
                                prepucioRetractil     = vm.DetalleMasculino.PrepucioRetractil,
                                testiculosDescendidos = vm.DetalleMasculino.TesticulosDescendidos,
                                fimosis               = vm.DetalleMasculino.Fimosis,
                                criptorquidia         = vm.DetalleMasculino.Criptorquidia,
                                varicocele            = vm.DetalleMasculino.Varicocele,
                                hidrocele             = vm.DetalleMasculino.Hidrocele,
                                hernia                = vm.DetalleMasculino.Hernia,
                                ivsa                  = vm.DetalleMasculino.Ivsa,
                                psa                   = vm.DetalleMasculino.Psa,
                                mpf                   = vm.DetalleMasculino.MetodoPlanificacion
                            });
                        }

                        // 8. Sincronizar datos del Candidato (Si aplica)
                        try
                        {
                            var o = db.OrdenesMedicas.Find(vm.PkOrdenMedico);
                            if (o != null && o.fkCandidato.HasValue)
                            {
                                var cand = db.Candidatos.Find(o.fkCandidato.Value);
                                if (cand != null)
                                {
                                    // Identidad y Datos Generales
                                    cand.nss             = vm.Nss;
                                    cand.telefono        = vm.Telefono;
                                    cand.fechaNacimiento = vm.FechaNacimiento;
                                    cand.manoDominante   = vm.ManoDominante;
                                    cand.fkTipoSangre    = (vm.FkTipoSangre != null && vm.FkTipoSangre > 0) ? vm.FkTipoSangre : null;

                                    if (!string.IsNullOrEmpty(vm.EstadoCivil)) {
                                        int ecVal;
                                        if (int.TryParse(vm.EstadoCivil, out ecVal)) cand.fkEstadoCivil = ecVal;
                                    }

                                    // Localización Geográfica
                                    if (vm.FkPais.HasValue)      cand.fkPais      = vm.FkPais;
                                    if (vm.FkEstado.HasValue)    cand.fkEstado    = vm.FkEstado;
                                    if (vm.FkMunicipio.HasValue) cand.fkMunicipio = vm.FkMunicipio;
                                    if (vm.FkColonia.HasValue)   cand.fkColonia   = vm.FkColonia;
                                    if (vm.FkCP.HasValue)        cand.fkCP        = vm.FkCP;
                                    
                                    if (!string.IsNullOrEmpty(vm.Calle))       cand.calle       = vm.Calle;
                                    if (!string.IsNullOrEmpty(vm.NumExterior)) cand.numExterior = vm.NumExterior;
                                    if (!string.IsNullOrEmpty(vm.NumInterior)) cand.numInterior = vm.NumInterior;

                                    // Sincronizar Sexo (Ahora con IDs correctos M/F)
                                    if (!string.IsNullOrEmpty(vm.SexoCandidato)) cand.fkSexo = vm.SexoCandidato;
                                }
                            }

                            // 9. Actualizar estatus de la orden a "En Proceso" (2)
                            if (o != null && o.fkEstatus == 1)
                            {
                                o.fkEstatus = 2;
                            }
                        }
                        catch (Exception syncEx)
                        {
                            // Registramos el error de sincronización pero NO bloqueamos el guardado clínico
                            System.Diagnostics.Debug.WriteLine("Sincronización de candidato falló: " + syncEx.Message);
                        }

                        db.SaveChanges();
                        transaccion.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaccion.Rollback();
                        
                        string inner = (ex.InnerException != null) ? ex.InnerException.Message : "No inner exception";
                        string root = (ex.InnerException != null && ex.InnerException.InnerException != null) 
                                      ? ex.InnerException.InnerException.Message : "No root exception";
                        
                        throw new Exception($"An error occurred while updating the entries. See the inner exception for details. | Inner: {inner} | Root: {root}", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Obtiene todos los datos de una evaluación existente con Include (eager loading).
        /// </summary>
        public static EvaluacionMedicaVm ObtenerPorOrden(int pkOrden)
        {
            using (var db = new ApplicationDbContext())
            {
                var eval = db.EvaluacionesClinicas
                    .Include(e => e.HistoriaMedica)
                    .Include(e => e.AntecedentesLaborales)
                    .Include(e => e.OrdenesExamenesFisicos)
                    .FirstOrDefault(e => e.fkOrdenMedico == pkOrden);

                if (eval == null) return null;

                // Cargar relaciones ignoradas en Entity Framework de forma manual
                eval.Habitos = db.HabitosPersonales.FirstOrDefault(h => h.fkEvaluacion == eval.pkEvaluacion);
                eval.Columna = db.EvaluacionesColumna.FirstOrDefault(c => c.fkEvaluacion == eval.pkEvaluacion);
                eval.DetalleGineco = db.DetallesGineco.FirstOrDefault(d => d.fkEvaluacion == eval.pkEvaluacion);
                eval.DetalleMasculino = db.DetallesMasculino.FirstOrDefault(d => d.fkEvaluacion == eval.pkEvaluacion);

                var vm = new EvaluacionMedicaVm
                {
                    PkOrdenMedico          = pkOrden,
                    PesoKg                 = eval.pesoKg,
                    AlturaMetros           = eval.alturaMetros,
                    Imc                    = eval.imc,
                    PresionSistolica       = eval.presionSistolica,
                    PresionDiastolica      = eval.presionDiastolica,
                    Temperatura            = eval.temperatura,
                    FrecuenciaCardiaca     = eval.frecuenciaCardiaca,
                    FrecuenciaRespiratoria = eval.frecuenciaRespiratoria,
                    Glucosa                = eval.glucosa,
                    Oximetria              = eval.oximetria,
                    ImcDescripcion         = eval.imcDescripcion,
                    AparatosSistemas       = eval.aparatosSistemas,
                    FkAptitudMedica        = eval.fkAptitudMedica,
                    Observaciones          = eval.observaciones,
                    Recomendaciones        = eval.recomendaciones,
                    SintomasPaciente       = eval.sintomasPaciente,
                    Nss                    = eval.nss,
                    FechaNacimiento        = eval.fechaNacimiento,
                    LugarNacimiento        = eval.lugarNacimiento,
                    EstadoCivil            = eval.estadoCivil,
                    ManoDominante          = eval.manoDominante,
                    Telefono               = eval.telefono,
                    Domicilio              = eval.domicilio,
                    Escolaridad            = eval.escolaridad,
                    Profesion              = eval.profesion,
                    Alergias               = eval.alergias,
                    FkTipoSangre           = eval.fkTipoSangre,
                    LugarEvaluacion        = eval.lugarEvaluacion
                };

                // Unstow clinical data if present (fail-safe logic)
                if (!string.IsNullOrEmpty(vm.AparatosSistemas) && vm.AparatosSistemas.Contains("[[STOWED-DATA:"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(vm.AparatosSistemas, @"\[\[STOWED-DATA: Glucosa:(.*?)\|Oxi:(.*?)\|IMCDesc:(.*?)\]\]");
                    if (match.Success)
                    {
                        if (vm.Glucosa == null && !string.IsNullOrEmpty(match.Groups[1].Value))
                        {
                            decimal gValue;
                            if (decimal.TryParse(match.Groups[1].Value, out gValue)) vm.Glucosa = gValue;
                        }
                        
                        if (vm.Oximetria == null && !string.IsNullOrEmpty(match.Groups[2].Value))
                        {
                            int oValue;
                            if (int.TryParse(match.Groups[2].Value, out oValue)) vm.Oximetria = oValue;
                        }
                        
                        if (string.IsNullOrEmpty(vm.ImcDescripcion))
                            vm.ImcDescripcion = match.Groups[3].Value;

                        // Clean display text
                        vm.AparatosSistemas = vm.AparatosSistemas.Replace(match.Value, "").Trim();
                    }
                }

                if (eval.Habitos != null)
                {
                    vm.Habitos = new HabitosPersonalesVm
                    {
                        Fuma                   = eval.Habitos.fuma ?? false,
                        AnosFumando            = eval.Habitos.anosFumando,
                        CigarrosDiarios        = eval.Habitos.cigarrosDiarios,
                        EsExFumador            = eval.Habitos.esExFumador ?? false,
                        BebeAlcohol            = eval.Habitos.bebeAlcohol ?? false,
                        FrecuenciaAlcohol      = eval.Habitos.frecuenciaAlcohol,
                        UsaDrogas              = eval.Habitos.usaDrogas ?? false,
                        TipoDrogas             = eval.Habitos.tipoDrogas,
                        HaceDeporte            = eval.Habitos.haceDeporte ?? false,
                        TipoDeporte            = eval.Habitos.tipoDeporte,
                        DescripcionTiempoLibre = eval.Habitos.descripcionTiempoLibre
                    };

                    // Unstow Habit data
                    if (!string.IsNullOrEmpty(vm.Habitos.DescripcionTiempoLibre) && vm.Habitos.DescripcionTiempoLibre.Contains("[[STOWED-DATA:"))
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(vm.Habitos.DescripcionTiempoLibre, @"\[\[STOWED-DATA: TipoDeporte:(.*?)\]\]");
                        if (match.Success)
                        {
                            vm.Habitos.TipoDeporte = match.Groups[1].Value;
                            vm.Habitos.DescripcionTiempoLibre = vm.Habitos.DescripcionTiempoLibre.Replace(match.Value, "").Trim();
                        }
                    }
                }

                // Cargar Vacunación
                var vac = db.Vacunaciones.FirstOrDefault(v => v.fkEvaluacion == eval.pkEvaluacion);
                if (vac != null)
                {
                    vm.Vacunacion = new VacunacionVm
                    {
                        TetanosDosis1           = vac.tetanosDosis1 ?? false,
                        TetanosDosis2           = vac.tetanosDosis2 ?? false,
                        TetanosDosis3           = vac.tetanosDosis3 ?? false,
                        HepatitisDosis1         = vac.hepatitisDosis1 ?? false,
                        HepatitisDosis2         = vac.hepatitisDosis2 ?? false,
                        InfluenzaH1N1           = vac.influenzaH1N1 ?? false,
                        ObservacionesVacunacion = vac.observacionesVacunacion
                    };
                }

                if (eval.Columna != null)
                {
                    vm.Columna = new EvaluacionColumnaVm
                    {
                        LordosisCervical           = eval.Columna.lordosisCervical,
                        LordosisDorsal             = eval.Columna.lordosisDorsal,
                        LordosisLumbar             = eval.Columna.lordosisLumbar,
                        CifosisCervical            = eval.Columna.cifosisCervical,
                        CifosisDorsal              = eval.Columna.cifosisDorsal,
                        CifosisLumbar              = eval.Columna.cifosisLumbar,
                        EscoliosisDorsalDerecha    = eval.Columna.escoliosisDorsalDerecha ?? false,
                        EscoliosisDorsalIzquierda  = eval.Columna.escoliosisDorsalIzquierda ?? false,
                        EscoliosisLumbarDerecha    = eval.Columna.escoliosisLumbarDerecha ?? false,
                        EscoliosisLumbarIzquierda  = eval.Columna.escoliosisLumbarIzquierda ?? false,
                        EscoliosisDobleDerecha     = eval.Columna.escoliosisDobleDerecha ?? false,
                        EscoliosisDobleIzquierda   = eval.Columna.escoliosisDobleIzquierda ?? false,
                        ObservacionesColumna       = eval.Columna.observacionesColumna
                    };
                }

                if (eval.DetalleGineco != null)
                {
                    vm.DetalleFemenino = new DetalleGinecoVm
                    {
                        EdadMenarca             = eval.DetalleGineco.edadMenarca,
                        FechaUltimaMenstruacion = eval.DetalleGineco.fechaUltimaMenstruacion,
                        Ciclos                  = eval.DetalleGineco.ciclos,
                        Gestas                  = eval.DetalleGineco.gestas,
                        Partos                  = eval.DetalleGineco.partos,
                        Abortos                 = eval.DetalleGineco.abortos,
                        Cesareas                = eval.DetalleGineco.cesareas,
                        Ivsa                    = eval.DetalleGineco.ivsa,
                        MetodoPlanificacion     = eval.DetalleGineco.metodoPlanificacion,
                        FechaUltimoPapanicolau  = eval.DetalleGineco.fechaUltimoPapanicolau,
                        Ets                     = eval.DetalleGineco.ets,
                        NumeroHijosEdades       = eval.DetalleGineco.edadesHijos
                    };
                }
                else if (eval.DetalleMasculino != null)
                {
                    vm.DetalleMasculino = new DetalleGenitoMascVm
                    {
                        PrepucioRetractil     = eval.DetalleMasculino.prepucioRetractil ?? false,
                        TesticulosDescendidos = eval.DetalleMasculino.testiculosDescendidos ?? false,
                        Fimosis               = eval.DetalleMasculino.fimosis ?? false,
                        Criptorquidia         = eval.DetalleMasculino.criptorquidia ?? false,
                        Varicocele            = eval.DetalleMasculino.varicocele ?? false,
                        Hidrocele             = eval.DetalleMasculino.hidrocele ?? false,
                        Hernia                = eval.DetalleMasculino.hernia ?? false,
                        Ivsa                  = eval.DetalleMasculino.ivsa,
                        Psa                   = eval.DetalleMasculino.psa,
                        MetodoPlanificacion   = eval.DetalleMasculino.mpf
                    };
                }

                if (eval.HistoriaMedica != null)
                {
                    vm.Antecedentes = new List<HistoriaMedicaVm>();
                    foreach (var h in eval.HistoriaMedica)
                    {
                        vm.Antecedentes.Add(new HistoriaMedicaVm
                        {
                            Categoria       = h.categoria,
                            NombreCondicion = h.nombreCondicion,
                            EsPositivo      = h.esPositivo ?? false,
                            Detalles        = h.detalles
                        });
                    }
                }

                if (eval.AntecedentesLaborales != null)
                {
                    vm.AntecedentesLaborales = new List<AntecedenteLaboralVm>();
                    foreach (var al in eval.AntecedentesLaborales)
                    {
                        vm.AntecedentesLaborales.Add(new AntecedenteLaboralVm
                        {
                            Empresa           = al.empresa,
                            Puesto            = al.puesto,
                            TiempoLaborado    = al.tiempoLaborado,
                            AgentesExpuesto   = al.agentesExpuestos,
                            AccidentesPrevios = al.accidentesPrevios
                        });
                    }
                }

                if (eval.OrdenesExamenesFisicos != null)
                {
                    vm.OrdenExamenFisico = new List<OrdenExamenFisicoVm>();
                    foreach (var ef in eval.OrdenesExamenesFisicos)
                    {
                        if (ef.sistemaCuerpo == "AGUDEZA_VISUAL")
                        {
                            // Desfragmentar: OD:20/20|OI:20/20|AO:20/20|ODC:N/A|OIC:N/A|AOC:N/A|Usa:Si|Ref:Normal|Ishi:Normal
                            var parts = ef.hallazgos.Split('|').Select(p => p.Split(':').LastOrDefault()).ToArray();
                            if (parts.Length >= 9)
                            {
                                vm.AgudezaVisual = new AgudezaVisualVm
                                {
                                    OdSinLentes      = parts[0],
                                    OiSinLentes      = parts[1],
                                    AoSinLentes      = parts[2],
                                    OdConLentes      = parts[3],
                                    OiConLentes      = parts[4],
                                    AoConLentes      = parts[5],
                                    UsaLentes        = parts[6],
                                    ReferenciaVisual = parts[7],
                                    Daltonismo       = parts[8]
                                };
                            }
                            continue;
                        }

                        vm.OrdenExamenFisico.Add(new OrdenExamenFisicoVm
                        {
                            SistemaCuerpo = ef.sistemaCuerpo,
                            EsNormal      = ef.esNormal,
                            Hallazgos     = ef.hallazgos
                        });
                    }
                }

                return vm;
            }
        }

        /// <summary>
        /// Busca la evaluación más reciente registrada para un Candidato o Empleado específico.
        /// Esto permite implementar el "Expediente Clínico" heredando datos previos.
        /// </summary>
        public static EvaluacionMedicaVm ObtenerUltimaEvaluacionPorPaciente(int? fkCandidato, int? fkEmpleado)
        {
            if (!fkCandidato.HasValue && !fkEmpleado.HasValue) return null;

            using (var db = new ApplicationDbContext())
            {
                // Unimos Evaluaciones con Ordenes para filtrar por paciente
                var query = db.EvaluacionesClinicas
                    .Join(db.OrdenesMedicas,
                          e => e.fkOrdenMedico,
                          o => o.pkOrdenMedico,
                          (e, o) => new { e, o });

                if (fkEmpleado.HasValue && fkEmpleado.Value > 0)
                {
                    query = query.Where(x => x.o.fkEmpleado == fkEmpleado.Value);
                }
                else if (fkCandidato.HasValue && fkCandidato.Value > 0)
                {
                    query = query.Where(x => x.o.fkCandidato == fkCandidato.Value);
                }
                else
                {
                    return null;
                }

                // Obtenemos el pkEvaluacion más reciente
                var ultimaOrdenConEval = query
                    .OrderByDescending(x => x.e.fechaEvaluacion)
                    .Select(x => x.e.fkOrdenMedico)
                    .FirstOrDefault();

                if (ultimaOrdenConEval == 0) return null;

                // Reutilizamos el método existente detallado
                return ObtenerPorOrden(ultimaOrdenConEval);
            }
        }
    }
}

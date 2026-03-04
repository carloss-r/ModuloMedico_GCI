using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Telerik.Models;
using Telerik.Models.ViewModels;

namespace Telerik.Models.Dal
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
                            aparatosSistemas       = vm.AparatosSistemas,
                            fkAptitudMedica        = vm.FkAptitudMedica,
                            observaciones          = vm.Observaciones,
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
                            tipoSangre             = vm.TipoSangre
                        };
                        db.EvaluacionesClinicas.Add(eval);
                        db.SaveChanges(); // genera pkEvaluacion

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
                                descripcionTiempoLibre = vm.Habitos.DescripcionTiempoLibre,
                                vacunaTetanos          = vm.Habitos.VacunaTetanos,
                                vacunaHepatitis        = vm.Habitos.VacunaHepatitis,
                                vacunaH1N1             = vm.Habitos.VacunaH1N1
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
                        if (vm.ExamenFisico != null)
                        {
                            foreach (var ef in vm.ExamenFisico)
                            {
                                db.ExamenesFisicos.Add(new ExamenFisico
                                {
                                    fkEvaluacion  = eval.pkEvaluacion,
                                    sistemaCuerpo = ef.SistemaCuerpo,
                                    esNormal      = ef.EsNormal,
                                    hallazgos     = ef.Hallazgos
                                });
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
                                escoliosisDobleDerecha    = vm.Columna.EscoliosisDoboDerecha,
                                escoliosisDobleIzquierda  = vm.Columna.EscoliosisDoboIzquierda,
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
                                psa                   = vm.DetalleMasculino.Psa
                            });
                        }

                        // 8. Actualizar estatus de la orden a "En Proceso" (2)
                        var orden = db.OrdenesMedicas.Find(vm.PkOrdenMedico);
                        if (orden != null && orden.fkEstatus == 1)
                        {
                            orden.fkEstatus = 2;
                        }

                        db.SaveChanges();
                        transaccion.Commit();
                    }
                    catch (Exception)
                    {
                        transaccion.Rollback();
                        throw;
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
                    .Include(e => e.ExamenFisico)
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
                    AparatosSistemas       = eval.aparatosSistemas,
                    FkAptitudMedica        = eval.fkAptitudMedica,
                    Observaciones          = eval.observaciones,
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
                    TipoSangre             = eval.tipoSangre
                };

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
                        DescripcionTiempoLibre = eval.Habitos.descripcionTiempoLibre,
                        VacunaTetanos          = eval.Habitos.vacunaTetanos,
                        VacunaHepatitis        = eval.Habitos.vacunaHepatitis,
                        VacunaH1N1             = eval.Habitos.vacunaH1N1 ?? false
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
                        EscoliosisDoboDerecha      = eval.Columna.escoliosisDobleDerecha ?? false,
                        EscoliosisDoboIzquierda    = eval.Columna.escoliosisDobleIzquierda ?? false,
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
                        Psa                   = eval.DetalleMasculino.psa
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

                if (eval.ExamenFisico != null)
                {
                    vm.ExamenFisico = new List<ExamenFisicoVm>();
                    foreach (var ef in eval.ExamenFisico)
                    {
                        vm.ExamenFisico.Add(new ExamenFisicoVm
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
    }
}

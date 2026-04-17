using System;
using Telerik.Models.Entities;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Telerik.Models;
using Telerik.Models.ViewModels;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace Telerik.Models.DAL
{
    public class EvaluacionDal
    {
        public static void GuardarEvaluacion(EvaluacionMedicaVm vm)
        {
            string connStr = ConfigurationManager.ConnectionStrings["GCI_ModuloMedico"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        string sqlCheck = "SELECT pkEvaluacion FROM EvaluacionesClinicas WHERE fkOrdenMedico = @id";
                        SqlCommand cmd = new SqlCommand(sqlCheck, conn, trans);
                        cmd.Parameters.AddWithValue("@id", vm.PkOrdenMedico);
                        object evalIdObj = cmd.ExecuteScalar();
                        int pkEval;

                        string sqlEval;
                        var p = new List<SqlParameter> {
                            new SqlParameter("@id", vm.PkOrdenMedico),
                            new SqlParameter("@peso", (object)vm.PesoKg ?? DBNull.Value),
                            new SqlParameter("@alt", (object)vm.AlturaMetros ?? DBNull.Value),
                            new SqlParameter("@imc", (object)vm.Imc ?? DBNull.Value),
                            new SqlParameter("@sis", (object)vm.PresionSistolica ?? DBNull.Value),
                            new SqlParameter("@dia", (object)vm.PresionDiastolica ?? DBNull.Value),
                            new SqlParameter("@temp", (object)vm.Temperatura ?? DBNull.Value),
                            new SqlParameter("@fc", (object)vm.FrecuenciaCardiaca ?? DBNull.Value),
                            new SqlParameter("@fr", (object)vm.FrecuenciaRespiratoria ?? DBNull.Value),
                            new SqlParameter("@glu", (object)vm.Glucosa ?? DBNull.Value),
                            new SqlParameter("@oxi", (object)vm.Oximetria ?? DBNull.Value),
                            new SqlParameter("@imcd", (object)vm.ImcDescripcion ?? (object)DBNull.Value),
                            new SqlParameter("@aps", (object)vm.AparatosSistemas ?? (object)DBNull.Value),
                            new SqlParameter("@apt", (object)vm.FkAptitudMedica ?? DBNull.Value),
                            new SqlParameter("@obs", (object)vm.Observaciones ?? (object)DBNull.Value),
                            new SqlParameter("@rec", (object)vm.Recomendaciones ?? (object)DBNull.Value),
                            new SqlParameter("@sin", (object)vm.SintomasPaciente ?? (object)DBNull.Value),
                            new SqlParameter("@nss", (object)vm.Nss ?? (object)DBNull.Value),
                            new SqlParameter("@fn", (object)vm.FechaNacimiento ?? (object)DBNull.Value),
                            new SqlParameter("@ln", (object)vm.LugarNacimiento ?? (object)DBNull.Value),
                            new SqlParameter("@ec", (object)vm.EstadoCivil ?? (object)DBNull.Value),
                            new SqlParameter("@md", (object)vm.ManoDominante ?? (object)DBNull.Value),
                            new SqlParameter("@tel", (object)vm.Telefono ?? (object)DBNull.Value),
                            new SqlParameter("@dom", (object)vm.Domicilio ?? (object)DBNull.Value),
                            new SqlParameter("@esc", (object)vm.Escolaridad ?? (object)DBNull.Value),
                            new SqlParameter("@pro", (object)vm.Profesion ?? (object)DBNull.Value),
                            new SqlParameter("@ale", (object)vm.Alergias ?? (object)DBNull.Value),
                            new SqlParameter("@ts", (object)vm.FkTipoSangre ?? DBNull.Value),
                            new SqlParameter("@lug", (object)vm.LugarEvaluacion ?? (object)DBNull.Value)
                        };

                        if (evalIdObj == null)
                        {
                            sqlEval = @"INSERT INTO EvaluacionesClinicas (fkOrdenMedico, fechaEvaluacion, pesoKg, alturaMetros, imc, presionSistolica, presionDiastolica, temperatura, frecuenciaCardiaca, frecuenciaRespiratoria, glucosa, oximetria, imcDescripcion, aparatosSistemas, fkAptitudMedica, observaciones, recomendaciones, sintomasPaciente, nss, fechaNacimiento, lugarNacimiento, estadoCivil, manoDominante, telefono, domicilio, escolaridad, profesion, alergias, fkTipoSangre, lugarEvaluacion)
                                        VALUES (@id, GETDATE(), @peso, @alt, @imc, @sis, @dia, @temp, @fc, @fr, @glu, @oxi, @imcd, @aps, @apt, @obs, @rec, @sin, @nss, @fn, @ln, @ec, @md, @tel, @dom, @esc, @pro, @ale, @ts, @lug);
                                        SELECT SCOPE_IDENTITY();";
                            cmd.CommandText = sqlEval;
                            cmd.Parameters.Clear(); cmd.Parameters.AddRange(p.ToArray());
                            pkEval = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                        else
                        {
                            pkEval = (int)evalIdObj;
                            sqlEval = @"UPDATE EvaluacionesClinicas SET pesoKg=@peso, alturaMetros=@alt, imc=@imc, presionSistolica=@sis, presionDiastolica=@dia, temperatura=@temp, frecuenciaCardiaca=@fc, frecuenciaRespiratoria=@fr, glucosa=@glu, oximetria=@oxi, imcDescripcion=@imcd, aparatosSistemas=@aps, fkAptitudMedica=@apt, observaciones=@obs, recomendaciones=@rec, sintomasPaciente=@sin, nss=@nss, fechaNacimiento=@fn, lugarNacimiento=@ln, estadoCivil=@ec, manoDominante=@md, telefono=@tel, domicilio=@dom, escolaridad=@esc, profesion=@pro, alergias=@ale, fkTipoSangre=@ts, lugarEvaluacion=@lug
                                        WHERE pkEvaluacion = @pk;
                                        DELETE FROM HistoriaMedica WHERE fkEvaluacion=@pk;
                                        DELETE FROM AntecedentesLaborales WHERE fkEvaluacion=@pk;
                                        DELETE FROM OrdenExamenFisico WHERE fkEvaluacion=@pk;
                                        DELETE FROM HabitosPersonales WHERE fkEvaluacion=@pk;
                                        DELETE FROM Vacunacion WHERE fkEvaluacion=@pk;
                                        DELETE FROM EvaluacionColumna WHERE fkEvaluacion=@pk;
                                        DELETE FROM DetallesGinecoObstetricos WHERE fkEvaluacion=@pk;
                                        DELETE FROM DetallesGenitourinariosMasc WHERE fkEvaluacion=@pk;";
                            cmd.CommandText = sqlEval;
                            cmd.Parameters.Clear(); cmd.Parameters.AddRange(p.ToArray()); cmd.Parameters.AddWithValue("@pk", pkEval);
                            cmd.ExecuteNonQuery();
                        }

                        if (vm.AgudezaVisual != null) {
                            string snellenData = $"OD:{vm.AgudezaVisual.OdSinLentes}|OI:{vm.AgudezaVisual.OiSinLentes}|AO:{vm.AgudezaVisual.AoSinLentes}|ODC:{vm.AgudezaVisual.OdConLentes}|OIC:{vm.AgudezaVisual.OiConLentes}|AOC:{vm.AgudezaVisual.AoConLentes}|Usa:{vm.AgudezaVisual.UsaLentes}|Ref:{vm.AgudezaVisual.ReferenciaVisual}|Ishi:{vm.AgudezaVisual.Daltonismo}";
                            cmd.CommandText = "INSERT INTO OrdenExamenFisico (fkEvaluacion, sistemaCuerpo, esNormal, hallazgos) VALUES (@pk, 'AGUDEZA_VISUAL', 0, @h)";
                            cmd.Parameters.Clear(); cmd.Parameters.AddWithValue("@pk", pkEval); cmd.Parameters.AddWithValue("@h", snellenData);
                            cmd.ExecuteNonQuery();
                        }

                        if (vm.Habitos != null) {
                            cmd.CommandText = "INSERT INTO HabitosPersonales (fkEvaluacion, fuma, anosFumando, cigarrosDiarios, esExFumador, bebeAlcohol, frecuenciaAlcohol, usaDrogas, tipoDrogas, haceDeporte, tipoDeporte, descripcionTiempoLibre) VALUES (@pk, @f, @af, @cd, @ef, @ba, @fa, @ud, @td, @hd, @dp, @tl)";
                            cmd.Parameters.Clear(); cmd.Parameters.AddWithValue("@pk", pkEval); cmd.Parameters.AddWithValue("@f", vm.Habitos.Fuma); cmd.Parameters.AddWithValue("@af", (object)vm.Habitos.AnosFumando ?? DBNull.Value); cmd.Parameters.AddWithValue("@cd", (object)vm.Habitos.CigarrosDiarios ?? DBNull.Value); cmd.Parameters.AddWithValue("@ef", vm.Habitos.EsExFumador); cmd.Parameters.AddWithValue("@ba", vm.Habitos.BebeAlcohol); cmd.Parameters.AddWithValue("@fa", (object)vm.Habitos.FrecuenciaAlcohol ?? DBNull.Value); cmd.Parameters.AddWithValue("@ud", vm.Habitos.UsaDrogas); cmd.Parameters.AddWithValue("@td", (object)vm.Habitos.TipoDrogas ?? DBNull.Value); cmd.Parameters.AddWithValue("@hd", vm.Habitos.HaceDeporte); cmd.Parameters.AddWithValue("@dp", (object)vm.Habitos.TipoDeporte ?? DBNull.Value); cmd.Parameters.AddWithValue("@tl", (object)vm.Habitos.DescripcionTiempoLibre ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        if (vm.Vacunacion != null) {
                            cmd.CommandText = "INSERT INTO Vacunacion (fkEvaluacion, tetanosDosis1, tetanosDosis2, tetanosDosis3, hepatitisDosis1, hepatitisDosis2, influenzaH1N1, observacionesVacunacion) VALUES (@pk, @t1, @t2, @t3, @h1, @h2, @i1, @obv)";
                            cmd.Parameters.Clear(); cmd.Parameters.AddWithValue("@pk", pkEval); cmd.Parameters.AddWithValue("@t1", vm.Vacunacion.TetanosDosis1); cmd.Parameters.AddWithValue("@t2", vm.Vacunacion.TetanosDosis2); cmd.Parameters.AddWithValue("@t3", vm.Vacunacion.TetanosDosis3); cmd.Parameters.AddWithValue("@h1", vm.Vacunacion.HepatitisDosis1); cmd.Parameters.AddWithValue("@h2", vm.Vacunacion.HepatitisDosis2); cmd.Parameters.AddWithValue("@i1", vm.Vacunacion.InfluenzaH1N1); cmd.Parameters.AddWithValue("@obv", (object)vm.Vacunacion.ObservacionesVacunacion ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        if (vm.Antecedentes != null) foreach(var a in vm.Antecedentes) {
                            cmd.CommandText = "INSERT INTO HistoriaMedica (fkEvaluacion, categoria, nombreCondicion, esPositivo, detalles) VALUES (@pk, @c, @n, @p, @d)";
                            cmd.Parameters.Clear(); cmd.Parameters.AddWithValue("@pk", pkEval); cmd.Parameters.AddWithValue("@c", a.Categoria); cmd.Parameters.AddWithValue("@n", a.NombreCondicion); cmd.Parameters.AddWithValue("@p", a.EsPositivo); cmd.Parameters.AddWithValue("@d", (object)a.Detalles ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }
                        if (vm.AntecedentesLaborales != null) foreach(var l in vm.AntecedentesLaborales) {
                            cmd.CommandText = "INSERT INTO AntecedentesLaborales (fkEvaluacion, empresa, puesto, tiempoLaborado, agentesExpuestos, accidentesPrevios) VALUES (@pk, @e, @p, @t, @a, @ap)";
                            cmd.Parameters.Clear(); cmd.Parameters.AddWithValue("@pk", pkEval); cmd.Parameters.AddWithValue("@e", l.Empresa); cmd.Parameters.AddWithValue("@p", l.Puesto); cmd.Parameters.AddWithValue("@t", l.TiempoLaborado); cmd.Parameters.AddWithValue("@a", l.AgentesExpuesto); cmd.Parameters.AddWithValue("@ap", l.AccidentesPrevios);
                            cmd.ExecuteNonQuery();
                        }
                        if (vm.OrdenExamenFisico != null) foreach(var f in vm.OrdenExamenFisico) {
                            cmd.CommandText = "INSERT INTO OrdenExamenFisico (fkEvaluacion, sistemaCuerpo, esNormal, hallazgos) VALUES (@pk, @sy, @n, @h)";
                            cmd.Parameters.Clear(); cmd.Parameters.AddWithValue("@pk", pkEval); cmd.Parameters.AddWithValue("@sy", f.SistemaCuerpo); cmd.Parameters.AddWithValue("@n", f.EsNormal); cmd.Parameters.AddWithValue("@h", (object)f.Hallazgos ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        if (vm.Columna != null) {
                            cmd.CommandText = "INSERT INTO EvaluacionColumna (fkEvaluacion, lordosisCervical, lordosisDorsal, lordosisLumbar, cifosisCervical, cifosisDorsal, cifosisLumbar, escoliosisDorsalDerecha, escoliosisDorsalIzquierda, escoliosisLumbarDerecha, escoliosisLumbarIzquierda, escoliosisDobleDerecha, escoliosisDobleIzquierda, observacionesColumna) VALUES (@pk, @lc, @ld, @ll, @cc, @cd, @cl, @edd, @edi, @eld, @eli, @edbd, @edbi, @obs)";
                            cmd.Parameters.Clear(); cmd.Parameters.AddWithValue("@pk", pkEval); cmd.Parameters.AddWithValue("@lc", (object)vm.Columna.LordosisCervical ?? DBNull.Value); cmd.Parameters.AddWithValue("@ld", (object)vm.Columna.LordosisDorsal ?? DBNull.Value); cmd.Parameters.AddWithValue("@ll", (object)vm.Columna.LordosisLumbar ?? DBNull.Value); cmd.Parameters.AddWithValue("@cc", (object)vm.Columna.CifosisCervical ?? DBNull.Value); cmd.Parameters.AddWithValue("@cd", (object)vm.Columna.CifosisDorsal ?? DBNull.Value); cmd.Parameters.AddWithValue("@cl", (object)vm.Columna.CifosisLumbar ?? DBNull.Value); cmd.Parameters.AddWithValue("@edd", vm.Columna.EscoliosisDorsalDerecha); cmd.Parameters.AddWithValue("@edi", vm.Columna.EscoliosisDorsalIzquierda); cmd.Parameters.AddWithValue("@eld", vm.Columna.EscoliosisLumbarDerecha); cmd.Parameters.AddWithValue("@eli", vm.Columna.EscoliosisLumbarIzquierda); cmd.Parameters.AddWithValue("@edbd", vm.Columna.EscoliosisDobleDerecha); cmd.Parameters.AddWithValue("@edbi", vm.Columna.EscoliosisDobleIzquierda); cmd.Parameters.AddWithValue("@obs", (object)vm.Columna.ObservacionesColumna ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        if (vm.DetalleFemenino != null) {
                            cmd.CommandText = "INSERT INTO DetallesGinecoObstetricos (fkEvaluacion, edadMenarca, fechaUltimaMenstruacion, ciclos, gestas, partos, abortos, cesareas, metodoPlanificacion, fechaUltimoPapanicolau, edadesHijos) VALUES (@pk, @em, @fm, @ci, @ge, @pa, @ab, @ce, @mp, @fp, @eh)";
                            cmd.Parameters.Clear(); cmd.Parameters.AddWithValue("@pk", pkEval); cmd.Parameters.AddWithValue("@em", (object)vm.DetalleFemenino.EdadMenarca ?? DBNull.Value); cmd.Parameters.AddWithValue("@fm", (object)vm.DetalleFemenino.FechaUltimaMenstruacion ?? DBNull.Value); cmd.Parameters.AddWithValue("@ci", (object)vm.DetalleFemenino.Ciclos ?? DBNull.Value); cmd.Parameters.AddWithValue("@ge", (object)vm.DetalleFemenino.Gestas ?? DBNull.Value); cmd.Parameters.AddWithValue("@pa", (object)vm.DetalleFemenino.Partos ?? DBNull.Value); cmd.Parameters.AddWithValue("@ab", (object)vm.DetalleFemenino.Abortos ?? DBNull.Value); cmd.Parameters.AddWithValue("@ce", (object)vm.DetalleFemenino.Cesareas ?? DBNull.Value); cmd.Parameters.AddWithValue("@mp", (object)vm.DetalleFemenino.MetodoPlanificacion ?? DBNull.Value); cmd.Parameters.AddWithValue("@fp", (object)vm.DetalleFemenino.FechaUltimoPapanicolau ?? DBNull.Value); cmd.Parameters.AddWithValue("@eh", (object)vm.DetalleFemenino.NumeroHijosEdades ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        } else if (vm.DetalleMasculino != null) {
                            cmd.CommandText = "INSERT INTO DetallesGenitourinariosMasc (fkEvaluacion, prepucioRetractil, testiculosDescendidos, fimosis, criptorquidia, varicocele, hidrocele, hernia, psa, mpf) VALUES (@pk, @pr, @td, @fi, @cr, @va, @hi, @he, @psa, @mpf)";
                            cmd.Parameters.Clear(); cmd.Parameters.AddWithValue("@pk", pkEval); cmd.Parameters.AddWithValue("@pr", vm.DetalleMasculino.PrepucioRetractil); cmd.Parameters.AddWithValue("@td", vm.DetalleMasculino.TesticulosDescendidos); cmd.Parameters.AddWithValue("@fi", vm.DetalleMasculino.Fimosis); cmd.Parameters.AddWithValue("@cr", vm.DetalleMasculino.Criptorquidia); cmd.Parameters.AddWithValue("@va", vm.DetalleMasculino.Varicocele); cmd.Parameters.AddWithValue("@hi", vm.DetalleMasculino.Hidrocele); cmd.Parameters.AddWithValue("@he", vm.DetalleMasculino.Hernia); cmd.Parameters.AddWithValue("@psa", (object)vm.DetalleMasculino.Psa ?? DBNull.Value); cmd.Parameters.AddWithValue("@mpf", (object)vm.DetalleMasculino.MetodoPlanificacion ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        cmd.CommandText = "SELECT fkCandidato, fkEstatus FROM OrdenServicioMedico WHERE pkOrdenMedico = @id";
                        cmd.Parameters.Clear(); cmd.Parameters.AddWithValue("@id", vm.PkOrdenMedico);
                        using (var reader = cmd.ExecuteReader()) {
                            if (reader.Read()) {
                                int? fkCand = reader["fkCandidato"] != DBNull.Value ? (int?)reader["fkCandidato"] : null;
                                int est = (int)reader["fkEstatus"];
                                reader.Close();
                                if (fkCand.HasValue) {
                                    SqlCommand cmdC = new SqlCommand("UPDATE Candidatos SET nss=@nss, telefono=@tel, fechaNacimiento=@fn, manoDominante=@md, fkTipoSangre=@ts, fkSexo=@sex WHERE pkCandidato=@cid", conn, trans);
                                    cmdC.Parameters.AddWithValue("@cid", fkCand.Value); cmdC.Parameters.AddWithValue("@nss", (object)vm.Nss ?? DBNull.Value); cmdC.Parameters.AddWithValue("@tel", (object)vm.Telefono ?? DBNull.Value); cmdC.Parameters.AddWithValue("@fn", (object)vm.FechaNacimiento ?? DBNull.Value); cmdC.Parameters.AddWithValue("@md", (object)vm.ManoDominante ?? DBNull.Value); cmdC.Parameters.AddWithValue("@ts", (object)vm.FkTipoSangre ?? DBNull.Value); cmdC.Parameters.AddWithValue("@sex", (object)vm.SexoCandidato ?? DBNull.Value);
                                    cmdC.ExecuteNonQuery();
                                }
                                if (est == 1) {
                                    SqlCommand cmdE = new SqlCommand("UPDATE OrdenServicioMedico SET fkEstatus = 2 WHERE pkOrdenMedico = @id", conn, trans);
                                    cmdE.Parameters.AddWithValue("@id", vm.PkOrdenMedico); cmdE.ExecuteNonQuery();
                                }
                            }
                        }
                        trans.Commit();
                    }
                    catch { trans.Rollback(); throw; }
                }
            }
        }

        public static EvaluacionMedicaVm ObtenerPorOrden(int pkOrden)
        {
            string sqlEval = @"
                SELECT e.*, 
                       CASE 
                         WHEN e.fkAptitudMedica = 1 THEN 'APTO'
                         WHEN e.fkAptitudMedica = 2 THEN 'APTO CONDICIONADO'
                         WHEN e.fkAptitudMedica = 3 THEN 'NO APTO'
                         ELSE 'SIN CLASIFICAR'
                       END as AptitudDesc
                FROM EvaluacionesClinicas e
                WHERE e.fkOrdenMedico = @id";

            var dtEval = SqlHelper.ExecuteDataTable(sqlEval, new SqlParameter("@id", pkOrden));
            if (dtEval.Rows.Count == 0) return null;

            DataRow r = dtEval.Rows[0];
            int pkEval = Convert.ToInt32(r["pkEvaluacion"]);

            var vm = new EvaluacionMedicaVm {
                PkOrdenMedico = pkOrden,
                PesoKg = r["pesoKg"] != DBNull.Value ? (decimal?)r["pesoKg"] : null,
                AlturaMetros = r["alturaMetros"] != DBNull.Value ? (decimal?)r["alturaMetros"] : null,
                Imc = r["imc"] != DBNull.Value ? (decimal?)r["imc"] : null,
                PresionSistolica = r["presionSistolica"] != DBNull.Value ? (int?)r["presionSistolica"] : null,
                PresionDiastolica = r["presionDiastolica"] != DBNull.Value ? (int?)r["presionDiastolica"] : null,
                Temperatura = r["temperatura"] != DBNull.Value ? (decimal?)r["temperatura"] : null,
                FrecuenciaCardiaca = r["frecuenciaCardiaca"] != DBNull.Value ? (int?)r["frecuenciaCardiaca"] : null,
                FrecuenciaRespiratoria = r["frecuenciaRespiratoria"] != DBNull.Value ? (int?)r["frecuenciaRespiratoria"] : null,
                Glucosa = r["glucosa"] != DBNull.Value ? (decimal?)r["glucosa"] : null,
                Oximetria = r["oximetria"] != DBNull.Value ? (int?)r["oximetria"] : null,
                ImcDescripcion = r["imcDescripcion"]?.ToString(),
                AparatosSistemas = r["aparatosSistemas"]?.ToString(),
                FkAptitudMedica = r["fkAptitudMedica"] != DBNull.Value ? (int?)r["fkAptitudMedica"] : null,
                AptitudMedicaDesc = r["AptitudDesc"]?.ToString(),
                Observaciones = r["observaciones"]?.ToString(),
                Recomendaciones = r["recomendaciones"]?.ToString(),
                SintomasPaciente = r["sintomasPaciente"]?.ToString(),
                Nss = r["nss"]?.ToString(),
                FechaNacimiento = r["fechaNacimiento"] != DBNull.Value ? (DateTime?)r["fechaNacimiento"] : null,
                LugarNacimiento = r["lugarNacimiento"]?.ToString(),
                EstadoCivil = r["estadoCivil"]?.ToString(),
                ManoDominante = r["manoDominante"]?.ToString(),
                Telefono = r["telefono"]?.ToString(),
                Domicilio = r["domicilio"]?.ToString(),
                Escolaridad = r["escolaridad"]?.ToString(),
                Profesion = r["profesion"]?.ToString(),
                Alergias = r["alergias"]?.ToString(),
                FkTipoSangre = r["fkTipoSangre"] != DBNull.Value ? (int?)r["fkTipoSangre"] : null,
                LugarEvaluacion = r["lugarEvaluacion"]?.ToString()
            };

            var dtHabRes = SqlHelper.ExecuteDataTable("SELECT * FROM HabitosPersonales WHERE fkEvaluacion = @id", new SqlParameter("@id", pkEval));
            if (dtHabRes.Rows.Count > 0) {
                DataRow rh = dtHabRes.Rows[0];
                vm.Habitos = new HabitosPersonalesVm { Fuma = rh["fuma"] != DBNull.Value && (bool)rh["fuma"], AnosFumando = rh["anosFumando"] != DBNull.Value ? (int?)rh["anosFumando"] : null, CigarrosDiarios = rh["cigarrosDiarios"] != DBNull.Value ? (int?)rh["cigarrosDiarios"] : null, EsExFumador = rh["esExFumador"] != DBNull.Value && (bool)rh["esExFumador"], BebeAlcohol = rh["bebeAlcohol"] != DBNull.Value && (bool)rh["bebeAlcohol"], FrecuenciaAlcohol = rh["frecuenciaAlcohol"]?.ToString(), UsaDrogas = rh["usaDrogas"] != DBNull.Value && (bool)rh["usaDrogas"], TipoDrogas = rh["tipoDrogas"]?.ToString(), HaceDeporte = rh["haceDeporte"] != DBNull.Value && (bool)rh["haceDeporte"], TipoDeporte = rh["tipoDeporte"]?.ToString(), DescripcionTiempoLibre = rh["descripcionTiempoLibre"]?.ToString() };
            }
            var dtVac = SqlHelper.ExecuteDataTable("SELECT * FROM Vacunacion WHERE fkEvaluacion = @id", new SqlParameter("@id", pkEval));
            if (dtVac.Rows.Count > 0) {
                DataRow rv = dtVac.Rows[0];
                vm.Vacunacion = new VacunacionVm { TetanosDosis1 = rv["tetanosDosis1"] != DBNull.Value && (bool)rv["tetanosDosis1"], TetanosDosis2 = rv["tetanosDosis2"] != DBNull.Value && (bool)rv["tetanosDosis2"], TetanosDosis3 = rv["tetanosDosis3"] != DBNull.Value && (bool)rv["tetanosDosis3"], HepatitisDosis1 = rv["hepatitisDosis1"] != DBNull.Value && (bool)rv["hepatitisDosis1"], HepatitisDosis2 = rv["hepatitisDosis2"] != DBNull.Value && (bool)rv["hepatitisDosis2"], InfluenzaH1N1 = rv["influenzaH1N1"] != DBNull.Value && (bool)rv["influenzaH1N1"], ObservacionesVacunacion = rv["observacionesVacunacion"]?.ToString() };
            }
            var dtAnt = SqlHelper.ExecuteDataTable("SELECT * FROM HistoriaMedica WHERE fkEvaluacion = @id", new SqlParameter("@id", pkEval));
            vm.Antecedentes = new List<HistoriaMedicaVm>();
            foreach (DataRow ra in dtAnt.Rows) vm.Antecedentes.Add(new HistoriaMedicaVm { Categoria = ra["categoria"]?.ToString(), NombreCondicion = ra["nombreCondicion"]?.ToString(), EsPositivo = ra["esPositivo"] != DBNull.Value && (bool)ra["esPositivo"], Detalles = ra["detalles"]?.ToString() });
            var dtLab = SqlHelper.ExecuteDataTable("SELECT * FROM AntecedentesLaborales WHERE fkEvaluacion = @id", new SqlParameter("@id", pkEval));
            vm.AntecedentesLaborales = new List<AntecedenteLaboralVm>();
            foreach (DataRow rl in dtLab.Rows) vm.AntecedentesLaborales.Add(new AntecedenteLaboralVm { Empresa = rl["empresa"]?.ToString(), Puesto = rl["puesto"]?.ToString(), TiempoLaborado = rl["tiempoLaborado"]?.ToString(), AgentesExpuesto = rl["agentesExpuestos"]?.ToString(), AccidentesPrevios = rl["accidentesPrevios"]?.ToString() });
            var dtExF = SqlHelper.ExecuteDataTable("SELECT * FROM OrdenExamenFisico WHERE fkEvaluacion = @id", new SqlParameter("@id", pkEval));
            vm.OrdenExamenFisico = new List<OrdenExamenFisicoVm>();
            foreach (DataRow re in dtExF.Rows) {
                string sis = re["sistemaCuerpo"]?.ToString();
                if (sis == "AGUDEZA_VISUAL") {
                    var parts = re["hallazgos"]?.ToString().Split('|').Select(p => p.Split(':').LastOrDefault()).ToArray();
                    if (parts != null && parts.Length >= 9) vm.AgudezaVisual = new AgudezaVisualVm { OdSinLentes = parts[0], OiSinLentes = parts[1], AoSinLentes = parts[2], OdConLentes = parts[3], OiConLentes = parts[4], AoConLentes = parts[5], UsaLentes = parts[6], ReferenciaVisual = parts[7], Daltonismo = parts[8] };
                } else vm.OrdenExamenFisico.Add(new OrdenExamenFisicoVm { SistemaCuerpo = sis, EsNormal = re["esNormal"] != DBNull.Value && (bool)re["esNormal"], Hallazgos = re["hallazgos"]?.ToString() });
            }
            var dtCol = SqlHelper.ExecuteDataTable("SELECT * FROM EvaluacionColumna WHERE fkEvaluacion = @id", new SqlParameter("@id", pkEval));
            if (dtCol.Rows.Count > 0) {
                DataRow rc = dtCol.Rows[0];
                vm.Columna = new EvaluacionColumnaVm { LordosisCervical = rc["lordosisCervical"] != DBNull.Value ? (int?)Convert.ToInt32(rc["lordosisCervical"]) : null, LordosisDorsal = rc["lordosisDorsal"] != DBNull.Value ? (int?)Convert.ToInt32(rc["lordosisDorsal"]) : null, LordosisLumbar = rc["lordosisLumbar"] != DBNull.Value ? (int?)Convert.ToInt32(rc["lordosisLumbar"]) : null, CifosisCervical = rc["cifosisCervical"] != DBNull.Value ? (int?)Convert.ToInt32(rc["cifosisCervical"]) : null, CifosisDorsal = rc["cifosisDorsal"] != DBNull.Value ? (int?)Convert.ToInt32(rc["cifosisDorsal"]) : null, CifosisLumbar = rc["cifosisLumbar"] != DBNull.Value ? (int?)Convert.ToInt32(rc["cifosisLumbar"]) : null, EscoliosisDorsalDerecha = rc["escoliosisDorsalDerecha"] != DBNull.Value && (bool)rc["escoliosisDorsalDerecha"], EscoliosisDorsalIzquierda = rc["escoliosisDorsalIzquierda"] != DBNull.Value && (bool)rc["escoliosisDorsalIzquierda"], EscoliosisLumbarDerecha = rc["escoliosisLumbarDerecha"] != DBNull.Value && (bool)rc["escoliosisLumbarDerecha"], EscoliosisLumbarIzquierda = rc["escoliosisLumbarIzquierda"] != DBNull.Value && (bool)rc["escoliosisLumbarIzquierda"], EscoliosisDobleDerecha = rc["escoliosisDobleDerecha"] != DBNull.Value && (bool)rc["escoliosisDobleDerecha"], EscoliosisDobleIzquierda = rc["escoliosisDobleIzquierda"] != DBNull.Value && (bool)rc["escoliosisDobleIzquierda"], ObservacionesColumna = rc["observacionesColumna"]?.ToString() };
            }
            var dtGin = SqlHelper.ExecuteDataTable("SELECT * FROM DetallesGinecoObstetricos WHERE fkEvaluacion = @id", new SqlParameter("@id", pkEval));
            if (dtGin.Rows.Count > 0) {
                DataRow rg = dtGin.Rows[0];
                vm.DetalleFemenino = new DetalleGinecoVm { 
                    EdadMenarca = rg["edadMenarca"] != DBNull.Value ? (int?)Convert.ToInt32(rg["edadMenarca"]) : null, 
                    FechaUltimaMenstruacion = rg["fechaUltimaMenstruacion"] != DBNull.Value ? (DateTime?)rg["fechaUltimaMenstruacion"] : null, 
                    Ciclos = rg["ciclos"]?.ToString(), 
                    Gestas = rg["gestas"] != DBNull.Value ? (int?)Convert.ToInt32(rg["gestas"]) : null, 
                    Partos = rg["partos"] != DBNull.Value ? (int?)Convert.ToInt32(rg["partos"]) : null, 
                    Abortos = rg["abortos"] != DBNull.Value ? (int?)Convert.ToInt32(rg["abortos"]) : null, 
                    Cesareas = rg["cesareas"] != DBNull.Value ? (int?)Convert.ToInt32(rg["cesareas"]) : null, 
                    MetodoPlanificacion = rg["metodoPlanificacion"]?.ToString(), 
                    FechaUltimoPapanicolau = rg["fechaUltimoPapanicolau"] != DBNull.Value ? (DateTime?)rg["fechaUltimoPapanicolau"] : null,
                    Ivsa = rg.Table.Columns.Contains("ivsa") && rg["ivsa"] != DBNull.Value ? (int?)Convert.ToInt32(rg["ivsa"]) : null,
                    Ets = rg.Table.Columns.Contains("ets") ? rg["ets"]?.ToString() : null,
                    NumeroHijosEdades = rg["edadesHijos"]?.ToString() 
                };
            } else {
                var dtMas = SqlHelper.ExecuteDataTable("SELECT * FROM DetallesGenitourinariosMasc WHERE fkEvaluacion = @id", new SqlParameter("@id", pkEval));
                if (dtMas.Rows.Count > 0) {
                    DataRow rm = dtMas.Rows[0];
                    vm.DetalleMasculino = new DetalleGenitoMascVm { 
                        PrepucioRetractil = rm["prepucioRetractil"] != DBNull.Value && (bool)rm["prepucioRetractil"], 
                        TesticulosDescendidos = rm["testiculosDescendidos"] != DBNull.Value && (bool)rm["testiculosDescendidos"], 
                        Fimosis = rm["fimosis"] != DBNull.Value && (bool)rm["fimosis"], 
                        Criptorquidia = rm["criptorquidia"] != DBNull.Value && (bool)rm["criptorquidia"], 
                        Varicocele = rm["varicocele"] != DBNull.Value && (bool)rm["varicocele"], 
                        Hidrocele = rm["hidrocele"] != DBNull.Value && (bool)rm["hidrocele"], 
                        Hernia = rm["hernia"] != DBNull.Value && (bool)rm["hernia"], 
                        Psa = rm["psa"]?.ToString(), 
                        MetodoPlanificacion = rm["mpf"]?.ToString(),
                        Ivsa = rm.Table.Columns.Contains("ivsa") ? rm["ivsa"]?.ToString() : null
                    };
                }
            }
            return vm;
        }

        public static EvaluacionMedicaVm ObtenerUltimaEvaluacionPorPaciente(int? fkCandidato, int? fkEmpleado)
        {
            if (!fkCandidato.HasValue && !fkEmpleado.HasValue) return null;
            string where = fkEmpleado.HasValue ? "o.fkEmpleado = @pid" : "o.fkCandidato = @pid";
            int pid = fkEmpleado ?? fkCandidato.Value;
            string sql = $@"SELECT TOP 1 e.fkOrdenMedico FROM EvaluacionesClinicas e INNER JOIN OrdenServicioMedico o ON e.fkOrdenMedico = o.pkOrdenMedico WHERE {where} ORDER BY e.fechaEvaluacion DESC";
            object res = SqlHelper.ExecuteScalar(sql, new SqlParameter("@pid", pid));
            if (res == null) return null;
            return ObtenerPorOrden(Convert.ToInt32(res));
        }

        public static List<ResumenEvaluacionVm> ObtenerHistorialCompleto(int fkEmpleado)
        {
            string sql = @"
                SELECT e.*, 
                       CASE 
                         WHEN e.fkAptitudMedica = 1 THEN 'APTO'
                         WHEN e.fkAptitudMedica = 2 THEN 'APTO CONDICIONADO'
                         WHEN e.fkAptitudMedica = 3 THEN 'NO APTO'
                         ELSE 'SIN CLASIFICAR'
                       END as AptitudDesc
                FROM EvaluacionesClinicas e
                INNER JOIN OrdenServicioMedico o ON e.fkOrdenMedico = o.pkOrdenMedico
                WHERE o.fkEmpleado = @id
                ORDER BY e.fechaEvaluacion DESC";

            DataTable dt = SqlHelper.ExecuteDataTable(sql, new SqlParameter("@id", fkEmpleado));
            List<ResumenEvaluacionVm> lista = new List<ResumenEvaluacionVm>();
            foreach (DataRow r in dt.Rows) {
                int pkEval = (int)r["pkEvaluacion"];
                
                // 1. Antecedentes
                var positivos = new List<string>();
                var dtPos = SqlHelper.ExecuteDataTable("SELECT nombreCondicion FROM HistoriaMedica WHERE fkEvaluacion = @id AND esPositivo = 1", new SqlParameter("@id", pkEval));
                foreach (DataRow rp in dtPos.Rows) positivos.Add(rp["nombreCondicion"].ToString());

                // 2. Vacunas Resumen
                string vacRes = "—";
                var dtVac = SqlHelper.ExecuteDataTable("SELECT * FROM Vacunacion WHERE fkEvaluacion = @id", new SqlParameter("@id", pkEval));
                if (dtVac.Rows.Count > 0) {
                    var rv = dtVac.Rows[0];
                    var vs = new List<string>();
                    if (rv["tetanosDosis1"] != DBNull.Value && (bool)rv["tetanosDosis1"]) vs.Add("Tétanos (D1)");
                    if (rv["tetanosDosis2"] != DBNull.Value && (bool)rv["tetanosDosis2"]) vs.Add("Tétanos (D2)");
                    if (rv["tetanosDosis3"] != DBNull.Value && (bool)rv["tetanosDosis3"]) vs.Add("Tétanos (D3)");
                    if (rv["hepatitisDosis1"] != DBNull.Value && (bool)rv["hepatitisDosis1"]) vs.Add("HepB (D1)");
                    if (rv["hepatitisDosis2"] != DBNull.Value && (bool)rv["hepatitisDosis2"]) vs.Add("HepB (D2)");
                    if (rv["influenzaH1N1"] != DBNull.Value && (bool)rv["influenzaH1N1"]) vs.Add("H1N1");
                    vacRes = vs.Count > 0 ? string.Join(", ", vs) : "Ninguna";
                }

                // 3. Visión y Sistemas
                string visRes = "—";
                var sisAnorm = new List<string>();
                var dtExF = SqlHelper.ExecuteDataTable("SELECT * FROM OrdenExamenFisico WHERE fkEvaluacion = @id", new SqlParameter("@id", pkEval));
                foreach (DataRow re in dtExF.Rows) {
                    string sis = re["sistemaCuerpo"]?.ToString();
                    bool normal = re["esNormal"] != DBNull.Value && (bool)re["esNormal"];
                    if (sis == "AGUDEZA_VISUAL") {
                        var parts = re["hallazgos"]?.ToString().Split('|').Select(p => p.Split(':').LastOrDefault()).ToArray();
                        if (parts != null && parts.Length >= 3) visRes = "OD:" + parts[0] + " OI:" + parts[1];
                    } else if (!normal) {
                        sisAnorm.Add(sis);
                    }
                }

                // 4. Columna
                string colRes = "Normal";
                var dtCol = SqlHelper.ExecuteDataTable("SELECT * FROM EvaluacionColumna WHERE fkEvaluacion = @id", new SqlParameter("@id", pkEval));
                if (dtCol.Rows.Count > 0) {
                    var rc = dtCol.Rows[0];
                    var cs = new List<string>();
                    if (rc["escoliosisDorsalDerecha"] != DBNull.Value && (bool)rc["escoliosisDorsalDerecha"]) cs.Add("Escoliosis DD");
                    if (rc["escoliosisDorsalIzquierda"] != DBNull.Value && (bool)rc["escoliosisDorsalIzquierda"]) cs.Add("Escoliosis DI");
                    if (rc["escoliosisLumbarDerecha"] != DBNull.Value && (bool)rc["escoliosisLumbarDerecha"]) cs.Add("Escoliosis LD");
                    if (rc["escoliosisLumbarIzquierda"] != DBNull.Value && (bool)rc["escoliosisLumbarIzquierda"]) cs.Add("Escoliosis LI");
                    colRes = cs.Count > 0 ? string.Join(", ", cs) : "Alineada";
                }

                lista.Add(new ResumenEvaluacionVm { 
                    PkEvaluacion = pkEval, 
                    FechaEvaluacion = r["fechaEvaluacion"] != DBNull.Value ? Convert.ToDateTime(r["fechaEvaluacion"]).ToString("dd/MM/yyyy") : "—", 
                    AptitudDesc = r["AptitudDesc"]?.ToString() ?? "—", 
                    FkAptitudMedica = r["fkAptitudMedica"] != DBNull.Value ? (int?)r["fkAptitudMedica"] : null, 
                    PesoKg = r["pesoKg"] != DBNull.Value ? (decimal?)r["pesoKg"] : null, 
                    AlturaMetros = r["alturaMetros"] != DBNull.Value ? (decimal?)r["alturaMetros"] : null, 
                    Imc = r["imc"] != DBNull.Value ? (decimal?)r["imc"] : null, 
                    ImcDescripcion = r["imcDescripcion"]?.ToString(), 
                    PresionSistolica = r["presionSistolica"] != DBNull.Value ? (int?)r["presionSistolica"] : null, 
                    PresionDiastolica = r["presionDiastolica"] != DBNull.Value ? (int?)r["presionDiastolica"] : null, 
                    Observaciones = r["observaciones"]?.ToString(), 
                    LugarEvaluacion = r["lugarEvaluacion"]?.ToString(), 
                    AntecedentesPositivos = positivos,
                    VacunasResumen = vacRes,
                    VisionResumen = visRes,
                    SistemasAnormales = string.Join(", ", sisAnorm),
                    ColumnaResumen = colRes
                });
            }
            return lista;
        }
    }
}

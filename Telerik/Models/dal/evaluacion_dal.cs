using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Telerik.Models.ViewModels;

namespace Telerik.Models.Dal
{
    public class EvaluacionDal
    {
        public static void GuardarEvaluacion(EvaluacionMedicaVm vm)
        {
            using (SqlConnection con = ConexionBd.ObtenerConexion())
            {
                con.Open();
                SqlTransaction transaccion = con.BeginTransaction();

                try
                {
                    // 1. Insertar/Actualizar EvaluacionClinica Principal
                    string sqlEval = @"
                        INSERT INTO EvaluacionesClinicas 
                        (fkOrdenMedico, fechaEvaluacion, pesoKg, alturaMetros, imc, presionSistolica, presionDiastolica, temperatura, frecuenciaCardiaca, frecuenciaRespiratoria, aparatosSistemas, fkAptitudMedica, observaciones, recomendaciones, sintomasPaciente, nss, fechaNacimiento, lugarNacimiento, estadoCivil, manoDominante, telefono, domicilio, escolaridad, profesion, alergias, tipoSangre)
                        VALUES 
                        (@fkOrden, GETDATE(), @peso, @altura, @imc, @psis, @pdias, @temp, @fc, @fr, @aparatos, @aptitud, @obs, @recom, @sintomas, @nss, @fnac, @lnac, @eciv, @mano, @tel, @dom, @esc, @prof, @ale, @tsangre);
                        SELECT SCOPE_IDENTITY();";

                    SqlCommand cmdEval = new SqlCommand(sqlEval, con, transaccion);
                    cmdEval.Parameters.AddWithValue("@fkOrden", vm.PkOrdenMedico);
                    cmdEval.Parameters.AddWithValue("@peso", (object)vm.PesoKg ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@altura", (object)vm.AlturaMetros ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@imc", (object)vm.Imc ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@psis", (object)vm.PresionSistolica ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@pdias", (object)vm.PresionDiastolica ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@temp", (object)vm.Temperatura ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@fc", (object)vm.FrecuenciaCardiaca ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@fr", (object)vm.FrecuenciaRespiratoria ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@aparatos", (object)vm.AparatosSistemas ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@aptitud", (object)vm.FkAptitudMedica ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@obs", (object)vm.Observaciones ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@recom", (object)vm.Recomendaciones ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@sintomas", (object)vm.SintomasPaciente ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@nss", (object)vm.Nss ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@fnac", (object)vm.FechaNacimiento ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@lnac", (object)vm.LugarNacimiento ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@eciv", (object)vm.EstadoCivil ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@mano", (object)vm.ManoDominante ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@tel", (object)vm.Telefono ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@dom", (object)vm.Domicilio ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@esc", (object)vm.Escolaridad ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@prof", (object)vm.Profesion ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@ale", (object)vm.Alergias ?? DBNull.Value);
                    cmdEval.Parameters.AddWithValue("@tsangre", (object)vm.TipoSangre ?? DBNull.Value);

                    int pkEvaluacion = Convert.ToInt32(cmdEval.ExecuteScalar());

                    // 2. Habitos Personales
                    if (vm.Habitos != null)
                    {
                        string sqlHab = @"
                            INSERT INTO HabitosPersonales
                            (fkEvaluacion, fuma, anosFumando, cigarrosDiarios, esExFumador, bebeAlcohol, frecuenciaAlcohol, usaDrogas, tipoDrogas, haceDeporte, descripcionTiempoLibre, vacunaTetanos, vacunaHepatitis, vacunaH1N1)
                            VALUES
                            (@fk, @fuma, @anosF, @cigarros, @exFuma, @alcohol, @frecA, @drogas, @tipoD, @deporte, @descTL, @vt, @vh, @hn)";
                        
                        SqlCommand cmdHab = new SqlCommand(sqlHab, con, transaccion);
                        cmdHab.Parameters.AddWithValue("@fk", pkEvaluacion);
                        cmdHab.Parameters.AddWithValue("@fuma", vm.Habitos.Fuma);
                        cmdHab.Parameters.AddWithValue("@anosF", (object)vm.Habitos.AnosFumando ?? DBNull.Value);
                        cmdHab.Parameters.AddWithValue("@cigarros", (object)vm.Habitos.CigarrosDiarios ?? DBNull.Value);
                        cmdHab.Parameters.AddWithValue("@exFuma", vm.Habitos.EsExFumador);
                        cmdHab.Parameters.AddWithValue("@alcohol", vm.Habitos.BebeAlcohol);
                        cmdHab.Parameters.AddWithValue("@frecA", (object)vm.Habitos.FrecuenciaAlcohol ?? DBNull.Value);
                        cmdHab.Parameters.AddWithValue("@drogas", vm.Habitos.UsaDrogas);
                        cmdHab.Parameters.AddWithValue("@tipoD", (object)vm.Habitos.TipoDrogas ?? DBNull.Value);
                        cmdHab.Parameters.AddWithValue("@deporte", vm.Habitos.HaceDeporte);
                        cmdHab.Parameters.AddWithValue("@descTL", (object)vm.Habitos.DescripcionTiempoLibre ?? DBNull.Value);
                        cmdHab.Parameters.AddWithValue("@vt", (object)vm.Habitos.VacunaTetanos ?? DBNull.Value);
                        cmdHab.Parameters.AddWithValue("@vh", (object)vm.Habitos.VacunaHepatitis ?? DBNull.Value);
                        cmdHab.Parameters.AddWithValue("@hn", vm.Habitos.VacunaH1N1);
                        cmdHab.ExecuteNonQuery();
                    }

                    // 3. Historia Medica (Antecedentes)
                    if (vm.Antecedentes != null)
                    {
                        foreach (var ant in vm.Antecedentes)
                        {
                            string sqlAnt = @"
                                INSERT INTO HistoriaMedica (fkEvaluacion, categoria, nombreCondicion, esPositivo, detalles)
                                VALUES (@fk, @cat, @nom, @pos, @det)";
                            SqlCommand cmdAnt = new SqlCommand(sqlAnt, con, transaccion);
                            cmdAnt.Parameters.AddWithValue("@fk", pkEvaluacion);
                            cmdAnt.Parameters.AddWithValue("@cat", (object)ant.Categoria ?? DBNull.Value);
                            cmdAnt.Parameters.AddWithValue("@nom", (object)ant.NombreCondicion ?? DBNull.Value);
                            cmdAnt.Parameters.AddWithValue("@pos", ant.EsPositivo);
                            cmdAnt.Parameters.AddWithValue("@det", (object)ant.Detalles ?? DBNull.Value);
                            cmdAnt.ExecuteNonQuery();
                        }
                    }

                    // 3.5 Antecedentes Laborales
                    if (vm.AntecedentesLaborales != null)
                    {
                        foreach (var al in vm.AntecedentesLaborales)
                        {
                            string sqlAl = @"
                                INSERT INTO AntecedentesLaborales (fkEvaluacion, empresa, puesto, tiempoLaborado, agentesExpuesto, accidentesPrevios)
                                VALUES (@fk, @emp, @psto, @tiempo, @agentes, @accid)";
                            SqlCommand cmdAl = new SqlCommand(sqlAl, con, transaccion);
                            cmdAl.Parameters.AddWithValue("@fk", pkEvaluacion);
                            cmdAl.Parameters.AddWithValue("@emp", (object)al.Empresa ?? DBNull.Value);
                            cmdAl.Parameters.AddWithValue("@psto", (object)al.Puesto ?? DBNull.Value);
                            cmdAl.Parameters.AddWithValue("@tiempo", (object)al.TiempoLaborado ?? DBNull.Value);
                            cmdAl.Parameters.AddWithValue("@agentes", (object)al.AgentesExpuesto ?? DBNull.Value);
                            cmdAl.Parameters.AddWithValue("@accid", (object)al.AccidentesPrevios ?? DBNull.Value);
                            cmdAl.ExecuteNonQuery();
                        }
                    }

                    // 4. Examen Fisico
                    if (vm.ExamenFisico != null)
                    {
                        foreach (var ef in vm.ExamenFisico)
                        {
                            string sqlEf = @"
                                INSERT INTO ExamenFisico (fkEvaluacion, sistemaCuerpo, esNormal, hallazgos)
                                VALUES (@fk, @sis, @norm, @hall)";
                            SqlCommand cmdEf = new SqlCommand(sqlEf, con, transaccion);
                            cmdEf.Parameters.AddWithValue("@fk", pkEvaluacion);
                            cmdEf.Parameters.AddWithValue("@sis", (object)ef.SistemaCuerpo ?? DBNull.Value);
                            cmdEf.Parameters.AddWithValue("@norm", ef.EsNormal);
                            cmdEf.Parameters.AddWithValue("@hall", (object)ef.Hallazgos ?? DBNull.Value);
                            cmdEf.ExecuteNonQuery();
                        }
                    }

                    // 5. Columna
                    if (vm.Columna != null)
                    {
                        string sqlCol = @"
                            INSERT INTO EvaluacionColumna 
                            (fkEvaluacion, lordosisCervical, lordosisDorsal, lordosisLumbar, cifosisCervical, cifosisDorsal, cifosisLumbar,
                             escoliosisDorsalDerecha, escoliosisDorsalIzquierda, escoliosisLumbarDerecha, escoliosisLumbarIzquierda, observacionesColumna)
                            VALUES
                            (@fk, @lc, @ld, @ll, @cc, @cd, @cl, @edd, @edi, @eld, @eli, @obs)";
                        
                        SqlCommand cmdCol = new SqlCommand(sqlCol, con, transaccion);
                        cmdCol.Parameters.AddWithValue("@fk", pkEvaluacion);
                        cmdCol.Parameters.AddWithValue("@lc", (object)vm.Columna.LordosisCervical ?? DBNull.Value);
                        cmdCol.Parameters.AddWithValue("@ld", (object)vm.Columna.LordosisDorsal ?? DBNull.Value);
                        cmdCol.Parameters.AddWithValue("@ll", (object)vm.Columna.LordosisLumbar ?? DBNull.Value);
                        cmdCol.Parameters.AddWithValue("@cc", (object)vm.Columna.CifosisCervical ?? DBNull.Value);
                        cmdCol.Parameters.AddWithValue("@cd", (object)vm.Columna.CifosisDorsal ?? DBNull.Value);
                        cmdCol.Parameters.AddWithValue("@cl", (object)vm.Columna.CifosisLumbar ?? DBNull.Value);
                        cmdCol.Parameters.AddWithValue("@edd", vm.Columna.EscoliosisDorsalDerecha);
                        cmdCol.Parameters.AddWithValue("@edi", vm.Columna.EscoliosisDorsalIzquierda);
                        cmdCol.Parameters.AddWithValue("@eld", vm.Columna.EscoliosisLumbarDerecha);
                        cmdCol.Parameters.AddWithValue("@eli", vm.Columna.EscoliosisLumbarIzquierda);
                        cmdCol.Parameters.AddWithValue("@obs", (object)vm.Columna.ObservacionesColumna ?? DBNull.Value);
                        cmdCol.ExecuteNonQuery();
                    }

                    // 6. Específicos Masc/Fem
                    if (vm.DetalleFemenino != null)
                    {
                        string sqlFem = @"
                            INSERT INTO DetallesGinecoObstetricos
                            (fkEvaluacion, edadMenarca, fechaUltimaMenstruacion, ciclos, gestas, partos, abortos, cesareas, ivsa, metodoPlanificacion, fechaUltimoPapanicolau, ets, numeroHijosEdades)
                            VALUES
                            (@fk, @em, @fum, @cicl, @gest, @part, @abor, @ces, @ivsa, @plan, @fup, @ets, @nhe)";
                        
                        SqlCommand cmdFem = new SqlCommand(sqlFem, con, transaccion);
                        cmdFem.Parameters.AddWithValue("@fk", pkEvaluacion);
                        cmdFem.Parameters.AddWithValue("@em", (object)vm.DetalleFemenino.EdadMenarca ?? DBNull.Value);
                        cmdFem.Parameters.AddWithValue("@fum", (object)vm.DetalleFemenino.FechaUltimaMenstruacion ?? DBNull.Value);
                        cmdFem.Parameters.AddWithValue("@cicl", (object)vm.DetalleFemenino.Ciclos ?? DBNull.Value);
                        cmdFem.Parameters.AddWithValue("@gest", (object)vm.DetalleFemenino.Gestas ?? DBNull.Value);
                        cmdFem.Parameters.AddWithValue("@part", (object)vm.DetalleFemenino.Partos ?? DBNull.Value);
                        cmdFem.Parameters.AddWithValue("@abor", (object)vm.DetalleFemenino.Abortos ?? DBNull.Value);
                        cmdFem.Parameters.AddWithValue("@ces", (object)vm.DetalleFemenino.Cesareas ?? DBNull.Value);
                        cmdFem.Parameters.AddWithValue("@ivsa", (object)vm.DetalleFemenino.Ivsa ?? DBNull.Value);
                        cmdFem.Parameters.AddWithValue("@plan", (object)vm.DetalleFemenino.MetodoPlanificacion ?? DBNull.Value);
                        cmdFem.Parameters.AddWithValue("@fup", (object)vm.DetalleFemenino.FechaUltimoPapanicolau ?? DBNull.Value);
                        cmdFem.Parameters.AddWithValue("@ets", (object)vm.DetalleFemenino.Ets ?? DBNull.Value);
                        cmdFem.Parameters.AddWithValue("@nhe", (object)vm.DetalleFemenino.NumeroHijosEdades ?? DBNull.Value);
                        cmdFem.ExecuteNonQuery();
                    }
                    else if (vm.DetalleMasculino != null)
                    {
                        string sqlMasc = @"
                            INSERT INTO DetallesGenitourinariosMasc
                            (fkEvaluacion, prepucioRetractil, testiculosDescendidos, fimosis, criptorquidia, varicocele, hidrocele, hernia, ivsa, psa)
                            VALUES
                            (@fk, @pr, @td, @fim, @crip, @vari, @hidr, @hern, @ivsa, @psa)";
                         
                        SqlCommand cmdMasc = new SqlCommand(sqlMasc, con, transaccion);
                        cmdMasc.Parameters.AddWithValue("@fk", pkEvaluacion);
                        cmdMasc.Parameters.AddWithValue("@pr", vm.DetalleMasculino.PrepucioRetractil);
                        cmdMasc.Parameters.AddWithValue("@td", vm.DetalleMasculino.TesticulosDescendidos);
                        cmdMasc.Parameters.AddWithValue("@fim", vm.DetalleMasculino.Fimosis);
                        cmdMasc.Parameters.AddWithValue("@crip", vm.DetalleMasculino.Criptorquidia);
                        cmdMasc.Parameters.AddWithValue("@vari", vm.DetalleMasculino.Varicocele);
                        cmdMasc.Parameters.AddWithValue("@hidr", vm.DetalleMasculino.Hidrocele);
                        cmdMasc.Parameters.AddWithValue("@hern", vm.DetalleMasculino.Hernia);
                        cmdMasc.Parameters.AddWithValue("@ivsa", (object)vm.DetalleMasculino.Ivsa ?? DBNull.Value);
                        cmdMasc.Parameters.AddWithValue("@psa", (object)vm.DetalleMasculino.Psa ?? DBNull.Value);
                        cmdMasc.ExecuteNonQuery();
                    }

                    // 7. Actualizar estatus de la orden a 'En Proceso' (2) si estaba pendiente
                    string sqlUpdate = "UPDATE OrdenServicioMedico SET fkEstatus = 2 WHERE pkOrdenMedico = @pk AND fkEstatus = 1";
                    SqlCommand cmdUpd = new SqlCommand(sqlUpdate, con, transaccion);
                    cmdUpd.Parameters.AddWithValue("@pk", vm.PkOrdenMedico);
                    cmdUpd.ExecuteNonQuery();

                    // 7. Actualizar Candidato (Si aplica y vienen datos nuevos)
                    if (!string.IsNullOrEmpty(vm.NombreCandidato))
                    {
                        // Obtener fkCandidato de la orden
                        string sqlGetCand = "SELECT fkCandidato FROM OrdenServicioMedico WHERE pkOrdenMedico = @pk";
                        SqlCommand cmdGetCand = new SqlCommand(sqlGetCand, con, transaccion);
                        cmdGetCand.Parameters.AddWithValue("@pk", vm.PkOrdenMedico);
                        object resCand = cmdGetCand.ExecuteScalar();

                        if (resCand != null && resCand != DBNull.Value)
                        {
                            int fkCandidato = Convert.ToInt32(resCand);
                            // Actualizamos nombre completo en 'nombre' y limpiamos apellidos para evitar inconsistencias de concatenación
                            string sqlUpdCand = @"
                                UPDATE Candidatos SET 
                                    nombre = @nom, 
                                    aPaterno = '', 
                                    aMaterno = '',
                                    puestoDeseado = @puesto, 
                                    area = @area,
                                    empresa = @emp,
                                    fkSexo = @sexo
                                WHERE pkCandidato = @pkc";
                            
                            SqlCommand cmdUpdCand = new SqlCommand(sqlUpdCand, con, transaccion);
                            cmdUpdCand.Parameters.AddWithValue("@nom", vm.NombreCandidato);
                            cmdUpdCand.Parameters.AddWithValue("@puesto", (object)vm.PuestoCandidato ?? DBNull.Value);
                            cmdUpdCand.Parameters.AddWithValue("@area", (object)vm.AreaCandidato ?? DBNull.Value);
                            cmdUpdCand.Parameters.AddWithValue("@emp", (object)vm.EmpresaCandidato ?? DBNull.Value);
                            cmdUpdCand.Parameters.AddWithValue("@sexo", (object)vm.SexoCandidato ?? DBNull.Value);
                            cmdUpdCand.Parameters.AddWithValue("@pkc", fkCandidato);
                            cmdUpdCand.ExecuteNonQuery();
                        }
                    }

                    transaccion.Commit();
                }
                catch (Exception)
                {
                    transaccion.Rollback();
                    throw;
                }
            }
        }
        public static EvaluacionMedicaVm ObtenerPorOrden(int pkOrden)
        {
            EvaluacionMedicaVm vm = null;
            using (SqlConnection con = ConexionBd.ObtenerConexion())
            {
                con.Open();
                
                // 1. Get Main Info + 1-to-1 tables
                string sqlMain = @"
                    SELECT e.*, 
                           h.fuma, h.anosFumando, h.cigarrosDiarios, h.esExFumador, h.bebeAlcohol, h.frecuenciaAlcohol, h.usaDrogas, h.tipoDrogas, h.haceDeporte, h.descripcionTiempoLibre,
                           c.lordosisCervical, c.lordosisDorsal, c.lordosisLumbar, c.cifosisCervical, c.cifosisDorsal, c.cifosisLumbar, c.escoliosisDorsalDerecha, c.escoliosisDorsalIzquierda, c.escoliosisLumbarDerecha, c.escoliosisLumbarIzquierda, c.observacionesColumna,
                           fem.edadMenarca, fem.fechaUltimaMenstruacion, fem.ciclos, fem.gestas, fem.partos, fem.abortos, fem.cesareas, fem.ivsa AS ivsaFem, fem.metodoPlanificacion, fem.fechaUltimoPapanicolau, fem.ets,
                           masc.prepucioRetractil, masc.testiculosDescendidos, masc.fimosis, masc.criptorquidia, masc.varicocele, masc.hidrocele, masc.hernia, masc.ivsa AS ivsaMasc, masc.psa
                    FROM EvaluacionesClinicas e
                    LEFT JOIN HabitosPersonales h ON e.pkEvaluacion = h.fkEvaluacion
                    LEFT JOIN EvaluacionColumna c ON e.pkEvaluacion = c.fkEvaluacion
                    LEFT JOIN DetallesGinecoObstetricos fem ON e.pkEvaluacion = fem.fkEvaluacion
                    LEFT JOIN DetallesGenitourinariosMasc masc ON e.pkEvaluacion = masc.fkEvaluacion
                    WHERE e.fkOrdenMedico = @pk";

                SqlCommand cmd = new SqlCommand(sqlMain, con);
                cmd.Parameters.AddWithValue("@pk", pkOrden);
                
                int pkEvaluacion = 0;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        pkEvaluacion = Convert.ToInt32(dr["pkEvaluacion"]);
                        vm = new EvaluacionMedicaVm
                        {
                            PkOrdenMedico = pkOrden,
                            PesoKg = dr["pesoKg"] != DBNull.Value ? (decimal?)Convert.ToDecimal(dr["pesoKg"]) : null,
                            AlturaMetros = dr["alturaMetros"] != DBNull.Value ? (decimal?)Convert.ToDecimal(dr["alturaMetros"]) : null,
                            Imc = dr["imc"] != DBNull.Value ? (decimal?)Convert.ToDecimal(dr["imc"]) : null,
                            PresionSistolica = dr["presionSistolica"] != DBNull.Value ? (int?)Convert.ToInt32(dr["presionSistolica"]) : null,
                            PresionDiastolica = dr["presionDiastolica"] != DBNull.Value ? (int?)Convert.ToInt32(dr["presionDiastolica"]) : null,
                            Temperatura = dr["temperatura"] != DBNull.Value ? (decimal?)Convert.ToDecimal(dr["temperatura"]) : null,
                            FrecuenciaCardiaca = dr["frecuenciaCardiaca"] != DBNull.Value ? (int?)Convert.ToInt32(dr["frecuenciaCardiaca"]) : null,
                            FrecuenciaRespiratoria = dr["frecuenciaRespiratoria"] != DBNull.Value ? (int?)Convert.ToInt32(dr["frecuenciaRespiratoria"]) : null,
                            AparatosSistemas = dr["aparatosSistemas"] != DBNull.Value ? dr["aparatosSistemas"].ToString() : "",
                            FkAptitudMedica = dr["fkAptitudMedica"] != DBNull.Value ? (int?)Convert.ToInt32(dr["fkAptitudMedica"]) : null,
                            Observaciones = dr["observaciones"] != DBNull.Value ? dr["observaciones"].ToString() : "",
                            SintomasPaciente = dr["sintomasPaciente"] != DBNull.Value ? dr["sintomasPaciente"].ToString() : "",
                            
                            Nss = dr["nss"] != DBNull.Value ? dr["nss"].ToString() : "",
                            FechaNacimiento = dr["fechaNacimiento"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(dr["fechaNacimiento"]) : null,
                            LugarNacimiento = dr["lugarNacimiento"] != DBNull.Value ? dr["lugarNacimiento"].ToString() : "",
                            EstadoCivil = dr["estadoCivil"] != DBNull.Value ? dr["estadoCivil"].ToString() : "",
                            ManoDominante = dr["manoDominante"] != DBNull.Value ? dr["manoDominante"].ToString() : "",
                            Telefono = dr["telefono"] != DBNull.Value ? dr["telefono"].ToString() : "",
                            Domicilio = dr["domicilio"] != DBNull.Value ? dr["domicilio"].ToString() : "",
                            Escolaridad = dr["escolaridad"] != DBNull.Value ? dr["escolaridad"].ToString() : "",
                            Profesion = dr["profesion"] != DBNull.Value ? dr["profesion"].ToString() : "",
                            Alergias = dr["alergias"] != DBNull.Value ? dr["alergias"].ToString() : "",
                            TipoSangre = dr["tipoSangre"] != DBNull.Value ? dr["tipoSangre"].ToString() : "",
                            
                            Habitos = new HabitosPersonalesVm
                            {
                                Fuma = dr["fuma"] != DBNull.Value && Convert.ToBoolean(dr["fuma"]),
                                AnosFumando = dr["anosFumando"] != DBNull.Value ? (int?)Convert.ToInt32(dr["anosFumando"]) : null,
                                CigarrosDiarios = dr["cigarrosDiarios"] != DBNull.Value ? (int?)Convert.ToInt32(dr["cigarrosDiarios"]) : null,
                                EsExFumador = dr["esExFumador"] != DBNull.Value && Convert.ToBoolean(dr["esExFumador"]),
                                BebeAlcohol = dr["bebeAlcohol"] != DBNull.Value && Convert.ToBoolean(dr["bebeAlcohol"]),
                                FrecuenciaAlcohol = dr["frecuenciaAlcohol"] != DBNull.Value ? dr["frecuenciaAlcohol"].ToString() : "",
                                UsaDrogas = dr["usaDrogas"] != DBNull.Value && Convert.ToBoolean(dr["usaDrogas"]),
                                TipoDrogas = dr["tipoDrogas"] != DBNull.Value ? dr["tipoDrogas"].ToString() : "",
                                HaceDeporte = dr["haceDeporte"] != DBNull.Value && Convert.ToBoolean(dr["haceDeporte"]),
                                DescripcionTiempoLibre = dr["descripcionTiempoLibre"] != DBNull.Value ? dr["descripcionTiempoLibre"].ToString() : ""
                            },

                            Columna = new EvaluacionColumnaVm
                            {
                                LordosisCervical = dr["lordosisCervical"] != DBNull.Value ? (int?)Convert.ToInt32(dr["lordosisCervical"]) : null,
                                LordosisDorsal = dr["lordosisDorsal"] != DBNull.Value ? (int?)Convert.ToInt32(dr["lordosisDorsal"]) : null,
                                LordosisLumbar = dr["lordosisLumbar"] != DBNull.Value ? (int?)Convert.ToInt32(dr["lordosisLumbar"]) : null,
                                CifosisCervical = dr["cifosisCervical"] != DBNull.Value ? (int?)Convert.ToInt32(dr["cifosisCervical"]) : null,
                                CifosisDorsal = dr["cifosisDorsal"] != DBNull.Value ? (int?)Convert.ToInt32(dr["cifosisDorsal"]) : null,
                                CifosisLumbar = dr["cifosisLumbar"] != DBNull.Value ? (int?)Convert.ToInt32(dr["cifosisLumbar"]) : null,
                                EscoliosisDorsalDerecha = dr["escoliosisDorsalDerecha"] != DBNull.Value && Convert.ToBoolean(dr["escoliosisDorsalDerecha"]),
                                EscoliosisDorsalIzquierda = dr["escoliosisDorsalIzquierda"] != DBNull.Value && Convert.ToBoolean(dr["escoliosisDorsalIzquierda"]),
                                EscoliosisLumbarDerecha = dr["escoliosisLumbarDerecha"] != DBNull.Value && Convert.ToBoolean(dr["escoliosisLumbarDerecha"]),
                                EscoliosisLumbarIzquierda = dr["escoliosisLumbarIzquierda"] != DBNull.Value && Convert.ToBoolean(dr["escoliosisLumbarIzquierda"]),
                                ObservacionesColumna = dr["observacionesColumna"] != DBNull.Value ? dr["observacionesColumna"].ToString() : ""
                            }
                        };
                        
                        // Check gender details
                        if (dr["edadMenarca"] != DBNull.Value || dr["ciclos"] != DBNull.Value || dr["metodoPlanificacion"] != DBNull.Value || dr["fechaUltimaMenstruacion"] != DBNull.Value || dr["gestas"] != DBNull.Value || dr["ets"] != DBNull.Value || dr["ivsaFem"] != DBNull.Value)
                        {
                            vm.DetalleFemenino = new DetalleGinecoVm
                            {
                                EdadMenarca = Convert.ToInt32(dr["edadMenarca"]),
                                FechaUltimaMenstruacion = dr["fechaUltimaMenstruacion"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(dr["fechaUltimaMenstruacion"]) : null,
                                Ciclos = dr["ciclos"] != DBNull.Value ? dr["ciclos"].ToString() : "",
                                Gestas = dr["gestas"] != DBNull.Value ? (int?)Convert.ToInt32(dr["gestas"]) : null,
                                Partos = dr["partos"] != DBNull.Value ? (int?)Convert.ToInt32(dr["partos"]) : null,
                                Abortos = dr["abortos"] != DBNull.Value ? (int?)Convert.ToInt32(dr["abortos"]) : null,
                                Cesareas = dr["cesareas"] != DBNull.Value ? (int?)Convert.ToInt32(dr["cesareas"]) : null,
                                Ivsa = dr["ivsaFem"] != DBNull.Value ? (int?)Convert.ToInt32(dr["ivsaFem"]) : null,
                                MetodoPlanificacion = dr["metodoPlanificacion"] != DBNull.Value ? dr["metodoPlanificacion"].ToString() : "",
                                FechaUltimoPapanicolau = dr["fechaUltimoPapanicolau"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(dr["fechaUltimoPapanicolau"]) : null,
                                Ets = dr["ets"] != DBNull.Value ? dr["ets"].ToString() : ""
                            };
                        }
                        else if (dr["prepucioRetractil"] != DBNull.Value)
                        {
                            vm.DetalleMasculino = new DetalleGenitoMascVm
                            {
                                PrepucioRetractil = dr["prepucioRetractil"] != DBNull.Value && Convert.ToBoolean(dr["prepucioRetractil"]),
                                TesticulosDescendidos = dr["testiculosDescendidos"] != DBNull.Value && Convert.ToBoolean(dr["testiculosDescendidos"]),
                                Fimosis = dr["fimosis"] != DBNull.Value && Convert.ToBoolean(dr["fimosis"]),
                                Criptorquidia = dr["criptorquidia"] != DBNull.Value && Convert.ToBoolean(dr["criptorquidia"]),
                                Varicocele = dr["varicocele"] != DBNull.Value && Convert.ToBoolean(dr["varicocele"]),
                                Hidrocele = dr["hidrocele"] != DBNull.Value && Convert.ToBoolean(dr["hidrocele"]),
                                Hernia = dr["hernia"] != DBNull.Value && Convert.ToBoolean(dr["hernia"]),
                                Ivsa = dr["ivsaMasc"] != DBNull.Value ? dr["ivsaMasc"].ToString() : "",
                                Psa = dr["psa"] != DBNull.Value ? dr["psa"].ToString() : ""
                            };
                        }
                    }
                }

                if (vm != null && pkEvaluacion > 0)
                {
                    // 2. Historia Medica
                    vm.Antecedentes = new List<HistoriaMedicaVm>();
                    string sqlHist = "SELECT * FROM HistoriaMedica WHERE fkEvaluacion = @fk";
                    SqlCommand cmdHist = new SqlCommand(sqlHist, con);
                    cmdHist.Parameters.AddWithValue("@fk", pkEvaluacion);
                    using (SqlDataReader dr = cmdHist.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            vm.Antecedentes.Add(new HistoriaMedicaVm
                            {
                                Categoria = dr["categoria"].ToString(),
                                NombreCondicion = dr["nombreCondicion"].ToString(),
                                EsPositivo = Convert.ToBoolean(dr["esPositivo"]),
                                Detalles = dr["detalles"] != DBNull.Value ? dr["detalles"].ToString() : ""
                            });
                        }
                    }

                    // 3. Examen Fisico
                    vm.ExamenFisico = new List<ExamenFisicoVm>();
                    string sqlFis = "SELECT * FROM ExamenFisico WHERE fkEvaluacion = @fk";
                    SqlCommand cmdFis = new SqlCommand(sqlFis, con);
                    cmdFis.Parameters.AddWithValue("@fk", pkEvaluacion);
                    using (SqlDataReader dr = cmdFis.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            vm.ExamenFisico.Add(new ExamenFisicoVm
                            {
                                SistemaCuerpo = dr["sistemaCuerpo"].ToString(),
                                EsNormal = Convert.ToBoolean(dr["esNormal"]),
                                Hallazgos = dr["hallazgos"] != DBNull.Value ? dr["hallazgos"].ToString() : ""
                            });
                        }
                    }

                    // 4. Antecedentes Laborales
                    vm.AntecedentesLaborales = new List<AntecedenteLaboralVm>();
                    string sqlAl = "SELECT * FROM AntecedentesLaborales WHERE fkEvaluacion = @fk";
                    SqlCommand cmdAl = new SqlCommand(sqlAl, con);
                    cmdAl.Parameters.AddWithValue("@fk", pkEvaluacion);
                    using (SqlDataReader dr = cmdAl.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            vm.AntecedentesLaborales.Add(new AntecedenteLaboralVm
                            {
                                Empresa = dr["empresa"] != DBNull.Value ? dr["empresa"].ToString() : "",
                                Puesto = dr["puesto"] != DBNull.Value ? dr["puesto"].ToString() : "",
                                TiempoLaborado = dr["tiempoLaborado"] != DBNull.Value ? dr["tiempoLaborado"].ToString() : "",
                                AgentesExpuesto = dr["agentesExpuesto"] != DBNull.Value ? dr["agentesExpuesto"].ToString() : "",
                                AccidentesPrevios = dr["accidentesPrevios"] != DBNull.Value ? dr["accidentesPrevios"].ToString() : ""
                            });
                        }
                    }
                }
            }
            return vm;
        }
    }
}

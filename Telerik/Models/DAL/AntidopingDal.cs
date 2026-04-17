namespace Telerik.Models.DAL
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using Telerik.Models.ViewModels;

    public class AntidopingDal
    {
        public static void GuardarAntidoping(AntidopingVm vm)
        {
            string sqlCheck = "SELECT pkPrueba FROM PruebasToxicologicas WHERE fkOrdenMedico = @id";
            object pk = SqlHelper.ExecuteScalar(sqlCheck, new SqlParameter("@id", vm.PkOrdenMedico));

            string sql;
            var pars = new List<SqlParameter> {
                new SqlParameter("@id", vm.PkOrdenMedico),
                new SqlParameter("@cod", vm.CodigoMuestra ?? "N/A"),
                new SqlParameter("@con", vm.ConsentimientoFirmado),
                new SqlParameter("@alc", vm.AplicaAlcohol ? (object)vm.ResultadoAlcohol : DBNull.Value),
                new SqlParameter("@coc", vm.AplicaCocaina ? (object)vm.ResultadoCocaina : DBNull.Value),
                new SqlParameter("@thc", vm.AplicaTHC ? (object)vm.ResultadoTHC : DBNull.Value),
                new SqlParameter("@amp", vm.AplicaAnfetaminas ? (object)vm.ResultadoAnfetaminas : DBNull.Value),
                new SqlParameter("@met", vm.AplicaMetanfetaminas ? (object)vm.ResultadoMetanfetaminas : DBNull.Value),
                new SqlParameter("@opi", vm.AplicaOpiaceos ? (object)vm.ResultadoOpiaceos : DBNull.Value),
                new SqlParameter("@mfn", vm.AplicaMetilfenidato ? (object)vm.ResultadoMetilfenidato : DBNull.Value),
                new SqlParameter("@fen", vm.AplicaFentanilo ? (object)vm.ResultadoFentanilo : DBNull.Value),
                new SqlParameter("@bzo", vm.AplicaBenzodiacepinas ? (object)vm.ResultadoBenzodiacepinas : DBNull.Value),
                new SqlParameter("@ver", vm.VeredictoFinal ?? "NEGATIVO"),
                new SqlParameter("@com", vm.Comentarios ?? ""),
                new SqlParameter("@img", vm.UrlFotoEvidencia ?? (object)DBNull.Value)
            };

            if (pk == null)
            {
                sql = @"INSERT INTO PruebasToxicologicas (fkOrdenMedico, codigoMuestra, consentimientoFirmado, resultadoAlcohol, resultadoCocaina, resultadoTHC, 
                                                        resultadoAnfetaminas, resultadoMetanfetaminas, resultadoOpiaceos, resultadoMetilfenidato, resultadoFentanilo, 
                                                        resultadoBenzodiacepinas, veredictoFinal, comentarios, urlFotoEvidencia)
                        VALUES (@id, @cod, @con, @alc, @coc, @thc, @amp, @met, @opi, @mfn, @fen, @bzo, @ver, @com, @img)";
            }
            else
            {
                sql = @"UPDATE PruebasToxicologicas SET codigoMuestra=@cod, consentimientoFirmado=@con, resultadoAlcohol=@alc, resultadoCocaina=@coc, 
                                                       resultadoTHC=@thc, resultadoAnfetaminas=@amp, resultadoMetanfetaminas=@met, resultadoOpiaceos=@opi, 
                                                       resultadoMetilfenidato=@mfn, resultadoFentanilo=@fen, resultadoBenzodiacepinas=@bzo, 
                                                       veredictoFinal=@ver, comentarios=@com, urlFotoEvidencia=@img 
                        WHERE fkOrdenMedico=@id";
            }

            SqlHelper.ExecuteNonQuery(sql, pars.ToArray());
            
            // Actualizar Estatus de la Orden a Completado (3)
            SqlHelper.ExecuteNonQuery("UPDATE OrdenServicioMedico SET fkEstatus = 3 WHERE pkOrdenMedico = @id", new SqlParameter("@id", vm.PkOrdenMedico));
        }

        public static AntidopingVm ObtenerPorOrden(int pkOrden)
        {
            string sql = "SELECT * FROM PruebasToxicologicas WHERE fkOrdenMedico = @id";
            DataTable dt = SqlHelper.ExecuteDataTable(sql, new SqlParameter("@id", pkOrden));
            if (dt.Rows.Count == 0) return null;

            DataRow r = dt.Rows[0];
            return new AntidopingVm
            {
                PkOrdenMedico = (int)r["fkOrdenMedico"],
                CodigoMuestra = r["codigoMuestra"]?.ToString(),
                ConsentimientoFirmado = r["consentimientoFirmado"] != DBNull.Value && (bool)r["consentimientoFirmado"],
                ResultadoAlcohol = r["resultadoAlcohol"] != DBNull.Value && (bool)r["resultadoAlcohol"],
                AplicaAlcohol = r["resultadoAlcohol"] != DBNull.Value,
                ResultadoCocaina = r["resultadoCocaina"] != DBNull.Value && (bool)r["resultadoCocaina"],
                AplicaCocaina = r["resultadoCocaina"] != DBNull.Value,
                ResultadoTHC = r["resultadoTHC"] != DBNull.Value && (bool)r["resultadoTHC"],
                AplicaTHC = r["resultadoTHC"] != DBNull.Value,
                ResultadoAnfetaminas = r["resultadoAnfetaminas"] != DBNull.Value && (bool)r["resultadoAnfetaminas"],
                AplicaAnfetaminas = r["resultadoAnfetaminas"] != DBNull.Value,
                ResultadoMetanfetaminas = r["resultadoMetanfetaminas"] != DBNull.Value && (bool)r["resultadoMetanfetaminas"],
                AplicaMetanfetaminas = r["resultadoMetanfetaminas"] != DBNull.Value,
                ResultadoOpiaceos = r["resultadoOpiaceos"] != DBNull.Value && (bool)r["resultadoOpiaceos"],
                AplicaOpiaceos = r["resultadoOpiaceos"] != DBNull.Value,
                ResultadoMetilfenidato = r["resultadoMetilfenidato"] != DBNull.Value && (bool)r["resultadoMetilfenidato"],
                AplicaMetilfenidato = r["resultadoMetilfenidato"] != DBNull.Value,
                ResultadoFentanilo = r["resultadoFentanilo"] != DBNull.Value && (bool)r["resultadoFentanilo"],
                AplicaFentanilo = r["resultadoFentanilo"] != DBNull.Value,
                ResultadoBenzodiacepinas = r["resultadoBenzodiacepinas"] != DBNull.Value && (bool)r["resultadoBenzodiacepinas"],
                AplicaBenzodiacepinas = r["resultadoBenzodiacepinas"] != DBNull.Value,
                VeredictoFinal = r["veredictoFinal"]?.ToString(),
                Comentarios = r["comentarios"]?.ToString(),
                UrlFotoEvidencia = r["urlFotoEvidencia"]?.ToString()
            };
        }
    }
}
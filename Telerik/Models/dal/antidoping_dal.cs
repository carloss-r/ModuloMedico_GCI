using System;
using System.Data;
using System.Data.SqlClient;
using Telerik.Models.ViewModels;

namespace Telerik.Models.Dal
{
    public class AntidopingDal
    {
        public static void GuardarAntidoping(AntidopingVm vm)
        {
            using (SqlConnection con = ConexionBd.ObtenerConexion())
            {
                con.Open();
                SqlTransaction transaccion = con.BeginTransaction();

                try
                {
                    string sql = @"
                        INSERT INTO PruebasToxicologicas 
                        (fkOrdenMedico, codigoMuestra, consentimientoFirmado, resultadoOpiaceos, resultadoCocaina, resultadoTHC, resultadoAlcohol, veredictoFinal, comentarios, urlFotoEvidencia)
                        VALUES 
                        (@fk, @codigo, @consent, @op, @coc, @thc, 0, @veredicto, @com, @url)";
                    
                    // Note: 'resultadoAlcohol' is in DB schema but not in standard 3/5 panel usually, defaulting to 0/False or need to add to VM if required.
                    // Assumed standard 3-panel match for now based on DB columns (Opiaceos, Cocaina, THC).

                    SqlCommand cmd = new SqlCommand(sql, con, transaccion);
                    cmd.Parameters.AddWithValue("@fk", vm.PkOrdenMedico);
                    cmd.Parameters.AddWithValue("@codigo", (object)vm.CodigoMuestra ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@consent", vm.ConsentimientoFirmado);
                    cmd.Parameters.AddWithValue("@op", vm.ResultadoOpiaceos);
                    cmd.Parameters.AddWithValue("@coc", vm.ResultadoCocaina);
                    cmd.Parameters.AddWithValue("@thc", vm.ResultadoTHC);
                    cmd.Parameters.AddWithValue("@veredicto", (object)vm.VeredictoFinal ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@com", (object)vm.Comentarios ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@url", (object)vm.UrlFotoEvidencia ?? DBNull.Value);
                    
                    cmd.ExecuteNonQuery();

                    // Actualizar estatus de la orden a 'Completada' (3)
                    string sqlUpdate = "UPDATE OrdenServicioMedico SET fkEstatus = 3 WHERE pkOrdenMedico = @pk";
                    SqlCommand cmdUpd = new SqlCommand(sqlUpdate, con, transaccion);
                    cmdUpd.Parameters.AddWithValue("@pk", vm.PkOrdenMedico);
                    cmdUpd.ExecuteNonQuery();

                    transaccion.Commit();
                }
                catch (Exception)
                {
                    transaccion.Rollback();
                    throw;
                }
            }
        }
        public static AntidopingVm ObtenerPorOrden(int pkOrden)
        {
            AntidopingVm vm = null;
            using (SqlConnection con = ConexionBd.ObtenerConexion())
            {
                con.Open();
                string sql = "SELECT * FROM PruebasToxicologicas WHERE fkOrdenMedico = @pk";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@pk", pkOrden);
                
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        vm = new AntidopingVm
                        {
                            PkOrdenMedico = pkOrden,
                            CodigoMuestra = dr["codigoMuestra"].ToString(),
                            ConsentimientoFirmado = dr["consentimientoFirmado"] != DBNull.Value && Convert.ToBoolean(dr["consentimientoFirmado"]),
                            ResultadoOpiaceos = dr["resultadoOpiaceos"] != DBNull.Value && Convert.ToBoolean(dr["resultadoOpiaceos"]),
                            ResultadoCocaina = dr["resultadoCocaina"] != DBNull.Value && Convert.ToBoolean(dr["resultadoCocaina"]),
                            ResultadoTHC = dr["resultadoTHC"] != DBNull.Value && Convert.ToBoolean(dr["resultadoTHC"]),
                            ResultadoAnfetaminas = HasColumn(dr, "resultadoAnfetaminas") ? (dr["resultadoAnfetaminas"] != DBNull.Value ? Convert.ToBoolean(dr["resultadoAnfetaminas"]) : false) : false,
                            ResultadoMetanfetaminas = HasColumn(dr, "resultadoMetanfetaminas") ? (dr["resultadoMetanfetaminas"] != DBNull.Value ? Convert.ToBoolean(dr["resultadoMetanfetaminas"]) : false) : false,
                            VeredictoFinal = dr["veredictoFinal"].ToString(), 
                            Comentarios = dr["comentarios"] != DBNull.Value ? dr["comentarios"].ToString() : "",
                            UrlFotoEvidencia = dr["urlFotoEvidencia"] != DBNull.Value ? dr["urlFotoEvidencia"].ToString() : ""
                        };
                    }
                }
            }
            return vm;
        }

        private static bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}

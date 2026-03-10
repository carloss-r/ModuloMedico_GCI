using System;
using Telerik.Models.Entities;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Telerik.Models.Dal
{
    public class ConexionBd
    {
        private static string _connectionString;

        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    _connectionString = ConfigurationManager.ConnectionStrings["GCI_ModuloMedico"].ConnectionString;
                }
                return _connectionString;
            }
        }

        public static SqlConnection ObtenerConexion()
        {
            return new SqlConnection(ConnectionString);
        }

        public static DataTable EjecutarConsulta(string query, params SqlParameter[] parametros)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = ObtenerConexion())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandTimeout = 15; // 15 seconds max
                    if (parametros != null)
                    {
                        cmd.Parameters.AddRange(parametros);
                    }
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public static int EjecutarComando(string query, params SqlParameter[] parametros)
        {
            using (SqlConnection conn = ObtenerConexion())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parametros != null)
                    {
                        cmd.Parameters.AddRange(parametros);
                    }
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static object EjecutarEscalar(string query, params SqlParameter[] parametros)
        {
            using (SqlConnection conn = ObtenerConexion())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parametros != null)
                    {
                        cmd.Parameters.AddRange(parametros);
                    }
                    conn.Open();
                    return cmd.ExecuteScalar();
                }
            }
        }
    }
}

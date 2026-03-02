using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Telerik.Models.Dal
{
    public class CatalogoDal
    {
        public static List<CatalogoItem> ObtenerTiposServicio()
        {
            string query = "SELECT pkTipoServicio AS Id, descripcion AS Descripcion FROM TiposServicio ORDER BY descripcion";
            return MapearCatalogo(ConexionBd.EjecutarConsulta(query));
        }

        public static List<CatalogoItem> ObtenerEstatusSolicitud()
        {
            string query = "SELECT pkEstatus AS Id, descripcion AS Descripcion FROM EstatusSolicitud ORDER BY pkEstatus";
            return MapearCatalogo(ConexionBd.EjecutarConsulta(query));
        }

        public static List<CatalogoItem> ObtenerProyectos()
        {
            string query = "SELECT pkProyecto AS Id, descripcion AS Descripcion FROM Proyectos ORDER BY descripcion";
            return MapearCatalogo(ConexionBd.EjecutarConsulta(query));
        }

        /// <summary>
        /// Obtiene todas las empresas del catálogo.
        /// La tabla Empresas usa 'nombre' como campo descriptivo.
        /// </summary>
        public static List<CatalogoItem> ObtenerEmpresas()
        {
            string query = "SELECT pkEmpresa AS Id, nombre AS Descripcion FROM Empresas ORDER BY nombre";
            return MapearCatalogo(ConexionBd.EjecutarConsulta(query));
        }

        /// <summary>
        /// Obtiene tipos de servicio y empresas en una sola llamada a la BD.
        /// Devuelve un objeto dinámico con TiposServicio y Empresas.
        /// </summary>
        public static void ObtenerCatalogosParaSolicitud(out List<CatalogoItem> tiposServicio, out List<CatalogoItem> empresas)
        {
            string query = @"
                SELECT pkTipoServicio AS Id, descripcion AS Descripcion FROM TiposServicio ORDER BY descripcion;
                SELECT pkEmpresa AS Id, nombre AS Descripcion FROM Empresas ORDER BY nombre;";

            DataSet ds = new DataSet();
            using (System.Data.SqlClient.SqlConnection conn = ConexionBd.ObtenerConexion())
            {
                using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(query, conn))
                {
                    cmd.CommandTimeout = 15;
                    using (System.Data.SqlClient.SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                    {
                        da.Fill(ds);
                    }
                }
            }

            tiposServicio = MapearCatalogo(ds.Tables[0]);
            empresas = MapearCatalogo(ds.Tables[1]);
        }
        public static List<CatalogoItem> ObtenerProyectosPorEmpresa(int fkEmpresa)
        {
            string query = "SELECT pkProyecto AS Id, descripcion AS Descripcion FROM Proyectos WHERE fkEmpresa = @fkEmpresa ORDER BY descripcion";
            return MapearCatalogo(ConexionBd.EjecutarConsulta(query, new SqlParameter("@fkEmpresa", fkEmpresa)));
        }

        /// <summary>
        /// Obtiene puestos filtrados por empresa.
        /// La tabla Puesto tiene fkEmpresa directamente.
        /// </summary>
        public static List<CatalogoItem> ObtenerPuestosPorEmpresa(int fkEmpresa)
        {
            string query = "SELECT pkPuesto AS Id, descripcion AS Descripcion FROM Puesto WHERE fkEmpresa = @fkEmpresa ORDER BY descripcion";
            return MapearCatalogo(ConexionBd.EjecutarConsulta(query, new SqlParameter("@fkEmpresa", fkEmpresa)));
        }

        private static List<CatalogoItem> MapearCatalogo(DataTable dt)
        {
            var lista = new List<CatalogoItem>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new CatalogoItem
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Descripcion = row["Descripcion"] != DBNull.Value ? row["Descripcion"].ToString().Trim() : ""
                });
            }
            return lista;
        }
    }

    public class CatalogoItem
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Telerik.Models.ViewModels;

namespace Telerik.Models.DAL
{
    public class CatalogoDal
    {
        public static List<CatalogoItem> ObtenerTiposServicio()
        {
            // [Table("TiposServicio")]
            string sql = "SELECT pkTipoServicio as Id, descripcion as Descripcion FROM TiposServicio ORDER BY descripcion";
            DataTable dt = SqlHelper.ExecuteDataTable(sql);
            return MapCatalogo(dt);
        }

        public static List<CatalogoItem> ObtenerEstatusOrdenes()
        {
            // [Table("EstatusSolicitud")]
            string sql = "SELECT pkEstatus as Id, descripcion as Descripcion FROM EstatusSolicitud ORDER BY pkEstatus";
            DataTable dt = SqlHelper.ExecuteDataTable(sql);
            return MapCatalogo(dt);
        }

        public static List<CatalogoItem> ObtenerProyectos()
        {
            // [Table("Proyectos")]
            string sql = "SELECT pkProyecto as Id, descripcion as Descripcion FROM Proyectos ORDER BY descripcion";
            DataTable dt = SqlHelper.ExecuteDataTable(sql);
            return MapCatalogo(dt);
        }

        public static List<CatalogoItem> ObtenerEmpresas()
        {
            // [Table("Empresas")]
            string sql = "SELECT pkEmpresa as Id, nombre as Descripcion FROM Empresas ORDER BY nombre";
            DataTable dt = SqlHelper.ExecuteDataTable(sql);
            return MapCatalogo(dt);
        }

        public static List<CatalogoItem> ObtenerPuestos()
        {
            // [Table("Puesto")]
            string sql = "SELECT pkPuesto as Id, descripcion as Descripcion FROM Puesto ORDER BY descripcion";
            DataTable dt = SqlHelper.ExecuteDataTable(sql);
            return MapCatalogo(dt);
        }

        public static List<CatalogoItem> ObtenerAreas()
        {
            // [Table("Areas")]
            string sql = "SELECT pkArea as Id, descripcion as Descripcion FROM Areas ORDER BY descripcion";
            DataTable dt = SqlHelper.ExecuteDataTable(sql);
            return MapCatalogo(dt);
        }

        public static void ObtenerCatalogosParaSolicitud(out List<CatalogoItem> tiposServicio, out List<CatalogoItem> empresas)
        {
            tiposServicio = ObtenerTiposServicio();
            empresas      = ObtenerEmpresas();
        }

        public static List<CatalogoItem> ObtenerProyectosPorEmpresa(int fkEmpresa)
        {
            string sql = "SELECT pkProyecto as Id, descripcion as Descripcion FROM Proyectos WHERE fkEmpresa = @id ORDER BY descripcion";
            DataTable dt = SqlHelper.ExecuteDataTable(sql, new SqlParameter("@id", fkEmpresa));
            return MapCatalogo(dt);
        }

        public static List<CatalogoItem> ObtenerDepartamentosPorEmpresa(int fkEmpresa)
        {
            string sql = "SELECT pkDepartamento as Id, descripcion as Descripcion FROM Departamentos WHERE fkEmpresa = @id AND (regActivo = 1 OR regActivo IS NULL) ORDER BY descripcion";
            DataTable dt = SqlHelper.ExecuteDataTable(sql, new SqlParameter("@id", fkEmpresa));
            return MapCatalogo(dt);
        }

        public static List<CatalogoItem> ObtenerAreasPorDepartamento(int fkDepartamento)
        {
            string sql = "SELECT pkArea as Id, descripcion as Descripcion FROM Areas WHERE fkDepartamento = @id ORDER BY descripcion";
            DataTable dt = SqlHelper.ExecuteDataTable(sql, new SqlParameter("@id", fkDepartamento));
            return MapCatalogo(dt);
        }

        public static List<CatalogoItem> ObtenerPuestosPorArea(int fkArea)
        {
            string sql = "SELECT pkPuesto as Id, descripcion as Descripcion FROM Puesto WHERE fkArea = @id ORDER BY descripcion";
            DataTable dt = SqlHelper.ExecuteDataTable(sql, new SqlParameter("@id", fkArea));
            return MapCatalogo(dt);
        }

        public static List<CatalogoItem> ObtenerPuestosPorEmpresa(int fkEmpresa)
        {
            string sql = "SELECT pkPuesto as Id, descripcion as Descripcion FROM Puesto WHERE fkEmpresa = @id ORDER BY descripcion";
            DataTable dt = SqlHelper.ExecuteDataTable(sql, new SqlParameter("@id", fkEmpresa));
            return MapCatalogo(dt);
        }

        private static List<CatalogoItem> MapCatalogo(DataTable dt)
        {
            List<CatalogoItem> list = new List<CatalogoItem>();
            foreach (DataRow r in dt.Rows)
            {
                list.Add(new CatalogoItem { 
                    Id = (int)r["Id"], 
                    Descripcion = r["Descripcion"].ToString() 
                });
            }
            return list;
        }
    }
}

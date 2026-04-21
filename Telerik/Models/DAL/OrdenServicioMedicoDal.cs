using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Telerik.Models.ViewModels;
using System.Linq;

namespace Telerik.Models.DAL
{
    public class OrdenServicioMedicoDal
    {
        private const string SQL_SELECT = @"
            SELECT
                o.pkOrdenMedico,
                o.fkEmpleado,
                o.fkCandidato,
                o.fkProyecto,
                o.fkTipoServicio,
                o.fkEstatus,
                o.fechaOrden,
                ISNULL(ts.descripcion, '') AS TipoServicioDesc,
                ISNULL(es.descripcion, '') AS EstatusDesc,
                COALESCE(
                    NULLIF(RTRIM(ISNULL(e.nombre,'') + ' ' + ISNULL(e.aPaterno,'') + ' ' + ISNULL(e.aMaterno,'')), ''),
                    NULLIF(RTRIM(ISNULL(c.nombre,'') + ' ' + ISNULL(c.aPaterno,'') + ' ' + ISNULL(c.aMaterno,'')), ''),
                    'S/N'
                ) AS NombreCompleto,
                COALESCE(p.descripcion, c.puestoDeseado, '') AS PuestoDesc,
                COALESCE(emp_dir.nombre, emp_pro.nombre, c.empresa, '') AS EmpresaDesc,
                COALESCE(pro.descripcion, pro_e.descripcion, '') AS ProyectoDesc,
                COALESCE(e.fkSexo, c.fkSexo, '') AS SexoPersona,
                CASE WHEN o.fkCandidato IS NOT NULL THEN 'INGRESO' ELSE 'PERIODICO' END AS Modalidad,
                (SELECT COUNT(1) FROM EvaluacionesClinicas WHERE fkOrdenMedico = o.pkOrdenMedico) as HasEval,
                (SELECT COUNT(1) FROM PruebasToxicologicas WHERE fkOrdenMedico = o.pkOrdenMedico) as HasAnti
            FROM OrdenServicioMedico o
            LEFT JOIN TiposServicio     ts      ON ts.pkTipoServicio  = o.fkTipoServicio
            LEFT JOIN EstatusSolicitud  es      ON es.pkEstatus       = o.fkEstatus
            LEFT JOIN Empleados         e       ON e.pkEmpleado       = o.fkEmpleado
            LEFT JOIN Candidatos        c       ON c.pkCandidato      = o.fkCandidato
            LEFT JOIN Puesto            p       ON p.pkPuesto         = e.fkPuesto
            LEFT JOIN Proyectos         pro     ON pro.pkProyecto     = o.fkProyecto
            LEFT JOIN Proyectos         pro_e   ON pro_e.pkProyecto   = e.fkProyecto
            LEFT JOIN Empresas          emp_dir ON emp_dir.pkEmpresa  = e.fkEmpresa
            LEFT JOIN Empresas          emp_pro ON emp_pro.pkEmpresa  = pro.fkEmpresa
        ";

        public static List<OrdenServicioMedicoVm> ObtenerTodas(
            out int totalCount, 
            int pageIndex = 1, 
            int pageSize = 25,
            int? filtroNumEmpleado = null, 
            string modalidad = null,
            int? fkEstatus = null, 
            DateTime? fechaDesde = null, 
            DateTime? fechaHasta = null,
            int? fkEmpresa = null, 
            int? fkArea = null, 
            int? anio = null, 
            int? semana = null, 
            string filtroNombre = null)
        {
            string where = " WHERE 1=1 ";
            
            if (filtroNumEmpleado.HasValue) 
                where += " AND (o.fkEmpleado = @fNum OR e.pkEmpleado = @fNum OR o.pkOrdenMedico = @fNum) ";
            
            if (!string.IsNullOrEmpty(filtroNombre)) 
                where += " AND (e.nombre LIKE @fNm OR e.aPaterno LIKE @fNm OR c.nombre LIKE @fNm OR c.aPaterno LIKE @fNm) ";
            
            if (!string.IsNullOrEmpty(modalidad)) {
                if (modalidad == "INGRESO") where += " AND o.fkEmpleado IS NULL ";
                else if (modalidad == "PERIODICO") where += " AND o.fkEmpleado IS NOT NULL ";
            }

            if (fkEstatus.HasValue && fkEstatus.Value != 0) {
                if (fkEstatus.Value == -1) where += " AND o.fkEstatus != 3 ";
                else where += " AND o.fkEstatus = @est ";
            }

            if (fechaDesde.HasValue) where += " AND CAST(o.fechaOrden AS DATE) >= @f1 ";
            if (fechaHasta.HasValue) where += " AND CAST(o.fechaOrden AS DATE) <= @f2 ";

            // Nuevos filtros que faltaban en la versión anterior por error
            if (fkEmpresa.HasValue && fkEmpresa.Value > 0) where += " AND (e.fkEmpresa = @fEmp OR pro.fkEmpresa = @fEmp) ";
            if (fkArea.HasValue && fkArea.Value > 0) where += " AND (e.fkArea = @fArea) "; // Nota: Ajustar según esquema si Candidatos tienen área

            SqlParameter[] GetParams() {
                var p = new List<SqlParameter>();
                if (filtroNumEmpleado.HasValue) p.Add(new SqlParameter("@fNum", filtroNumEmpleado.Value));
                if (!string.IsNullOrEmpty(filtroNombre)) p.Add(new SqlParameter("@fNm", "%" + filtroNombre + "%"));
                if (fkEstatus.HasValue && fkEstatus.Value != 0 && fkEstatus.Value != -1) p.Add(new SqlParameter("@est", fkEstatus.Value));
                if (fechaDesde.HasValue) p.Add(new SqlParameter("@f1", fechaDesde.Value.Date));
                if (fechaHasta.HasValue) p.Add(new SqlParameter("@f2", fechaHasta.Value.Date));
                if (fkEmpresa.HasValue && fkEmpresa.Value > 0) p.Add(new SqlParameter("@fEmp", fkEmpresa.Value));
                if (fkArea.HasValue && fkArea.Value > 0) p.Add(new SqlParameter("@fArea", fkArea.Value));
                return p.ToArray();
            }

            string sqlCount = @"SELECT COUNT(1) FROM OrdenServicioMedico o 
                               LEFT JOIN Empleados e ON e.pkEmpleado = o.fkEmpleado 
                               LEFT JOIN Candidatos c ON c.pkCandidato = o.fkCandidato 
                               LEFT JOIN Proyectos pro ON pro.pkProyecto = o.fkProyecto " + where;
            
            totalCount = Convert.ToInt32(SqlHelper.ExecuteScalar(sqlCount, GetParams()));

            string sqlData = SQL_SELECT + where + " ORDER BY ISNULL((SELECT MAX(fechaEvaluacion) FROM EvaluacionesClinicas WHERE fkOrdenMedico = o.pkOrdenMedico), o.fechaOrden) DESC, o.pkOrdenMedico DESC OFFSET @off ROWS FETCH NEXT @top ROWS ONLY";
            var parsData = GetParams().ToList();
            parsData.Add(new SqlParameter("@off", (pageIndex - 1) * pageSize));
            parsData.Add(new SqlParameter("@top", pageSize));

            DataTable dt = SqlHelper.ExecuteDataTable(sqlData, parsData.ToArray());
            var list = new List<OrdenServicioMedicoVm>();
            foreach (DataRow r in dt.Rows) list.Add(MapSingle(r));
            return list;
        }

        public static OrdenServicioMedicoVm ObtenerPorId(int pkOrden)
        {
            DataTable dt = SqlHelper.ExecuteDataTable(SQL_SELECT + " WHERE o.pkOrdenMedico = @id", new SqlParameter("@id", pkOrden));
            if (dt.Rows.Count == 0) return null;

            var vm = MapSingle(dt.Rows[0]);
            
            var evalDt = SqlHelper.ExecuteDataTable("SELECT fkAptitudMedica, recomendaciones FROM EvaluacionesClinicas WHERE fkOrdenMedico = @id", new SqlParameter("@id", pkOrden));
            if (evalDt.Rows.Count > 0) {
                vm.FkAptitudMedica = evalDt.Rows[0]["fkAptitudMedica"] != DBNull.Value ? (int?)evalDt.Rows[0]["fkAptitudMedica"] : null;
                vm.Recomendaciones = evalDt.Rows[0]["recomendaciones"]?.ToString();
            }

            return vm;
        }

        public static int Insertar(int? fkEmpleado, int? fkCandidato, int? fkProyecto, int fkTipoServicio)
        {
            return Insertar(new OrdenServicioMedicoVm {
                FkEmpleado = fkEmpleado,
                FkCandidato = fkCandidato,
                FkProyecto = fkProyecto,
                FkTipoServicio = fkTipoServicio,
                FkEstatus = 1,
                FechaOrden = DateTime.Now
            });
        }

        public static int Insertar(OrdenServicioMedicoVm vm)
        {
            string sql = @"
                INSERT INTO OrdenServicioMedico (fkEmpleado, fkCandidato, fkProyecto, fkTipoServicio, fkEstatus, fechaOrden)
                VALUES (@emp, @can, @pro, @tip, @est, @fec);
                SELECT SCOPE_IDENTITY();";

            var pars = new List<SqlParameter> {
                new SqlParameter("@emp", (object)vm.FkEmpleado ?? DBNull.Value),
                new SqlParameter("@can", (object)vm.FkCandidato ?? DBNull.Value),
                new SqlParameter("@pro", (object)vm.FkProyecto ?? DBNull.Value),
                new SqlParameter("@tip", vm.FkTipoServicio),
                new SqlParameter("@est", (object)vm.FkEstatus ?? DBNull.Value),
                new SqlParameter("@fec", vm.FechaOrden ?? DateTime.Now)
            };

            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql, pars.ToArray()));
        }

        public static void ActualizarEstatus(int pkOrden, int fkEstatus)
        {
            string sql = "UPDATE OrdenServicioMedico SET fkEstatus = @est WHERE pkOrdenMedico = @id";
            SqlHelper.ExecuteNonQuery(sql, new SqlParameter("@est", fkEstatus), new SqlParameter("@id", pkOrden));
        }

        public static void Eliminar(int pkOrden)
        {
            string sql = "DELETE FROM OrdenServicioMedico WHERE pkOrdenMedico = @id";
            SqlHelper.ExecuteNonQuery(sql, new SqlParameter("@id", pkOrden));
        }

        private static OrdenServicioMedicoVm MapSingle(DataRow r)
        {
            bool HasCol(string col) => r.Table.Columns.Contains(col) && r[col] != DBNull.Value;

            return new OrdenServicioMedicoVm
            {
                PkOrdenMedico   = (int)r["pkOrdenMedico"],
                FkEmpleado      = HasCol("fkEmpleado") ? (int?)r["fkEmpleado"] : null,
                FkCandidato     = HasCol("fkCandidato")? (int?)r["fkCandidato"] : null,
                FkProyecto      = HasCol("fkProyecto") ? (int?)r["fkProyecto"] : null,
                FkTipoServicio  = (int)r["fkTipoServicio"],
                FkEstatus       = HasCol("fkEstatus")  ? (int?)r["fkEstatus"] : null,
                FechaOrden      = HasCol("fechaOrden") ? (DateTime?)r["fechaOrden"] : null,
                NombrePersona   = r["NombreCompleto"]?.ToString(),
                Modalidad       = HasCol("Modalidad") ? r["Modalidad"]?.ToString() : "INGRESO",
                PuestoCandidato = r["PuestoDesc"]?.ToString(),
                EmpresaCandidato= r["EmpresaDesc"]?.ToString(),
                EmpresaNombre   = r["EmpresaDesc"]?.ToString(),
                ProyectoDesc    = r["ProyectoDesc"]?.ToString(),
                TipoServicioDesc= r["TipoServicioDesc"]?.ToString(),
                EstatusDesc     = r["EstatusDesc"]?.ToString(),
                SexoCandidato   = HasCol("SexoPersona") ? r["SexoPersona"].ToString() : "",
                TieneEvaluacion = HasCol("HasEval") && Convert.ToInt32(r["HasEval"]) > 0,
                TieneAntidoping = HasCol("HasAnti") && Convert.ToInt32(r["HasAnti"]) > 0
            };
        }
    }
}

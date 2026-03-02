using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Telerik.Models.ViewModels;

namespace Telerik.Models.Dal
{
    public class OrdenServicioMedicoDal
    {
        public static List<OrdenServicioMedicoVm> ObtenerTodas()
        {
            string query = @"
                SELECT TOP 50 
                    o.pkOrdenMedico,
                    o.fkEmpleado,
                    o.fkCandidato,
                    o.fkProyecto,
                    o.fkTipoServicio,
                    o.fkEstatus,
                    o.fechaOrden,
                    CASE 
                        WHEN o.fkEmpleado IS NOT NULL THEN e.nombre + ' ' + ISNULL(e.aPaterno, '') + ' ' + ISNULL(e.aMaterno, '')
                        WHEN o.fkCandidato IS NOT NULL THEN c.nombre + ' ' + ISNULL(c.aPaterno, '') + ' ' + ISNULL(c.aMaterno, '')
                        ELSE ''
                    END AS nombrePersona,
                    CASE 
                        WHEN o.fkEmpleado IS NOT NULL THEN 'PERIODICO'
                        ELSE 'INGRESO'
                    END AS modalidad,
                    ts.descripcion AS tipoServicioDesc,
                    es.descripcion AS estatusDesc,
                    COALESCE(p.descripcion, pEmp.descripcion) AS proyectoDesc
                FROM OrdenServicioMedico o
                LEFT JOIN Empleados e ON o.fkEmpleado = e.pkEmpleado
                LEFT JOIN Candidatos c ON o.fkCandidato = c.pkCandidato
                LEFT JOIN TiposServicio ts ON o.fkTipoServicio = ts.pkTipoServicio
                LEFT JOIN EstatusSolicitud es ON o.fkEstatus = es.pkEstatus
                LEFT JOIN Proyectos p ON o.fkProyecto = p.pkProyecto
                LEFT JOIN Proyectos pEmp ON e.fkProyecto = pEmp.pkProyecto
                ORDER BY o.fechaOrden DESC";

            DataTable dt = ConexionBd.EjecutarConsulta(query);
            var lista = new List<OrdenServicioMedicoVm>();

            foreach (DataRow row in dt.Rows)
            {
                lista.Add(MapearDesdeRow(row));
            }

            return lista;
        }

        public static OrdenServicioMedicoVm ObtenerPorId(int pkOrdenMedico)
        {
            string query = @"
                SELECT 
                    o.pkOrdenMedico,
                    o.fkEmpleado,
                    o.fkCandidato,
                    o.fkProyecto,
                    o.fkTipoServicio,
                    o.fkEstatus,
                    o.fechaOrden,
                    CASE 
                        WHEN o.fkEmpleado IS NOT NULL THEN e.nombre + ' ' + ISNULL(e.aPaterno, '') + ' ' + ISNULL(e.aMaterno, '')
                        WHEN o.fkCandidato IS NOT NULL THEN c.nombre + ' ' + ISNULL(c.aPaterno, '') + ' ' + ISNULL(c.aMaterno, '')
                        ELSE ''
                    END AS nombrePersona,
                    CASE 
                        WHEN o.fkEmpleado IS NOT NULL THEN 'PERIODICO'
                        ELSE 'INGRESO'
                    END AS modalidad,
                    ts.descripcion AS tipoServicioDesc,
                    es.descripcion AS estatusDesc,
                    p.descripcion AS proyectoDesc,
                    emp2.nombre   AS empresaNombre,
                    c.puestoDeseado,
                    c.area AS areaCandidato,
                    c.empresa AS empresaCandidato,
                    c.fkSexo AS sexoCandidato,
                    DATEDIFF(YEAR, c.fechaNacimiento, GETDATE()) AS edadCandidato
                FROM OrdenServicioMedico o
                LEFT JOIN Empleados e ON o.fkEmpleado = e.pkEmpleado
                LEFT JOIN Candidatos c ON o.fkCandidato = c.pkCandidato
                LEFT JOIN TiposServicio ts ON o.fkTipoServicio = ts.pkTipoServicio
                LEFT JOIN EstatusSolicitud es ON o.fkEstatus = es.pkEstatus
                LEFT JOIN Proyectos p ON o.fkProyecto = p.pkProyecto
                LEFT JOIN Empresas emp2 ON ISNULL(p.fkEmpresa,
                    (SELECT TOP 1 pr2.fkEmpresa FROM Proyectos pr2 WHERE pr2.pkProyecto = e.fkProyecto)
                ) = emp2.pkEmpresa
                WHERE o.pkOrdenMedico = @pk";

            DataTable dt = ConexionBd.EjecutarConsulta(query, new SqlParameter("@pk", pkOrdenMedico));

            if (dt.Rows.Count > 0)
            {
                return MapearDesdeRow(dt.Rows[0]);
            }

            return null;
        }

        public static int Insertar(int? fkEmpleado, int? fkCandidato, int? fkProyecto, int fkTipoServicio)
        {
            string query = @"
                INSERT INTO OrdenServicioMedico (fkEmpleado, fkCandidato, fkProyecto, fkTipoServicio, fkEstatus, fechaOrden)
                VALUES (@fkEmpleado, @fkCandidato, @fkProyecto, @fkTipoServicio, 1, GETDATE());
                SELECT SCOPE_IDENTITY();";

            var parametros = new[]
            {
                new SqlParameter("@fkEmpleado", (object)fkEmpleado ?? DBNull.Value),
                new SqlParameter("@fkCandidato", (object)fkCandidato ?? DBNull.Value),
                new SqlParameter("@fkProyecto", (object)fkProyecto ?? DBNull.Value),
                new SqlParameter("@fkTipoServicio", fkTipoServicio)
            };

            object resultado = ConexionBd.EjecutarEscalar(query, parametros);
            return Convert.ToInt32(resultado);
        }

        public static void ActualizarEstatus(int pkOrdenMedico, int fkEstatus)
        {
            string query = "UPDATE OrdenServicioMedico SET fkEstatus = @fkEstatus WHERE pkOrdenMedico = @pk";

            var parametros = new[]
            {
                new SqlParameter("@fkEstatus", fkEstatus),
                new SqlParameter("@pk", pkOrdenMedico)
            };

            ConexionBd.EjecutarComando(query, parametros);
        }

        private static OrdenServicioMedicoVm MapearDesdeRow(DataRow row)
        {
            return new OrdenServicioMedicoVm
            {
                PkOrdenMedico = Convert.ToInt32(row["pkOrdenMedico"]),
                FkEmpleado = row["fkEmpleado"] != DBNull.Value ? (int?)Convert.ToInt32(row["fkEmpleado"]) : null,
                FkCandidato = row["fkCandidato"] != DBNull.Value ? (int?)Convert.ToInt32(row["fkCandidato"]) : null,
                FkProyecto = row["fkProyecto"] != DBNull.Value ? (int?)Convert.ToInt32(row["fkProyecto"]) : null,
                FkTipoServicio = Convert.ToInt32(row["fkTipoServicio"]),
                FkEstatus = row["fkEstatus"] != DBNull.Value ? (int?)Convert.ToInt32(row["fkEstatus"]) : null,
                FechaOrden = row["fechaOrden"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["fechaOrden"]) : null,
                NombrePersona = row["nombrePersona"].ToString().Trim(),
                Modalidad = row["modalidad"].ToString(),
                TipoServicioDesc = row["tipoServicioDesc"].ToString(),
                EstatusDesc = row["estatusDesc"].ToString(),
                ProyectoDesc = row["proyectoDesc"] != DBNull.Value ? row["proyectoDesc"].ToString() : "",
                EmpresaNombre = row.Table.Columns.Contains("empresaNombre") && row["empresaNombre"] != DBNull.Value ? row["empresaNombre"].ToString().Trim() : "",
                
                PuestoCandidato = row.Table.Columns.Contains("puestoDeseado") && row["puestoDeseado"] != DBNull.Value ? row["puestoDeseado"].ToString() : "",
                AreaCandidato = row.Table.Columns.Contains("areaCandidato") && row["areaCandidato"] != DBNull.Value ? row["areaCandidato"].ToString() : "",
                EmpresaCandidato = row.Table.Columns.Contains("empresaCandidato") && row["empresaCandidato"] != DBNull.Value ? row["empresaCandidato"].ToString() : "",
                SexoCandidato = row.Table.Columns.Contains("sexoCandidato") && row["sexoCandidato"] != DBNull.Value ? row["sexoCandidato"].ToString() : "",
                EdadCandidato = row.Table.Columns.Contains("edadCandidato") && row["edadCandidato"] != DBNull.Value ? row["edadCandidato"].ToString() : ""
        };
        }

        public static void Eliminar(int pkOrdenMedico)
        {
            string query = "DELETE FROM OrdenServicioMedico WHERE pkOrdenMedico = @pk";
            ConexionBd.EjecutarComando(query, new SqlParameter("@pk", pkOrdenMedico));
        }
    }
}

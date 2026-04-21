using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using Telerik.Models.DAL;
using Telerik.Models.ViewModels;
using Telerik.Models.Entities;
using Telerik.Models;

namespace Telerik
{
    public partial class DashboardRecursosHumanosSM : System.Web.UI.Page
    {

        private static int ResolverTipoServicioIngreso()
        {
            using (var db = new ApplicationDbContext())
            {
                var tipo = db.TiposServicio
                    .Where(t => t.descripcion != null)
                    .OrderBy(t => t.pkTipoServicio)
                    .FirstOrDefault(t => t.descripcion.ToUpper().Contains("INGRESO"));

                if (tipo != null) return tipo.pkTipoServicio;

                return db.TiposServicio
                    .Where(t => t.descripcion != null)
                    .OrderBy(t => t.pkTipoServicio)
                    .FirstOrDefault(t => t.descripcion.ToUpper().Contains("EXAMEN") || t.descripcion.ToUpper().Contains("MEDICO"))?.pkTipoServicio ?? 1;
            }
        }

        private static int ResolverTipoServicioPeriodico()
        {
            using (var db = new ApplicationDbContext())
            {
                // Prioridad 1: Periódico
                var tipo = db.TiposServicio
                    .Where(t => t.descripcion != null)
                    .OrderBy(t => t.pkTipoServicio)
                    .FirstOrDefault(t => t.descripcion.ToUpper().Contains("PERI"));

                if (tipo != null) return tipo.pkTipoServicio;

                // Prioridad 2: Médico / Examen (si no hay específico de periódico)
                return db.TiposServicio
                    .Where(t => t.descripcion != null)
                    .OrderBy(t => t.pkTipoServicio)
                    .FirstOrDefault(t => t.descripcion.ToUpper().Contains("EXAMEN") || t.descripcion.ToUpper().Contains("MEDICO"))?.pkTipoServicio ?? 2;
            }
        }

        private static bool EsVisibleEnDashboardRh(OrdenServicioMedicoVm orden)
        {
            if (orden == null) return false;
            // RRHH solo gestiona Ingresos (candidatos) y Periódicos (empleados)
            string mod = (orden.Modalidad ?? "").ToUpperInvariant();
            return mod == "INGRESO" || mod == "PERIODICO";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // WebForms Page Load
        }

        [WebMethod]
        public static object CargarInicial()
        {
            try
            {
                int totalRegistros;
                var solicitudes = OrdenServicioMedicoDal.ObtenerTodas(out totalRegistros, 1, 10, null, "INGRESO");

                List<CatalogoItem> tiposServicio;
                List<CatalogoItem> empresas;
                CatalogoDal.ObtenerCatalogosParaSolicitud(out tiposServicio, out empresas);

                return new
                {
                    success = true,
                    data = solicitudes,
                    total = totalRegistros,
                    tiposServicio = tiposServicio,
                    empresas = empresas
                };
            }
            catch (Exception ex)
            {
                return new { success = false, message = "Error al cargar datos iniciales: " + ex.Message };
            }
        }

        [WebMethod]
        public static object CargarPagina(
            int? pagina,
            int tamanoPagina,
            int? filtroNumEmpleado,
            string filtroModalidad,
            int? filtroEstatus,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int? filtroEmpresa,
            int? filtroArea,
            int? filtroAnio,
            int? filtroSemana,
            string filtroNombre)
        {
            try
            {
                int totalRegistros;
                var solicitudes = OrdenServicioMedicoDal.ObtenerTodas(
                    out totalRegistros,
                    pagina ?? 1,
                    tamanoPagina,
                    filtroNumEmpleado,
                    filtroModalidad,
                    filtroEstatus,
                    fechaDesde,
                    fechaHasta,
                    filtroEmpresa,
                    filtroArea,
                    filtroAnio,
                    filtroSemana,
                    filtroNombre
                );

                return new { success = true, data = solicitudes, total = totalRegistros };
            }
            catch (Exception ex)
            {
                return new { success = false, message = "Error al cargar la página: " + ex.Message };
            }
        }

        [WebMethod]
        public static object VerDetalle(int id)
        {
            try
            {
                var orden = OrdenServicioMedicoDal.ObtenerPorId(id);
                if (orden == null)
                {
                    return new { success = false, message = "Solicitud no encontrada." };
                }

                if (!EsVisibleEnDashboardRh(orden))
                {
                    return new { success = false, message = "Esta solicitud no está disponible en el dashboard de Recursos Humanos." };
                }

                object datosEmpleado = null;
                if (orden.FkEmpleado.HasValue)
                {
                    var emp = EmpleadoDal.BuscarPorNumero(orden.FkEmpleado.Value);
                    if (emp != null)
                    {
                        datosEmpleado = new
                        {
                            emp.PkEmpleado,
                            emp.Nombre,
                            emp.APaterno,
                            emp.AMaterno,
                            emp.NombreCompleto,
                            emp.Rfc,
                            emp.Curp,
                            emp.Nss,
                            emp.PuestoDesc,
                            emp.ProyectoDesc
                        };
                    }
                }

                return new
                {
                    success = true,
                    orden = new
                    {
                        orden.PkOrdenMedico,
                        orden.FolioDisplay,
                        orden.FkEmpleado,
                        orden.FkCandidato,
                        orden.Modalidad,
                        orden.TipoServicioDesc,
                        orden.EstatusDesc,
                        orden.FechaOrdenFormateada,
                        orden.NombrePersona,
                        orden.ProyectoDesc,
                        EmpresaNombre = !string.IsNullOrEmpty(orden.EmpresaCandidato) ? orden.EmpresaCandidato :
                                        (!string.IsNullOrEmpty(orden.EmpresaNombre) ? orden.EmpresaNombre : orden.ProyectoDesc),
                        orden.PuestoCandidato,
                        orden.AreaCandidato,
                        orden.NssCandidato,
                        orden.TieneEvaluacion,
                        orden.TieneAntidoping
                    },
                    empleado = datosEmpleado
                };
            }
            catch (Exception ex)
            {
                return new { success = false, message = "Error: " + ex.Message };
            }
        }

        [WebMethod]
        public static object BuscarEmpleado(int numeroEmpleado)
        {
            try
            {
                var empleado = EmpleadoDal.BuscarPorNumero(numeroEmpleado);
                if (empleado != null)
                {
                    return new { success = true, empleado = empleado };
                }
                else
                {
                    return new { success = false, message = "No se encontró empleado con el número " + numeroEmpleado };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, message = "Error al buscar empleado: " + ex.Message };
            }
        }

        [WebMethod]
        public static object CrearSolicitud(string Modalidad, int? NumeroEmpleado, int FkTipoServicio, string NombreCandidato, string ApellidoPaterno, string ApellidoMaterno, string PuestoDesc, string PuestoDeseado, string EmpresaDesc, string ProyectoDesc, int? FkEmpresa, int? FkProyecto, string Sexo)
        {
            try
            {
                string modalidad = (Modalidad ?? "INGRESO").Trim().ToUpperInvariant();

                int? fkEmpleado = null;
                int? fkCandidato = null;
                int? fkProyecto = FkProyecto > 0 ? FkProyecto : null;
                int fkTipoServicio;

                if (modalidad == "PERIODICO")
                {
                    if (!NumeroEmpleado.HasValue || NumeroEmpleado.Value <= 0)
                    {
                        return new { success = false, message = "Debe ingresar un número de empleado válido." };
                    }

                    // Resolver tipo de servicio si no viene o para validar
                    int resolvedTipo = FkTipoServicio;
                    if (resolvedTipo <= 0)
                    {
                        resolvedTipo = ResolverTipoServicioPeriodico();
                    }

                    if (resolvedTipo <= 0)
                    {
                        return new { success = false, message = "Seleccione el tipo de examen para empleado." };
                    }

                    var empleado = EmpleadoDal.BuscarPorNumero(NumeroEmpleado.Value);
                    if (empleado == null)
                    {
                        return new { success = false, message = "No se encontró empleado con el número " + NumeroEmpleado.Value };
                    }

                    fkEmpleado = empleado.PkEmpleado;
                    fkTipoServicio = resolvedTipo;

                    // Intentar recuperar el proyecto del empleado si no se proporcionó
                    if (!fkProyecto.HasValue || fkProyecto.Value <= 0)
                    {
                        if (empleado.FkProyecto.HasValue && empleado.FkProyecto.Value > 0)
                        {
                            fkProyecto = empleado.FkProyecto;
                        }
                        else
                        {
                            using (var db = new ApplicationDbContext())
                            {
                                var ent = db.Empleados.Find(fkEmpleado.Value);
                                if (ent != null) fkProyecto = ent.fkProyecto;
                            }
                        }
                    }

                    int pkOrdenEmpleado = OrdenServicioMedicoDal.Insertar(fkEmpleado, fkCandidato, fkProyecto, fkTipoServicio);

                    return new
                    {
                        success = true,
                        message = "Solicitud de empleado creada exitosamente.",
                        pkOrdenMedico = pkOrdenEmpleado,
                        modalidad = modalidad,
                        nombreCandidato = empleado.NombreCompleto
                    };
                }

                fkTipoServicio = ResolverTipoServicioIngreso();

                if (string.IsNullOrWhiteSpace(NombreCandidato) || string.IsNullOrWhiteSpace(ApellidoPaterno) || !FkEmpresa.HasValue || FkEmpresa.Value <= 0)
                {
                    return new { success = false, message = "Debe completar los datos obligatorios para una solicitud de ingreso." };
                }

                int pkCandidato = CandidatoDal.Insertar(
                    NombreCandidato,
                    ApellidoPaterno,
                    ApellidoMaterno,
                    PuestoDesc ?? PuestoDeseado,
                    ProyectoDesc,
                    EmpresaDesc,
                    Sexo ?? "",
                    FkEmpresa,
                    FkProyecto
                );

                fkCandidato = pkCandidato;

                int pkOrden = OrdenServicioMedicoDal.Insertar(fkEmpleado, fkCandidato, fkProyecto, fkTipoServicio);

                return new
                {
                    success = true,
                    message = "Solicitud creada exitosamente.",
                    pkOrdenMedico = pkOrden,
                    modalidad = "INGRESO",
                    empresaDesc = EmpresaDesc,
                    proyectoDesc = ProyectoDesc,
                    puestoDesc = PuestoDesc,
                    nombreCandidato = (NombreCandidato + " " + ApellidoPaterno + " " + (ApellidoMaterno ?? "")).Trim()
                };
            }
            catch (Exception ex)
            {
                // Mostrar la causa real cuando proviene de EF (constraint, nullability, etc.)
                var inner = ex.InnerException != null ? ex.InnerException.InnerException?.Message ?? ex.InnerException.Message : null;
                var msg = "Error al crear la solicitud: " + ex.Message;
                if (!string.IsNullOrWhiteSpace(inner)) msg += " | Detalle: " + inner;
                return new { success = false, message = msg };
            }
        }

        [WebMethod]
        public static object ObtenerProyectosPorEmpresa(int fkEmpresa)
        {
            try { return new { success = true, data = CatalogoDal.ObtenerProyectosPorEmpresa(fkEmpresa) }; }
            catch (Exception ex) { return new { success = false, message = ex.Message }; }
        }

        [WebMethod]
        public static object ObtenerDepartamentosPorEmpresa(int fkEmpresa)
        {
            try { return new { success = true, data = CatalogoDal.ObtenerDepartamentosPorEmpresa(fkEmpresa) }; }
            catch (Exception ex) { return new { success = false, message = ex.Message }; }
        }

        [WebMethod]
        public static object ObtenerAreasPorDepartamento(int fkDepartamento)
        {
            try { return new { success = true, data = CatalogoDal.ObtenerAreasPorDepartamento(fkDepartamento) }; }
            catch (Exception ex) { return new { success = false, message = ex.Message }; }
        }

        [WebMethod]
        public static object ObtenerPuestosPorArea(int fkArea)
        {
            try { return new { success = true, data = CatalogoDal.ObtenerPuestosPorArea(fkArea) }; }
            catch (Exception ex) { return new { success = false, message = ex.Message }; }
        }

        [WebMethod]
        public static object ObtenerPuestosPorEmpresa(int fkEmpresa)
        {
            try { return new { success = true, data = CatalogoDal.ObtenerPuestosPorEmpresa(fkEmpresa) }; }
            catch (Exception ex) { return new { success = false, message = ex.Message }; }
        }

        [WebMethod]
        public static object Eliminar(int pkOrdenMedico)
        {
            try
            {
                var orden = OrdenServicioMedicoDal.ObtenerPorId(pkOrdenMedico);
                if (orden == null)
                {
                    return new { success = false, message = "Solicitud no encontrada." };
                }

                if (!EsVisibleEnDashboardRh(orden))
                {
                    return new { success = false, message = "Esta solicitud no puede eliminarse desde el dashboard de Recursos Humanos." };
                }

                OrdenServicioMedicoDal.Eliminar(pkOrdenMedico);
                return new { success = true, message = "Solicitud eliminada correctamente." };
            }
            catch (Exception ex)
            {
                return new { success = false, message = "Error: " + ex.Message };
            }
        }

        [WebMethod]
        public static object ObtenerPaseHtml(int pkOrdenMedico)
        {
            try
            {
                var orden = OrdenServicioMedicoDal.ObtenerPorId(pkOrdenMedico);
                if (orden == null)
                {
                    return new { success = false, message = "Orden no encontrada." };
                }

                if (!EsVisibleEnDashboardRh(orden))
                {
                    return new { success = false, message = "Esta solicitud no está disponible en el dashboard de Recursos Humanos." };
                }

                // Generar HTML del pase
                string html = GenerarHtmlPase(orden);
                return new { success = true, html = html };
            }
            catch (Exception ex)
            {
                return new { success = false, message = "Error: " + ex.Message };
            }
        }

        private static string GenerarHtmlPase(OrdenServicioMedicoVm orden)
        {
            string templatePath = System.Web.HttpContext.Current.Server.MapPath("~/ServicioMedico/Formatos/PaseMedico.html");
            if (!System.IO.File.Exists(templatePath)) 
                return "<p style='color:red;'>Error: Plantilla de Pase Médico no encontrada en " + templatePath + "</p>";

            string html = System.IO.File.ReadAllText(templatePath, System.Text.Encoding.UTF8);

            string empresa = !string.IsNullOrEmpty(orden.EmpresaCandidato) ? orden.EmpresaCandidato :
                            (!string.IsNullOrEmpty(orden.EmpresaNombre) ? orden.EmpresaNombre : orden.ProyectoDesc);

            string H(object s) => System.Web.HttpUtility.HtmlEncode(s?.ToString() ?? "");

            // Obtener evaluación si existe para mostrar aptitud
            var evaluacion = EvaluacionDal.ObtenerPorOrden(orden.PkOrdenMedico);
            int apt = (evaluacion != null) ? (evaluacion.FkAptitudMedica ?? 0) : 0;
            string recomendaciones = (evaluacion?.Recomendaciones ?? "").ToUpper();

            var rep = new Dictionary<string, string>
            {
                { "{{EMPRESA}}",         H(empresa).ToUpper() },
                { "{{FECHA}}",           orden.FechaOrden?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy") },
                { "{{PROYECTO}}",        H(orden.ProyectoDesc ?? "-").ToUpper() },
                { "{{NOMBRE}}",          H(orden.NombrePersona ?? "-").ToUpper() },
                { "{{PUESTO}}",          H(orden.PuestoCandidato ?? "-").ToUpper() },
                { "{{RES_APTO}}",        apt == 1 ? "&#10004;" : "" },
                { "{{RES_CONDICIONADO}}",apt == 2 ? "&#10004;" : "" },
                { "{{RES_NO_APTO}}",     apt == 3 ? "&#10004;" : "" },
                { "{{RECOMENDACIONES}}", H(recomendaciones) },
            };

            foreach (var kv in rep) 
                html = html.Replace(kv.Key, kv.Value);

            return html;
        }
    }
}


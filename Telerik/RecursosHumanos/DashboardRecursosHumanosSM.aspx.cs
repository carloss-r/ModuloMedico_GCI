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
        private static bool EsVisibleEnDashboardRh(OrdenServicioMedicoVm orden)
        {
            if (orden == null) return false;
            return !orden.FkEmpleado.HasValue && orden.FkTipoServicio != 3;
        }

        private static int ResolverTipoServicioIngreso()
        {
            using (var db = new ApplicationDbContext())
            {
                var tipoIngreso = db.TiposServicio
                    .Where(t => t.descripcion != null)
                    .OrderBy(t => t.pkTipoServicio)
                    .FirstOrDefault(t => t.descripcion.ToUpper().Contains("INGRESO"));

                if (tipoIngreso != null)
                {
                    return tipoIngreso.pkTipoServicio;
                }

                var tipoExamen = db.TiposServicio
                    .Where(t => t.descripcion != null)
                    .OrderBy(t => t.pkTipoServicio)
                    .FirstOrDefault(t => t.descripcion.ToUpper().Contains("EXAMEN") || t.descripcion.ToUpper().Contains("MEDICO"));

                return tipoExamen != null ? tipoExamen.pkTipoServicio : 1;
            }
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
                    "INGRESO",
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

                    if (FkTipoServicio <= 0)
                    {
                        return new { success = false, message = "Seleccione el tipo de examen para empleado." };
                    }

                    using (var db = new ApplicationDbContext())
                    {
                        var tipoServicio = db.TiposServicio.FirstOrDefault(t => t.pkTipoServicio == FkTipoServicio);
                        var descTipo = (tipoServicio != null ? tipoServicio.descripcion : string.Empty) ?? string.Empty;
                        var descUpper = descTipo.ToUpperInvariant();
                        bool esAntidoping = descUpper.Contains("ANTIDOP");
                        bool esPeriodico = descUpper.Contains("PERIOD");
                        if (!esAntidoping && !esPeriodico)
                        {
                            return new { success = false, message = "Para empleado solo se permite examen Antidoping o Periódico." };
                        }
                    }

                    var empleado = EmpleadoDal.BuscarPorNumero(NumeroEmpleado.Value);
                    if (empleado == null)
                    {
                        return new { success = false, message = "No se encontró empleado con el número " + NumeroEmpleado.Value };
                    }

                    fkEmpleado = empleado.PkEmpleado;
                    fkTipoServicio = FkTipoServicio;

                    if (!fkProyecto.HasValue || fkProyecto.Value <= 0)
                    {
                        using (var db = new ApplicationDbContext())
                        {
                            var ent = db.Empleados.Find(fkEmpleado.Value);
                            if (ent != null) fkProyecto = ent.fkProyecto;
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
            try
            {
                var data = CatalogoDal.ObtenerProyectosPorEmpresa(fkEmpresa);
                return new { success = true, data = data };
            }
            catch (Exception ex) { return new { success = false, message = ex.Message }; }
        }

        [WebMethod]
        public static object ObtenerPuestosPorEmpresa(int fkEmpresa)
        {
            try
            {
                var data = CatalogoDal.ObtenerPuestosPorEmpresa(fkEmpresa);
                return new { success = true, data = data };
            }
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
            string empresa = !string.IsNullOrEmpty(orden.EmpresaCandidato) ? orden.EmpresaCandidato :
                            (!string.IsNullOrEmpty(orden.EmpresaNombre) ? orden.EmpresaNombre : orden.ProyectoDesc);

            string puesto = orden.PuestoCandidato ?? "-";
            string nombre = orden.NombrePersona ?? "-";
            string proyecto = orden.ProyectoDesc ?? "-";
            string fecha = orden.FechaOrdenFormateada ?? DateTime.Now.ToString("dd/MM/yyyy");

            // Obtener evaluación si existe para mostrar aptitud
            var evaluacion = EvaluacionDal.ObtenerPorOrden(orden.PkOrdenMedico);
            string checkApto = (evaluacion != null && evaluacion.FkAptitudMedica == 1) ? "✔" : "";
            string checkCond = (evaluacion != null && evaluacion.FkAptitudMedica == 2) ? "✔" : "";
            string checkNoAp = (evaluacion != null && evaluacion.FkAptitudMedica == 3) ? "✔" : "";
            string recomendaciones = evaluacion?.Recomendaciones ?? "";

            return $@"<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width' />
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; color: #333; margin: 0; padding: 20px; }}
        .print-content {{ max-width: 700px; margin: 0 auto; }}
        table {{ width: 100%; border-collapse: collapse; }}
        .main-table {{ border: 2px solid #333; margin-bottom: 0; }}
        .main-table td {{ border-right: 1px solid #333; border-bottom: 1px solid #333; padding: 6px 10px; }}
        .label-cell {{ font-size: 11px; font-weight: bold; width: 15%; background-color: #f9f9f9; }}
        .value-cell {{ font-size: 12px; width: 35%; }}
        .header-cell {{ text-align: center; padding: 8px; font-size: 15px; font-weight: bold; border-bottom: 2px solid #333; letter-spacing: 1px; background-color: #eee; -webkit-print-color-adjust: exact; print-color-adjust: exact; }}
        .instruction-table {{ border-left: 2px solid #333; border-right: 2px solid #333; margin-top: 0; }}
        .instruction-header {{ text-align: center; padding: 6px; background: #444; color: #fff; font-size: 10px; font-weight: bold; letter-spacing: 0.5px; -webkit-print-color-adjust: exact; print-color-adjust: exact; }}
        .instruction-subheader {{ text-align: center; padding: 3px; font-size: 10px; color: #555; border-bottom: 1px solid #333; }}
        .option-cell {{ width: 33%; text-align: center; padding: 12px; font-size: 12px; font-weight: bold; border-right: 1px solid #333; border-bottom: 1px solid #333; }}
        .option-cell:last-child {{ border-right: none; }}
        .special-table {{ border-left: 2px solid #333; border-right: 2px solid #333; }}
        .fill-instruction {{ padding: 3px 10px; font-size: 9px; color: #888; text-align: center; border-bottom: 1px solid #999; }}
        .special-label {{ padding: 8px 10px; font-size: 11px; color: #333; }}
        .line-row td {{ padding: 5px 10px; border-bottom: 1px solid #999; }}
        .signature-table {{ border: 2px solid #333; margin-top: 0; }}
        .signature-space {{ height: 50px; border-right: 1px solid #333; border-bottom: 1px solid #333; }}
        .signature-space:last-child {{ border-right: none; }}
        .signature-label {{ width: 33%; text-align: center; padding: 6px; font-size: 9px; font-weight: bold; border-right: 1px solid #333; vertical-align: top; }}
        .signature-label:last-child {{ border-right: none; }}
        @media print {{ body {{ padding: 0; }} .no-print {{ display: none !important; }} * {{ -webkit-print-color-adjust: exact !important; print-color-adjust: exact !important; }} }}
    </style>
</head>
<body>
    <div class='print-content'>
        <table class='main-table'>
            <tr>
                <td colspan='4' class='header-cell'>SERVICIO MEDICO</td>
            </tr>
            <tr>
                <td class='label-cell'>EMPRESA</td>
                <td class='value-cell'>{System.Web.HttpUtility.HtmlEncode(empresa.ToUpper())}</td>
                <td class='label-cell'>FECHA:</td>
                <td class='value-cell'>{fecha}</td>
            </tr>
            <tr>
                <td class='label-cell'>PROYECTO</td>
                <td colspan='3' class='value-cell'>{System.Web.HttpUtility.HtmlEncode(proyecto.ToUpper())}</td>
            </tr>
            <tr>
                <td colspan='4' style='padding:6px 10px; font-size:11px; border-bottom:1px solid #333;'>
                    <span style='font-weight:bold;'>POR ESTE CONDUCTO LE ENVIO AL SR.(A):</span>
                    <span style='margin-left:8px; font-size:12px; text-transform:uppercase;'>{System.Web.HttpUtility.HtmlEncode(nombre.ToUpper())}</span>
                </td>
            </tr>
            <tr>
                <td colspan='4' style='padding:6px 10px; font-size:11px; border-bottom:1px solid #333;'>
                    <span style='font-weight:bold;'>CANDIDATO(A) A OCUPAR EL PUESTO DE:</span>
                    <span style='margin-left:8px; font-size:12px; text-transform:uppercase;'>{System.Web.HttpUtility.HtmlEncode(puesto.ToUpper())}</span>
                </td>
            </tr>
        </table>

        <table class='instruction-table'>
            <tr>
                <td colspan='3' class='instruction-header'>INSTRUCCIÓN AL AREA DE RECLUTAMIENTO Y SELECCIÓN</td>
            </tr>
            <tr>
                <td colspan='3' class='instruction-subheader'>El candidato es considerado como:</td>
            </tr>
            <tr>
                <td class='option-cell' style='vertical-align: middle;'>APTO <br/><span style='font-size: 20px; color: #1a5276;'>{checkApto}</span></td>
                <td class='option-cell' style='vertical-align: middle;'>APTO CONDICIONADO <br/><span style='font-size: 20px; color: #1a5276;'>{checkCond}</span></td>
                <td class='option-cell' style='vertical-align: middle;'>NO APTO <br/><span style='font-size: 20px; color: #1a5276;'>{checkNoAp}</span></td>
            </tr>
        </table>

        <table class='special-table'>
            <tr>
                <td class='fill-instruction'>RELLENAR O MARCAR LA CASILLA INDICADA</td>
            </tr>
            <tr>
                <td class='special-label'>Existe alguna indicación especial para el candidato:</td>
            </tr>
            <tr class='line-row'>
                <td style='height: 60px; vertical-align: top; font-size: 11px; padding: 5px 15px;'>{System.Web.HttpUtility.HtmlEncode(recomendaciones.ToUpper())}</td>
            </tr>
        </table>

        <table class='signature-table'>
            <tr>
                <td class='signature-space'>&nbsp;</td>
                <td class='signature-space'>&nbsp;</td>
                <td class='signature-space'>&nbsp;</td>
            </tr>
            <tr>
                <td class='signature-label'>Nombre y firma<br/>Reclutamiento</td>
                <td class='signature-label'>Vo. Bo. Nombre y firma<br/>del médico</td>
                <td class='signature-label'>EN CASO DE SER CONDICIONADO (NOMBRE Y FIRMA DE QUIEN AUTORIZA)</td>
            </tr>
        </table>
    </div>
</body>
</html>";
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Data.Entity;
using Telerik.Models.DAL;
using Telerik.Models.ViewModels;
using Telerik.Models.Entities;
using Telerik.Models;

namespace Telerik.ServicioMedico
{
    public partial class DashboardServicioMedico : System.Web.UI.Page
    {
        private const int DefaultPage = 1;
        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 200;

        private static object ErrorResponse(string message)
        {
            return new { success = false, message };
        }

        private static int NormalizePage(int? page)
        {
            return (page.HasValue && page.Value > 0) ? page.Value : DefaultPage;
        }

        private static int NormalizePageSize(int pageSize)
        {
            if (pageSize <= 0) return DefaultPageSize;
            return Math.Min(pageSize, MaxPageSize);
        }

        private static string NormalizeModalidad(string modalidad)
        {
            if (string.IsNullOrWhiteSpace(modalidad)) return null;
            var normalized = modalidad.Trim().ToUpperInvariant();
            return (normalized == "INGRESO" || normalized == "PERIODICO") ? normalized : null;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // WebForms Page Load - Inbox Médico
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
            int? filtroSemana)
        {
            try
            {
                if (fechaDesde.HasValue && fechaHasta.HasValue && fechaDesde.Value.Date > fechaHasta.Value.Date)
                {
                    return ErrorResponse("El rango de fechas no es válido.");
                }

                int totalRegistros;
                var solicitudes = OrdenServicioMedicoDal.ObtenerTodas(
                    out totalRegistros,
                    NormalizePage(pagina),
                    NormalizePageSize(tamanoPagina),
                    filtroNumEmpleado,
                    NormalizeModalidad(filtroModalidad),
                    filtroEstatus,
                    fechaDesde,
                    fechaHasta,
                    filtroEmpresa,
                    filtroArea,
                    filtroAnio,
                    filtroSemana
                );

                return new { success = true, data = solicitudes, total = totalRegistros };
            }
            catch (Exception)
            {
                return ErrorResponse("Error al cargar la bandeja médica.");
            }
        }

        [WebMethod]
        public static object VerDetalle(int id)
        {
            try
            {
                if (id <= 0) return ErrorResponse("Identificador de solicitud inválido.");

                var orden = OrdenServicioMedicoDal.ObtenerPorId(id);
                if (orden == null) return ErrorResponse("Solicitud no encontrada.");

                object datosEmpleado = null;
                if (orden.FkEmpleado.HasValue)
                {
                    var emp = EmpleadoDal.BuscarPorNumero(orden.FkEmpleado.Value);
                    if (emp != null)
                    {
                        datosEmpleado = new { 
                            PkEmpleado = emp.PkEmpleado,
                            NombreCompleto = emp.NombreCompleto, 
                            PuestoDesc = emp.PuestoDesc, 
                            Nss = emp.Nss,
                            Rfc = emp.Rfc,
                            Curp = emp.Curp,
                            ProyectoDesc = emp.ProyectoDesc
                        };
                    }
                }

                return new {
                    success = true,
                    orden = new {
                        orden.PkOrdenMedico,
                        orden.FolioDisplay,
                        orden.Modalidad,
                        orden.TipoServicioDesc,
                        orden.EstatusDesc,
                        orden.FechaOrdenFormateada,
                        orden.NombrePersona,
                        orden.ProyectoDesc,
                        EmpresaNombre = !string.IsNullOrEmpty(orden.EmpresaCandidato) ? orden.EmpresaCandidato : (orden.EmpresaNombre ?? orden.ProyectoDesc),
                        FkEstatus = orden.FkEstatus ?? 0
                    },
                    empleado = datosEmpleado
                };
            }
            catch (Exception) { return ErrorResponse("Error al obtener el detalle de la solicitud."); }
        }

        [WebMethod]
        public static object ObtenerEvaluacionPreview(int id)
        {
            try
            {
                if (id <= 0) return ErrorResponse("Identificador de solicitud inválido.");

                var orden = OrdenServicioMedicoDal.ObtenerPorId(id);
                if (orden == null) return ErrorResponse("Solicitud no encontrada.");

                var evaluacion = EvaluacionDal.ObtenerPorOrden(id);
                if (evaluacion == null)
                {
                    return ErrorResponse("La evaluación completa aún no ha sido registrada.");
                }

                return new
                {
                    success = true,
                    orden = new
                    {
                        orden.PkOrdenMedico,
                        orden.FolioDisplay,
                        orden.Modalidad,
                        orden.TipoServicioDesc,
                        orden.EstatusDesc,
                        orden.FechaOrdenFormateada,
                        orden.NombrePersona,
                        orden.ProyectoDesc,
                        orden.PuestoCandidato,
                        orden.NssCandidato,
                        orden.SexoCandidato,
                        EmpresaNombre = !string.IsNullOrEmpty(orden.EmpresaCandidato) ? orden.EmpresaCandidato : (orden.EmpresaNombre ?? orden.ProyectoDesc)
                    },
                    evaluacion
                };
            }
            catch (Exception)
            {
                return ErrorResponse("Error al cargar la previsualización de la evaluación.");
            }
        }

        [WebMethod]
        public static object BuscarEmpleado(int numeroEmpleado)
        {
            try
            {
                if (numeroEmpleado <= 0)
                {
                    return ErrorResponse("Número de empleado inválido.");
                }

                using (var db = new ApplicationDbContext())
                {
                    // Buscar empleado por ID (usamos el número como pkEmpleado)
                    var empleado = db.Empleados
                        .FirstOrDefault(e => e.pkEmpleado == numeroEmpleado);

                    if (empleado == null)
                    {
                        return ErrorResponse("Empleado no encontrado.");
                    }

                    // Obtener nombres de catálogos
                    var puesto = empleado.fkPuesto.HasValue ? db.Puestos.FirstOrDefault(p => p.pkPuesto == empleado.fkPuesto.Value)?.descripcion : null;
                    var empresa = empleado.fkEmpresa.HasValue ? db.Empresas.FirstOrDefault(emp => emp.pkEmpresa == empleado.fkEmpresa.Value)?.nombre : null;

                    // Verificar si ya tiene una orden médica pendiente
                    var ordenPendiente = db.OrdenesMedicas
                        .Where(o => o.fkEmpleado == empleado.pkEmpleado && o.fkEstatus != 3)
                        .OrderByDescending(o => o.pkOrdenMedico)
                        .FirstOrDefault();

                    int? pkOrden = ordenPendiente?.pkOrdenMedico;

                    // Si no hay orden pendiente, crear una automáticamente
                    if (!pkOrden.HasValue)
                    {
                        var nuevaOrden = CrearOrdenEmpleadoAutomatica(empleado, db, puesto, empresa);
                        if (nuevaOrden != null)
                            pkOrden = nuevaOrden.pkOrdenMedico;
                    }

                    // Verificar si tiene evaluación previa para heredar datos
                    var evaluacionPrevia = db.EvaluacionesClinicas
                        .Join(db.OrdenesMedicas,
                              e => e.fkOrdenMedico,
                              o => o.pkOrdenMedico,
                              (e, o) => new { e, o })
                        .Where(x => x.o.fkEmpleado == empleado.pkEmpleado)
                        .OrderByDescending(x => x.e.fechaEvaluacion)
                        .Select(x => x.e)
                        .FirstOrDefault();

                    var result = new
                    {
                        success = true,
                        empleado = new
                        {
                            PkEmpleado = empleado.pkEmpleado,
                            NumeroEmpleado = empleado.pkEmpleado,
                            NombreCompleto = empleado.nombre + " " + empleado.aPaterno + " " + empleado.aMaterno,
                            Curp = empleado.curp,
                            Nss = empleado.numeroSeguroSocial,
                            Puesto = puesto,
                            Area = "",
                            Empresa = empresa,
                            Sexo = empleado.fkSexo,
                            FechaNacimiento = empleado.fechaNacimiento,
                            PkOrdenMedico = pkOrden,
                            // Datos previos de evaluación si existen
                            DatosPrevios = evaluacionPrevia != null ? new
                            {
                                evaluacionPrevia.nss,
                                evaluacionPrevia.fechaNacimiento,
                                evaluacionPrevia.lugarNacimiento,
                                evaluacionPrevia.estadoCivil,
                                evaluacionPrevia.manoDominante,
                                evaluacionPrevia.telefono,
                                evaluacionPrevia.domicilio,
                                evaluacionPrevia.escolaridad,
                                evaluacionPrevia.profesion
                            } : null
                        }
                    };

                    return result;
                }
            }
            catch (Exception)
            {
                return ErrorResponse("Error al buscar empleado.");
            }
        }

        [WebMethod]
        public static object ObtenerPaseHtml(int pkOrdenMedico)
        {
            try
            {
                if (pkOrdenMedico <= 0)
                {
                    return ErrorResponse("Identificador de orden inválido.");
                }

                var orden = OrdenServicioMedicoDal.ObtenerPorId(pkOrdenMedico);
                if (orden == null)
                {
                    return ErrorResponse("Orden no encontrada.");
                }

                string html = GenerarHtmlPase(orden);
                return new { success = true, html = html };
            }
            catch (Exception)
            {
                return ErrorResponse("Error al generar el pase médico.");
            }
        }

        private static string GenerarHtmlPase(OrdenServicioMedicoVm orden)
        {
            string empresa = !string.IsNullOrEmpty(orden.EmpresaCandidato) ? orden.EmpresaCandidato :
                            (!string.IsNullOrEmpty(orden.EmpresaNombre) ? orden.EmpresaNombre : orden.ProyectoDesc);
            empresa = string.IsNullOrWhiteSpace(empresa) ? "-" : empresa;

            string puesto = orden.PuestoCandidato ?? "-";
            string nombre = orden.NombrePersona ?? "-";
            string proyecto = orden.ProyectoDesc ?? "-";
            string fecha = orden.FechaOrdenFormateada ?? DateTime.Now.ToString("dd/MM/yyyy");

            var evaluacion = EvaluacionDal.ObtenerPorOrden(orden.PkOrdenMedico);
            string checkApto = (evaluacion != null && evaluacion.FkAptitudMedica == 1) ? "✔" : "";
            string checkCond = (evaluacion != null && evaluacion.FkAptitudMedica == 2) ? "✔" : "";
            string checkNoAp = (evaluacion != null && evaluacion.FkAptitudMedica == 3) ? "✔" : "";
            string recomendaciones = string.IsNullOrWhiteSpace(evaluacion != null ? evaluacion.Recomendaciones : null)
                ? ""
                : evaluacion.Recomendaciones;

            return $@"<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width' />
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; color: #333; margin: 0; padding: 20px; }}
        .print-content {{ max-width: 700px; margin: 0 auto; background: #fff; }}
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
    </style>
</head>
<body>
    <div class='print-content'>
        <table class='main-table'>
            <tr><td colspan='4' class='header-cell'>SERVICIO MEDICO</td></tr>
            <tr>
                <td class='label-cell'>EMPRESA</td>
                <td class='value-cell'>{HttpUtility.HtmlEncode(empresa.ToUpper())}</td>
                <td class='label-cell'>FECHA:</td>
                <td class='value-cell'>{fecha}</td>
            </tr>
            <tr>
                <td class='label-cell'>PROYECTO</td>
                <td colspan='3' class='value-cell'>{HttpUtility.HtmlEncode(proyecto.ToUpper())}</td>
            </tr>
            <tr>
                <td colspan='4' style='padding:6px 10px; font-size:11px; border-bottom:1px solid #333;'>
                    <span style='font-weight:bold;'>POR ESTE CONDUCTO LE ENVIO AL SR.(A):</span>
                    <span style='margin-left:8px; font-size:12px; text-transform:uppercase;'>{HttpUtility.HtmlEncode(nombre.ToUpper())}</span>
                </td>
            </tr>
            <tr>
                <td colspan='4' style='padding:6px 10px; font-size:11px; border-bottom:1px solid #333;'>
                    <span style='font-weight:bold;'>CANDIDATO(A) A OCUPAR EL PUESTO DE:</span>
                    <span style='margin-left:8px; font-size:12px; text-transform:uppercase;'>{HttpUtility.HtmlEncode(puesto.ToUpper())}</span>
                </td>
            </tr>
        </table>

        <table class='instruction-table'>
            <tr><td colspan='3' class='instruction-header'>INSTRUCCIÓN AL AREA DE RECLUTAMIENTO Y SELECCIÓN</td></tr>
            <tr><td colspan='3' class='instruction-subheader'>El candidato es considerado como:</td></tr>
            <tr>
                <td class='option-cell' style='vertical-align: middle;'>APTO <br/><span style='font-size: 20px; color: #1a5276;'>{checkApto}</span></td>
                <td class='option-cell' style='vertical-align: middle;'>APTO CONDICIONADO <br/><span style='font-size: 20px; color: #1a5276;'>{checkCond}</span></td>
                <td class='option-cell' style='vertical-align: middle;'>NO APTO <br/><span style='font-size: 20px; color: #1a5276;'>{checkNoAp}</span></td>
            </tr>
        </table>

        <table class='special-table'>
            <tr><td class='fill-instruction'>RELLENAR O MARCAR LA CASILLA INDICADA</td></tr>
            <tr><td class='special-label'>Existe alguna indicación especial para el candidato:</td></tr>
            <tr class='line-row'>
                <td style='height: 60px; vertical-align: top; font-size: 11px; padding: 5px 15px;'>{HttpUtility.HtmlEncode(recomendaciones.ToUpper())}</td>
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


        private static OrdenServicioMedico CrearOrdenEmpleadoAutomatica(Empleado emp, ApplicationDbContext db, string puestoDesc, string empresaNombre)
        {
            try
            {
                // Obtener tipo de servicio EXAMEN MEDICO
                int fkTipoServicio = db.TiposServicio.FirstOrDefault(t => t.descripcion != null && (t.descripcion.Contains("EXAMEN") || t.descripcion.Contains("MEDICO")))?.pkTipoServicio ?? 1;
                int fkEstatus = 1; // PENDIENTE

                var orden = new OrdenServicioMedico
                {
                    fkEmpleado = emp.pkEmpleado,
                    fkTipoServicio = fkTipoServicio,
                    fkEstatus = fkEstatus,
                    fechaOrden = DateTime.Now
                };

                db.OrdenesMedicas.Add(orden);
                db.SaveChanges();

                return orden;
            }
            catch
            {
                return null;
            }
        }
        [WebMethod]
        public static object ObtenerAntidopingHtml(int pkOrdenMedico)
        {
            try
            {
                if (pkOrdenMedico <= 0)
                {
                    return ErrorResponse("Identificador de orden inválido.");
                }

                var orden = OrdenServicioMedicoDal.ObtenerPorId(pkOrdenMedico);
                if (orden == null)
                {
                    return ErrorResponse("Orden no encontrada.");
                }

                string html = GenerarHtmlAntidoping(orden);
                return new { success = true, html = html };
            }
            catch (Exception ex)
            {
                return ErrorResponse("Error al generar el formato de antidoping: " + ex.Message);
            }
        }

        private static string GenerarHtmlAntidoping(OrdenServicioMedicoVm orden)
        {
            // 1. Ruta exacta de tu archivo Antidoping.html
            string templatePath = HttpContext.Current.Server.MapPath("~/ServicioMedico/Formatos/Antidoping.html");
            if (!System.IO.File.Exists(templatePath))
                throw new Exception("Template de antidoping no encontrado en: " + templatePath);

            // 2. Leer el archivo HTML
            string html = System.IO.File.ReadAllText(templatePath, System.Text.Encoding.UTF8);

            // 3. Obtener resultados desde la base de datos
            var anti = AntidopingDal.ObtenerPorOrden(orden.PkOrdenMedico);

            // Helpers para limpiar texto y marcar casillas
            string H(string s) => HttpUtility.HtmlEncode(s ?? "");
            bool ResultNeg(bool aplica, bool positivo) => aplica && !positivo;
            bool ResultPos(bool aplica, bool positivo) => aplica && positivo;

            // Lógica de Veredicto
            string veredictoHtml = "";
            if (anti != null && !string.IsNullOrWhiteSpace(anti.VeredictoFinal))
            {
                bool esNoApto = anti.VeredictoFinal.ToUpper().Contains("NO APTO");
                veredictoHtml = esNoApto
                    ? "<span style='border:2px solid #000; padding:4px 12px; font-size:11px; font-weight:bold;'>NO APTO PARA REALIZAR ACTIVIDADES OPERACIONALES</span>"
                    : "<span style='border:2px solid #000; padding:4px 12px; font-size:11px; font-weight:bold;'>APTO PARA REALIZAR ACTIVIDADES OPERACIONALES</span>";
            }

            // Foto de evidencia
            string fotoHtml = "";
            if (anti != null && !string.IsNullOrWhiteSpace(anti.UrlFotoEvidencia))
                fotoHtml = "<img src='" + anti.UrlFotoEvidencia + "' style='max-width:100%;max-height:100%;object-fit:contain;' />";

            string empresa = !string.IsNullOrEmpty(orden.EmpresaCandidato) ? orden.EmpresaCandidato : (orden.EmpresaNombre ?? orden.ProyectoDesc);

            // 4. Diccionario de variables a reemplazar en tu HTML (las que están entre {{ }})
            var rep = new Dictionary<string, string>
            {
                { "{{FECHA}}",        DateTime.Now.ToString("dd/MM/yyyy") },
                { "{{PROYECTO}}",     H(orden.ProyectoDesc ?? "") },
                { "{{EMPRESA}}",      H(empresa ?? "") },
                { "{{NOMBRE}}",       H(orden.NombrePersona ?? "") },
                { "{{NUM_TRABAJADOR}}", H(orden.FkEmpleado?.ToString() ?? "") },
                { "{{FOTO_HTML}}",    fotoHtml },
                { "{{VEREDICTO_HTML}}", veredictoHtml },
                { "{{COMENTARIOS}}",  H(anti?.Comentarios) },
                { "{{MEDICO}}",       "LIC. NATALY MARTINEZ PUGA" },
                
                // Sustancias (Marca con X si es Negativo o Positivo)
                { "{{OPI_NEG}}", anti != null && ResultNeg(anti.AplicaOpiaceos, anti.ResultadoOpiaceos) ? "X" : "" },
                { "{{OPI_POS}}", anti != null && ResultPos(anti.AplicaOpiaceos, anti.ResultadoOpiaceos) ? "X" : "" },
                { "{{COC_NEG}}", anti != null && ResultNeg(anti.AplicaCocaina, anti.ResultadoCocaina) ? "X" : "" },
                { "{{COC_POS}}", anti != null && ResultPos(anti.AplicaCocaina, anti.ResultadoCocaina) ? "X" : "" },
                { "{{BZO_NEG}}", anti != null && ResultNeg(anti.AplicaBenzodiacepinas, anti.ResultadoBenzodiacepinas) ? "X" : "" },
                { "{{BZO_POS}}", anti != null && ResultPos(anti.AplicaBenzodiacepinas, anti.ResultadoBenzodiacepinas) ? "X" : "" },
                { "{{AMP_NEG}}", anti != null && ResultNeg(anti.AplicaAnfetaminas, anti.ResultadoAnfetaminas) ? "X" : "" },
                { "{{AMP_POS}}", anti != null && ResultPos(anti.AplicaAnfetaminas, anti.ResultadoAnfetaminas) ? "X" : "" },
                { "{{MET_NEG}}", anti != null && ResultNeg(anti.AplicaMetanfetaminas, anti.ResultadoMetanfetaminas) ? "X" : "" },
                { "{{MET_POS}}", anti != null && ResultPos(anti.AplicaMetanfetaminas, anti.ResultadoMetanfetaminas) ? "X" : "" },
                { "{{THC_NEG}}", anti != null && ResultNeg(anti.AplicaTHC, anti.ResultadoTHC) ? "X" : "" },
                { "{{THC_POS}}", anti != null && ResultPos(anti.AplicaTHC, anti.ResultadoTHC) ? "X" : "" },
                { "{{ALC_NEG}}", anti != null && ResultNeg(anti.AplicaAlcohol, anti.ResultadoAlcohol) ? "X" : "" },
                { "{{ALC_POS}}", anti != null && ResultPos(anti.AplicaAlcohol, anti.ResultadoAlcohol) ? "X" : "" },
            };

            // 5. Reemplazar los tags en el HTML
            foreach (var kv in rep)
                html = html.Replace(kv.Key, kv.Value);

            return html;
        }
    }
}

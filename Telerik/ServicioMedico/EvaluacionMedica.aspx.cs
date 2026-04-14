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
using Telerik.Services;
using Telerik.Models;

namespace Telerik.ServicioMedico
{
    public partial class EvaluacionMedica : System.Web.UI.Page
    {
        public int IdOrden { get; set; }
        public int currentTipoServicio { get; set; }
        public string initialSexo { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            var qsId = Request.QueryString["id"];
            if (!string.IsNullOrEmpty(qsId))
            {
                if (int.TryParse(qsId, out int id))
                {
                    IdOrden = id;
                    var ms = new MedicalService();
                    var orden = OrdenServicioMedicoDal.ObtenerPorId(id);
                    if (orden != null)
                    {
                        currentTipoServicio = orden.FkTipoServicio;
                        initialSexo = ms.NormalizarSexo(orden.SexoCandidato);
                    }
                }
            }

            // Manejo de AJAX FormData para GuardarAntidoping
            if (Request.QueryString["action"] == "GuardarAntidoping" && Request.HttpMethod == "POST")
            {
                ManejarGuardarAntidoping();
            }
        }

        private void ManejarGuardarAntidoping()
        {
            Response.ContentType = "application/json";
            try
            {
                var req = Request.Form;
                int pkOrden = int.Parse(req["PkOrdenMedico"] ?? "0");

                var model = new AntidopingVm
                {
                    PkOrdenMedico = pkOrden,
                    ConsentimientoFirmado = (req["ConsentimientoFirmado"] ?? "").ToLower().Contains("true"),
                    ResultadoCocaina = (req["ResultadoCocaina"] ?? "").ToLower().Contains("true"),
                    AplicaCocaina = (req["AplicaCocaina"] ?? "").ToLower().Contains("true"),
                    ResultadoTHC = (req["ResultadoTHC"] ?? "").ToLower().Contains("true"),
                    AplicaTHC = (req["AplicaTHC"] ?? "").ToLower().Contains("true"),
                    ResultadoAlcohol = (req["ResultadoAlcohol"] ?? "").ToLower().Contains("true"),
                    AplicaAlcohol = (req["AplicaAlcohol"] ?? "").ToLower().Contains("true"),
                    ResultadoAnfetaminas = (req["ResultadoAnfetaminas"] ?? "").ToLower().Contains("true"),
                    AplicaAnfetaminas = (req["AplicaAnfetaminas"] ?? "").ToLower().Contains("true"),
                    ResultadoMetanfetaminas = (req["ResultadoMetanfetaminas"] ?? "").ToLower().Contains("true"),
                    AplicaMetanfetaminas = (req["AplicaMetanfetaminas"] ?? "").ToLower().Contains("true"),
                    ResultadoOpiaceos = (req["ResultadoOpiaceos"] ?? "").ToLower().Contains("true"),
                    AplicaOpiaceos = (req["AplicaOpiaceos"] ?? "").ToLower().Contains("true"),
                    ResultadoMetilfenidato = (req["ResultadoMetilfenidato"] ?? "").ToLower().Contains("true"),
                    AplicaMetilfenidato = (req["AplicaMetilfenidato"] ?? "").ToLower().Contains("true"),
                    ResultadoFentanilo = (req["ResultadoFentanilo"] ?? "").ToLower().Contains("true"),
                    AplicaFentanilo = (req["AplicaFentanilo"] ?? "").ToLower().Contains("true"),
                    ResultadoBenzodiacepinas = (req["ResultadoBenzodiacepinas"] ?? "").ToLower().Contains("true"),
                    AplicaBenzodiacepinas = (req["AplicaBenzodiacepinas"] ?? "").ToLower().Contains("true"),
                    VeredictoFinal = req["VeredictoFinal"],
                    Comentarios = req["Comentarios"]
                };

                var ms = new MedicalService();

                // Manejo de la foto evidencia
                if (Request.Files["FileEvidencia"] != null && Request.Files["FileEvidencia"].ContentLength > 0)
                {
                    var file = Request.Files["FileEvidencia"];
                    string ext = System.IO.Path.GetExtension(file.FileName);
                    string nombre = ms.GenerarNombreArchivoAntidoping(pkOrden, ext);

                    string carpeta = Server.MapPath("~/Content/Evidencias/Antidoping/");
                    if (!System.IO.Directory.Exists(carpeta))
                        System.IO.Directory.CreateDirectory(carpeta);

                    file.SaveAs(System.IO.Path.Combine(carpeta, nombre));
                    model.UrlFotoEvidencia = "/Content/Evidencias/Antidoping/" + nombre;
                }

                ms.GuardarAntidoping(model);
                ms.CompletarOrden(pkOrden);

                Response.Write("{\"success\": true, \"message\": \"Antidoping guardado y solicitud COMPLETADA.\"}");
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                {
                    msg += " | Inner: " + ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                        msg += " | Root: " + ex.InnerException.InnerException.Message;
                }
                Response.Write("{\"success\": false, \"message\": \"Error: " + msg.Replace("\"", "\\\"") + "\"}");
            }
            Response.End();
        }

        [WebMethod]
        public static object ObtenerDatosPaciente(int idOrden)
        {
            try
            {
                var orden = OrdenServicioMedicoDal.ObtenerPorId(idOrden);
                if (orden == null)
                    return new { success = false, message = "Orden no encontrada." };

                var ms = new MedicalService();
                var paciente = ms.ObtenerInfoPaciente(orden);

                var evaluacionActual = EvaluacionDal.ObtenerPorOrden(idOrden);
                var evaluacionPrevia = (evaluacionActual == null)
                    ? EvaluacionDal.ObtenerUltimaEvaluacionPorPaciente(orden.FkCandidato, orden.FkEmpleado)
                    : null;

                return new
                {
                    success = true,
                    paciente,
                    evaluacionActual,
                    evaluacionPrevia,
                    esNuevoExpediente = (evaluacionActual == null && evaluacionPrevia != null),
                    esEmpleado = orden.FkEmpleado.HasValue && orden.FkEmpleado.Value > 0
                };
            }
            catch (Exception ex)
            {
                return new { success = false, message = "Error: " + ex.Message };
            }
        }

        [WebMethod]
        public static object ObtenerHistorialEmpleado(int idOrden)
        {
            try
            {
                var orden = OrdenServicioMedicoDal.ObtenerPorId(idOrden);
                if (orden == null)
                    return new { success = false, message = "Orden no encontrada." };

                if (!orden.FkEmpleado.HasValue || orden.FkEmpleado.Value <= 0)
                    return new { success = true, esCandidato = true, historial = new object[0] };

                var historial = EvaluacionDal.ObtenerHistorialCompleto(orden.FkEmpleado.Value);

                return new
                {
                    success = true,
                    esCandidato = false,
                    totalEvaluaciones = historial.Count,
                    historial
                };
            }
            catch (Exception ex)
            {
                return new { success = false, message = "Error al obtener historial: " + ex.Message };
            }
        }

        [WebMethod]
        public static object ObtenerPaises()
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var data = db.Paises
                        .OrderBy(p => p.descripcion)
                        .Select(p => new CatalogoItem { Id = p.pkPais, Descripcion = p.descripcion })
                        .ToList();
                    return new { success = true, data = data };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, message = ex.Message };
            }
        }

        [WebMethod]
        public static object ObtenerEstados(int idPais)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var data = db.Estados
                        .Where(x => x.fkPais == idPais)
                        .OrderBy(x => x.descripcion)
                        .Select(x => new CatalogoItem { Id = x.pkEstado, Descripcion = x.descripcion })
                        .ToList();
                    return new { success = true, data = data };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, message = ex.Message };
            }
        }

        [WebMethod]
        public static object ObtenerMunicipios(int idEstado)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var data = db.Municipios
                        .Where(m => m.fkEstado == idEstado)
                        .OrderBy(m => m.descripcion)
                        .Select(m => new CatalogoItem { Id = m.pkMunicipio, Descripcion = m.descripcion })
                        .ToList();
                    return new { success = true, data = data };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, message = ex.Message };
            }
        }

        [WebMethod]
        public static object ObtenerColonias(int idMunicipio)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var data = (from c in db.Colonias
                                join cp in db.CodigosPostales on c.pkColonia equals cp.fkColonia into cps
                                from cp in cps.DefaultIfEmpty()
                                where c.fkMunicipio == idMunicipio
                                orderby c.descripcion
                                select new {
                                    Id = c.pkColonia,
                                    Descripcion = c.descripcion,
                                    CodigoPostal = cp != null ? cp.descripcion : "",
                                    pkCP = cp != null ? (int?)cp.pkCP : null
                                }).ToList();
                    return new { success = true, data = data };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, message = ex.Message };
            }
        }

        [WebMethod]
        public static object GuardarEvaluacion(EvaluacionMedicaVm model)
        {
            try
            {
                var ms = new MedicalService();
                ms.GuardarEvaluacion(model);
                ms.CompletarOrden(model.PkOrdenMedico);
                return new { success = true, pkOrden = model.PkOrdenMedico, message = "Evaluación médica guardada y completada correctamente." };
            }
            catch (Exception ex)
            {
                return new { success = false, message = "Error al guardar: " + ex.Message };
            }
        }

        [WebMethod]
        public static object CompletarSinAntidoping(int pkOrdenMedico)
        {
            try
            {
                var ms = new MedicalService();
                ms.CompletarOrden(pkOrdenMedico);
                return new { success = true };
            }
            catch (Exception ex)
            {
                return new { success = false, message = ex.Message };
            }
        }

    }
}


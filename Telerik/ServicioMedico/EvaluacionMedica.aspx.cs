using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using Telerik.Models.DAL;           // Capa de acceso a datos para operaciones con BD
using Telerik.Models.ViewModels;   // Clases para transferencia de datos entre capas
using Telerik.Models.Entities;     // Entidades que representan tablas de la BD
using Telerik.Services;            // Lógica de negocio del sistema médico
using Telerik.Models;              // Contexto y modelos principales

namespace Telerik.ServicioMedico
{
    public partial class EvaluacionMedica : System.Web.UI.Page
    {
        // Propiedades públicas para acceder desde el frontend (JavaScript)
        public int IdOrden { get; set; }              // ID de la orden de servicio médico actual
        public int currentTipoServicio { get; set; }  // Tipo de examen (1=Ingreso, 2=Periódico, 3=Antidoping)
        public string initialSexo { get; set; }       // Sexo del paciente (M/F) para mostrar sección adecuada

        // Evento que se ejecuta al cargar la página
        protected void Page_Load(object sender, EventArgs e)
        {
            // Obtener el ID de la orden desde la URL (ej: EvaluacionMedica.aspx?id=123)
            var qsId = Request.QueryString["id"];
            if (!string.IsNullOrEmpty(qsId))
            {
                // Convertir el ID a número entero de forma segura
                if (int.TryParse(qsId, out int id))
                {
                    IdOrden = id;  // Guardar el ID para uso posterior
                    
                    // Crear instancia del servicio médico para obtener datos
                    var ms = new MedicalService();
                    
                    // Buscar la orden de servicio en la base de datos usando el ID
                    var orden = OrdenServicioMedicoDal.ObtenerPorId(id);
                    if (orden != null)
                    {
                        // Guardar el tipo de servicio para determinar qué formulario mostrar
                        currentTipoServicio = orden.FkTipoServicio;
                        
                        // Normalizar el sexo (M/F) para mostrar sección masculina o femenina
                        initialSexo = ms.NormalizarSexo(orden.SexoCandidato);
                    }
                }
            }

            // Detectar si es una petición AJAX para guardar antidoping (sin recargar página)
            if (Request.QueryString["action"] == "GuardarAntidoping" && Request.HttpMethod == "POST")
            {
                ManejarGuardarAntidoping();  // Procesar el formulario de antidoping
            }
        }

        // Método para procesar y guardar los resultados del examen de antidoping vía AJAX
        private void ManejarGuardarAntidoping()
        {
            // Establecer el tipo de respuesta como JSON para que JavaScript lo interprete correctamente
            Response.ContentType = "application/json";
            try
            {
                // Obtener todos los datos enviados desde el formulario (FormData)
                var req = Request.Form;
                
                // Extraer el ID de la orden médica desde el formulario
                int pkOrden = int.Parse(req["PkOrdenMedico"] ?? "0");

                // Crear el modelo de vista con todos los datos del antidoping
                var model = new AntidopingVm
                {
                    PkOrdenMedico = pkOrden,  // ID de la orden
                    
                    // Consentimiento del trabajador (checkbox convertido a booleano)
                    ConsentimientoFirmado = (req["ConsentimientoFirmado"] ?? "").ToLower().Contains("true"),
                    
                    // Resultados de cada sustancia (checkbox -> booleano)
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
                    
                    // Campos de texto (veredicto y comentarios)
                    VeredictoFinal = req["VeredictoFinal"],
                    Comentarios = req["Comentarios"]
                };

                // Crear instancia del servicio médico para guardar en BD
                var ms = new MedicalService();

                // Procesar la foto de evidencia si se subió algún archivo
                if (Request.Files["FileEvidencia"] != null && Request.Files["FileEvidencia"].ContentLength > 0)
                {
                    var file = Request.Files["FileEvidencia"];  // Obtener el archivo subido
                    string ext = System.IO.Path.GetExtension(file.FileName);  // Extraer extensión (.jpg, .png)
                    
                    // Generar nombre único para el archivo usando el ID de orden
                    string nombre = ms.GenerarNombreArchivoAntidoping(pkOrden, ext);

                    // Ruta física donde se guardarán las evidencias
                    string carpeta = Server.MapPath("~/Content/Evidencias/Antidoping/");
                    
                    // Crear la carpeta si no existe
                    if (!System.IO.Directory.Exists(carpeta))
                        System.IO.Directory.CreateDirectory(carpeta);

                    // Guardar el archivo en el servidor
                    file.SaveAs(System.IO.Path.Combine(carpeta, nombre));
                    
                    // Guardar la URL relativa para acceder a la imagen desde la web
                    model.UrlFotoEvidencia = "/Content/Evidencias/Antidoping/" + nombre;
                }

                // Guardar todos los datos del antidoping en la base de datos
                ms.GuardarAntidoping(model);
                
                // Cambiar el estatus de la orden a "COMPLETADA" (estatus 3)
                ms.CompletarOrden(pkOrden);

                // Enviar respuesta de éxito al frontend (JavaScript)
                Response.Write("{\"success\": true, \"message\": \"Antidoping guardado y solicitud COMPLETADA.\"}");
            }
            catch (Exception ex)
            {
                // Capturar cualquier error durante el proceso
                string msg = ex.Message;
                
                // Obtener errores anidados para mejor diagnóstico
                if (ex.InnerException != null)
                {
                    msg += " | Inner: " + ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                        msg += " | Root: " + ex.InnerException.InnerException.Message;
                }
                
                // Enviar respuesta de error al frontend con detalles del problema
                Response.Write("{\"success\": false, \"message\": \"Error: " + msg.Replace("\"", "\\\"") + "\"}");
            }
            // Terminar la respuesta HTTP para evitar contenido adicional
            Response.End();
        }

        // WebMethod: Método que puede ser llamado desde JavaScript vía AJAX
        [WebMethod]
        public static object ObtenerDatosPaciente(int idOrden)
        {
            try
            {
                // Buscar la orden de servicio médica por su ID
                var orden = OrdenServicioMedicoDal.ObtenerPorId(idOrden);
                if (orden == null)
                    return new { success = false, message = "Orden no encontrada." };

                // Crear servicio médico para obtener información completa del paciente
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
        public static object ObtenerEstadoCivil()
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var data = db.EstadoCivil
                        .OrderBy(ec => ec.descripcion)
                        .Select(ec => new CatalogoItem { Id = ec.pkEstadoCivil, Descripcion = ec.descripcion })
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
        public static object ObtenerTipoSangre()
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var data = db.TipoSangre
                        .OrderBy(ts => ts.descripcion)
                        .Select(ts => new CatalogoItem { Id = ts.pkTipoSangre, Descripcion = ts.descripcion })
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
        public static object ObtenerProfesiones()
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var data = db.Profesion
                        .OrderBy(p => p.descripcion)
                        .Select(p => new CatalogoItem { Id = p.pkProfesion, Descripcion = p.descripcion })
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
        public static object ObtenerNivelEscolaridad()
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var data = db.NivelEscolaridad
                        .OrderBy(ne => ne.descripcion)
                        .Select(ne => new CatalogoItem { Id = ne.pkNivelEscolaridad, Descripcion = ne.descripcion })
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


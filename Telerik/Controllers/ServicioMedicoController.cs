using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Telerik.Models.Dal;
using Telerik.Models.ViewModels;

namespace Telerik.Controllers
{
    public class ServicioMedicoController : Controller
    {
        // GET: /ServicioMedico/
        public ActionResult Index()
        {
            ViewBag.Title = "Servicio Médico - Solicitudes Recibidas";
            return View();
        }

        // GET: /ServicioMedico/IniciarEvaluacion/5
        public ActionResult IniciarEvaluacion(int id)
        {
            // Validar que la orden NO esté ya Completada (fkEstatus = 3)
            // para evitar regresar el estatus de una orden terminada
            var orden = OrdenServicioMedicoDal.ObtenerPorId(id);
            if (orden != null && orden.FkEstatus == 3)
            {
                TempData["ErrorMsg"] = "Esta solicitud ya está Completada y no puede ser re-evaluada.";
                return RedirectToAction("Index");
            }

            Session["IdOrdenEvaluacion"] = id;
            return RedirectToAction("Evaluacion");
        }

        // GET: /ServicioMedico/Evaluacion
        public ActionResult Evaluacion()
        {
            if (Session["IdOrdenEvaluacion"] == null)
            {
                return RedirectToAction("Index");
            }

            int id = (int)Session["IdOrdenEvaluacion"];
            var orden = OrdenServicioMedicoDal.ObtenerPorId(id);
            ViewBag.Title = orden.FkTipoServicio == 3 ? "Evaluación Antidoping" : "Evaluación Médica";
            ViewBag.IdOrden = id;
            ViewBag.TipoServicio = orden.FkTipoServicio;
            return View();
        }

        // GET: /ServicioMedico/ObtenerSolicitudes
        [HttpGet]
        public JsonResult ObtenerSolicitudes()
        {
            try
            {
                var solicitudes = OrdenServicioMedicoDal.ObtenerTodas();
                return Json(new { success = true, data = solicitudes }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener solicitudes: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        public JsonResult GuardarEvaluacion(EvaluacionMedicaVm model)
        {
            try
            {
                EvaluacionDal.GuardarEvaluacion(model);
                return Json(new { success = true, message = "Evaluación médica guardada correctamente. ¿Desea continuar con el Antidoping?" });
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = "Error al guardar: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult GuardarAntidoping()
        {
            try
            {
                // El model binder de MVC 4.0 no resuelve HttpPostedFileBase dentro de un VM
                // cuando el POST es multipart/form-data con campos bool. Se leen manualmente.
                var req = Request.Form;

                int pkOrden = 0;
                if (!int.TryParse(req["PkOrdenMedico"], out pkOrden) || pkOrden == 0)
                    return Json(new { success = false, message = "PkOrdenMedico inválido o no recibido. Valor: '" + req["PkOrdenMedico"] + "'" });

                var model = new AntidopingVm
                {
                    PkOrdenMedico         = pkOrden,
                    ConsentimientoFirmado = (req["ConsentimientoFirmado"] ?? "").ToLower().Contains("true"),
                    ResultadoCocaina      = (req["ResultadoCocaina"]      ?? "").ToLower().Contains("true"),
                    ResultadoTHC          = (req["ResultadoTHC"]          ?? "").ToLower().Contains("true"),
                    ResultadoAnfetaminas  = (req["ResultadoAnfetaminas"]  ?? "").ToLower().Contains("true"),
                    ResultadoMetanfetaminas=(req["ResultadoMetanfetaminas"]?? "").ToLower().Contains("true"),
                    ResultadoOpiaceos     = (req["ResultadoOpiaceos"]     ?? "").ToLower().Contains("true"),
                    VeredictoFinal        = req["VeredictoFinal"],
                    Comentarios           = req["Comentarios"]
                };

                // Guardar imagen en carpeta ~/Content/Evidencias/Antidoping/
                // Nombre: ANTI-SOL-SM-{id}_{fecha}_{hora}.ext  (fácil de identificar)
                if (Request.Files["FileEvidencia"] != null && Request.Files["FileEvidencia"].ContentLength > 0)
                {
                    var file = Request.Files["FileEvidencia"];
                    string ext   = System.IO.Path.GetExtension(file.FileName);
                    string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string nombre = string.Format("ANTI-SOL-SM-{0:D4}_{1}{2}",
                        model.PkOrdenMedico, stamp, ext);

                    string carpeta = Server.MapPath("~/Content/Evidencias/Antidoping/");
                    if (!System.IO.Directory.Exists(carpeta))
                        System.IO.Directory.CreateDirectory(carpeta);

                    file.SaveAs(System.IO.Path.Combine(carpeta, nombre));
                    model.UrlFotoEvidencia = "/Content/Evidencias/Antidoping/" + nombre;
                }

                AntidopingDal.GuardarAntidoping(model);
                return Json(new { success = true, message = "Antidoping guardado y solicitud COMPLETADA." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al guardar antidoping: " + ex.Message });
            }
        }

        // POST: /ServicioMedico/CompletarSinAntidoping
        // Marca la orden como Completada (3) cuando el médico finaliza
        // la evaluación pero elige no hacer el antidoping.
        [HttpPost]
        public JsonResult CompletarSinAntidoping(int pkOrdenMedico)
        {
            try
            {
                OrdenServicioMedicoDal.ActualizarEstatus(pkOrdenMedico, 3);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: /ServicioMedico/ImprimirEvaluacion/5
        public ActionResult ImprimirEvaluacion(int id)
        {
            try
            {
                var orden = OrdenServicioMedicoDal.ObtenerPorId(id);
                if (orden == null)
                    return Content("Error: Orden no encontrada.");

                var evaluacion = EvaluacionDal.ObtenerPorOrden(id);
                if (evaluacion == null)
                    return Content("Error: No se encontró la evaluación médica para esta orden (es posible que aún no se haya completado).");

                ViewBag.Orden = orden;
                ViewBag.Paciente = ObtenerInfoPaciente(orden);
                return View("~/Views/ServicioMedico/ImprimirEvaluacion.cshtml", evaluacion);
            }
            catch (Exception ex)
            {
                return Content("Error al generar el reporte: " + ex.Message);
            }
        }

        public ActionResult ImprimirAntidoping(int id)
        {
            try
            {
                var orden = OrdenServicioMedicoDal.ObtenerPorId(id);
                if (orden == null)
                    return Content("Error: Orden no encontrada.");

                var antidoping = AntidopingDal.ObtenerPorOrden(id);
                if (antidoping == null)
                    return Content("Error: No se encontró el examen antidoping para esta orden (es posible que aún no se haya completado).");

                ViewBag.Orden = orden;
                ViewBag.Paciente = ObtenerInfoPaciente(orden);
                return View("~/Views/ServicioMedico/ImprimirAntidoping.cshtml", antidoping);
            }
            catch (Exception ex)
            {
                return Content("Error al generar el reporte: " + ex.Message);
            }
        }

        // GET: /ServicioMedico/ObtenerDatosPaciente
        public JsonResult ObtenerDatosPaciente(int idOrden)
        {
            try
            {
                var orden = OrdenServicioMedicoDal.ObtenerPorId(idOrden);
                if (orden == null)
                    return Json(new { success = false, message = "Orden no encontrada." }, JsonRequestBehavior.AllowGet);

                var paciente = ObtenerInfoPaciente(orden);
                return Json(new { success = true, paciente }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private PacienteInfoVm ObtenerInfoPaciente(OrdenServicioMedicoVm orden)
        {
            if (orden.FkEmpleado.HasValue)
            {
                var emp = EmpleadoDal.BuscarPorNumero(orden.FkEmpleado.Value);
                if (emp != null)
                {
                    string edad = emp.Edad;
                    if (emp.FechaNacimiento.HasValue)
                    {
                         var hoy = DateTime.Today;
                         var nacimiento = emp.FechaNacimiento.Value;
                         var edadCalc = hoy.Year - nacimiento.Year;
                         if (nacimiento.Date > hoy.AddYears(-edadCalc)) edadCalc--;
                         edad = edadCalc.ToString();
                    }

                    return new PacienteInfoVm
                    {
                        NombreCompleto = emp.NombreCompleto,
                        Edad = edad,
                        Puesto = emp.PuestoDesc,
                        Area = emp.AreaDesc,
                        Empresa = emp.ProyectoDesc, // Project for employees
                        Sexo = emp.Sexo,
                        Tipo = "EMPLEADO",  
                        TipoServicioId = orden.FkTipoServicio,
                        TipoServicioDesc = orden.TipoServicioDesc,
                        NumeroEmpleado = emp.PkEmpleado.ToString(),
                        
                        FechaNacimiento = emp.FechaNacimiento.HasValue ? emp.FechaNacimiento.Value.ToString("yyyy-MM-dd") : "",
                        Nss = emp.Nss,
                        Telefono = emp.Telefono,
                        Direccion = string.Join(", ", new string[] {
                            (!string.IsNullOrEmpty(emp.Calle) ? emp.Calle + " " + emp.NumExterior + (string.IsNullOrEmpty(emp.NumInterior) ? "" : " " + emp.NumInterior) : ""),
                            emp.ColoniaDesc,
                            emp.MunicipioDesc,
                            emp.EstadoDesc,
                            emp.PaisDesc,
                            (!string.IsNullOrEmpty(emp.CPDesc) ? "CP: " + emp.CPDesc : "")
                        }.Where(s => !string.IsNullOrEmpty(s))).Trim(),
                        EstadoCivil = emp.EstadoCivil,
                        TipoSangre = emp.TipoSangre,
                        Rfc = emp.Rfc,
                        Curp = emp.Curp,
                        TieneHijos = emp.TieneHijos,
                        NumeroHijos = emp.NumeroHijosDesc,
                        Escolaridad = emp.EscolaridadDesc
                    };
                }
            }
            
            return new PacienteInfoVm
            {
                NombreCompleto = orden.NombrePersona ?? "",
                Edad = orden.EdadCandidato ?? "",
                Puesto = orden.PuestoCandidato ?? "",
                Area = orden.AreaCandidato ?? "",
                Empresa = !string.IsNullOrEmpty(orden.EmpresaCandidato) ? orden.EmpresaCandidato : (orden.ProyectoDesc ?? ""),
                Sexo = orden.SexoCandidato ?? "",
                Tipo = "CANDIDATO",
                TipoServicioId = orden.FkTipoServicio,
                TipoServicioDesc = orden.TipoServicioDesc,
                NumeroEmpleado = "N/A"
            };
        }

        // GET: /ServicioMedico/VerDetalle?id=1
        [HttpGet]
        public JsonResult VerDetalle(int id)
        {
            try
            {
                var orden = OrdenServicioMedicoDal.ObtenerPorId(id);
                if (orden == null)
                {
                    return Json(new { success = false, message = "Solicitud no encontrada." }, JsonRequestBehavior.AllowGet);
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

                return Json(new
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
                        orden.ProyectoDesc
                    },
                    empleado = datosEmpleado
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: /ServicioMedico/CambiarEstatus
        [HttpPost]
        public JsonResult CambiarEstatus(int pkOrdenMedico, int fkEstatus)
        {
            try
            {
                OrdenServicioMedicoDal.ActualizarEstatus(pkOrdenMedico, fkEstatus);
                return Json(new { success = true, message = "Estatus actualizado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // POST: /ServicioMedico/BuscarEmpleado?numero=123
        [HttpGet]
        public JsonResult BuscarEmpleado(int numero)
        {
            try
            {
                var empleado = EmpleadoDal.BuscarPorNumero(numero);
                if (empleado != null)
                {
                    return Json(new { success = true, data = empleado }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "No se encontró empleado con el número " + numero }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al buscar empleado: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: /ServicioMedico/CrearSolicitud
        [HttpPost]
        public JsonResult CrearSolicitud(NuevaSolicitudVm model)
        {
            try
            {
                if (model.FkTipoServicio <= 0)
                {
                    return Json(new { success = false, message = "Debe seleccionar un tipo de servicio." });
                }

                int? fkEmpleado = null;
                int? fkCandidato = null;
                int? fkProyecto = model.FkProyecto > 0 ? model.FkProyecto : null;

                if (model.Modalidad == "PERIODICO")
                {
                    if (!model.NumeroEmpleado.HasValue || model.NumeroEmpleado.Value <= 0)
                    {
                        return Json(new { success = false, message = "Debe ingresar un número de empleado válido." });
                    }

                    // Verificar que el empleado existe
                    var empleado = EmpleadoDal.BuscarPorNumero(model.NumeroEmpleado.Value);
                    if (empleado == null)
                    {
                        return Json(new { success = false, message = "El empleado con número " + model.NumeroEmpleado.Value + " no existe." });
                    }

                    fkEmpleado = model.NumeroEmpleado.Value;
                }
                else if (model.Modalidad == "INGRESO")
                {
                    if (string.IsNullOrWhiteSpace(model.NombreCandidato))
                    {
                        return Json(new { success = false, message = "Debe ingresar el nombre del candidato." });
                    }

                    // Crear candidato nuevo
                    int pkCandidato = CandidatoDal.Insertar(
                        model.NombreCandidato,
                        model.ApellidoCandidato,
                        model.PuestoDesc ?? model.PuestoDeseado
                    );

                    fkCandidato = pkCandidato;
                }
                else
                {
                    return Json(new { success = false, message = "Modalidad no válida." });
                }

                // Crear la orden de servicio médico
                int pkOrden = OrdenServicioMedicoDal.Insertar(fkEmpleado, fkCandidato, fkProyecto, model.FkTipoServicio);

                return Json(new {
                    success = true,
                    message = "Solicitud creada exitosamente.",
                    pkOrdenMedico = pkOrden,
                    empresaDesc = model.EmpresaDesc,
                    proyectoDesc = model.ProyectoDesc,
                    puestoDesc = model.PuestoDesc,
                    nombreCandidato = (model.NombreCandidato + " " + model.ApellidoCandidato).Trim()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al crear la solicitud: " + ex.Message });
            }
        }

        // GET: /ServicioMedico/ObtenerCatalogos
        [HttpGet]
        public JsonResult ObtenerCatalogos()
        {
            try
            {
                List<CatalogoItem> tiposServicio;
                List<CatalogoItem> empresas;
                CatalogoDal.ObtenerCatalogosParaSolicitud(out tiposServicio, out empresas);

                return Json(new { success = true, tiposServicio = tiposServicio, empresas = empresas }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener catálogos: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /ServicioMedico/CargarInicial — solicitudes + catálogos en 1 sola llamada HTTP
        [HttpGet]
        public JsonResult CargarInicial()
        {
            try
            {
                var solicitudes = OrdenServicioMedicoDal.ObtenerTodas();

                List<CatalogoItem> tiposServicio;
                List<CatalogoItem> empresas;
                CatalogoDal.ObtenerCatalogosParaSolicitud(out tiposServicio, out empresas);

                return Json(new
                {
                    success = true,
                    data = solicitudes,
                    tiposServicio = tiposServicio,
                    empresas = empresas
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cargar datos: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult ProyectosPorEmpresa(int fkEmpresa)
        {
            try
            {
                var proyectos = CatalogoDal.ObtenerProyectosPorEmpresa(fkEmpresa);
                return Json(new { success = true, data = proyectos }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener proyectos: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult PuestosPorEmpresa(int fkEmpresa)
        {
            try
            {
                var puestos = CatalogoDal.ObtenerPuestosPorEmpresa(fkEmpresa);
                return Json(new { success = true, data = puestos }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener puestos: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult ObtenerEmpresas()
        {
            try
            {
                var empresas = CatalogoDal.ObtenerEmpresas();
                return Json(new { success = true, data = empresas }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener empresas: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /ServicioMedico/ImprimirSolicitud?id=1
        public ActionResult ImprimirSolicitud(int id)
        {
            var orden = OrdenServicioMedicoDal.ObtenerPorId(id);
            if (orden == null) return HttpNotFound();

            // Para órdenes PERIÓDICAS (empleado existente), enriquecer con datos del empleado
            if (orden.FkEmpleado.HasValue)
            {
                var emp = EmpleadoDal.BuscarPorNumero(orden.FkEmpleado.Value);
                if (emp != null)
                {
                    orden.PuestoCandidato = emp.PuestoDesc;
                    orden.AreaCandidato   = emp.AreaDesc;
                    // EmpresaNombre viene del JOIN en ObtenerPorId;
                    // si falta, usar el AreaDesc del empleado como referencia
                    if (string.IsNullOrEmpty(orden.EmpresaNombre))
                        orden.EmpresaNombre = emp.AreaDesc;
                    if (string.IsNullOrEmpty(orden.ProyectoDesc))
                        orden.ProyectoDesc = emp.ProyectoDesc;
                }
            }

            return View("~/Views/Solicitud/ImprimirSolicitud.cshtml", orden);
        }

        // POST: /ServicioMedico/Eliminar
        [HttpPost]
        public JsonResult Eliminar(int pkOrdenMedico)
        {
            try
            {
                OrdenServicioMedicoDal.Eliminar(pkOrdenMedico);
                return Json(new { success = true, message = "Solicitud eliminada correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}

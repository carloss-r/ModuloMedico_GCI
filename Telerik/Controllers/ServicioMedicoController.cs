using System;
using Telerik.Models.Entities;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Telerik.Models.DAL;
using Telerik.Models.ViewModels;
using Telerik.Models;
using Telerik.Services;

namespace Telerik.Controllers
{
    public class ServicioMedicoController : Controller
    {
        private readonly MedicalService _medicalService;

        public ServicioMedicoController()
        {
            _medicalService = new MedicalService();
        }

        public ServicioMedicoController(MedicalService medicalService)
        {
            _medicalService = medicalService;
        }
        // GET: /ServicioMedico/
        public ActionResult Index()
        {
            ViewBag.Title = "Servicio Médico - Solicitudes Recibidas";
            return View("~/Views/ServicioMedico/ListaServiciosMedicos.cshtml");
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
            ViewBag.Sexo = _medicalService.NormalizarSexo(orden.SexoCandidato);
            return View("~/Views/ServicioMedico/FormularioEvaluacionMedica.cshtml");
        }

        // GET: /ServicioMedico/ObtenerSolicitudes?pagina=1&tamanoPagina=25
        [HttpGet]
        public JsonResult ObtenerSolicitudes(int pagina = 1, int tamanoPagina = 25)
        {
            try
            {
                // Clamp para evitar abuso: máximo 100 por página
                tamanoPagina = Math.Min(tamanoPagina, 100);
                int total;
                var solicitudes = OrdenServicioMedicoDal.ObtenerTodas(out total, pagina, tamanoPagina);
                return Json(new { success = true, data = solicitudes, total = total }, JsonRequestBehavior.AllowGet);
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
                _medicalService.GuardarEvaluacion(model);
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
                var req = Request.Form;
                int pkOrden = 0;
                if (!int.TryParse(req["PkOrdenMedico"], out pkOrden) || pkOrden == 0)
                    return Json(new { success = false, message = "PkOrdenMedico inválido." });

                var model = new AntidopingVm
                {
                    PkOrdenMedico          = pkOrden,
                    ConsentimientoFirmado  = (req["ConsentimientoFirmado"] ?? "").ToLower().Contains("true"),
                    ResultadoAlcohol       = (req["ResultadoAlcohol"]       ?? "").ToLower().Contains("true"),
                    AplicaAlcohol          = (req["AplicaAlcohol"]          ?? "").ToLower().Contains("true"),
                    ResultadoCocaina       = (req["ResultadoCocaina"]       ?? "").ToLower().Contains("true"),
                    AplicaCocaina          = (req["AplicaCocaina"]          ?? "").ToLower().Contains("true"),
                    ResultadoTHC           = (req["ResultadoTHC"]           ?? "").ToLower().Contains("true"),
                    AplicaTHC              = (req["AplicaTHC"]              ?? "").ToLower().Contains("true"),
                    ResultadoAnfetaminas   = (req["ResultadoAnfetaminas"]   ?? "").ToLower().Contains("true"),
                    AplicaAnfetaminas      = (req["AplicaAnfetaminas"]      ?? "").ToLower().Contains("true"),
                    ResultadoMetanfetaminas= (req["ResultadoMetanfetaminas"] ?? "").ToLower().Contains("true"),
                    AplicaMetanfetaminas   = (req["AplicaMetanfetaminas"]   ?? "").ToLower().Contains("true"),
                    ResultadoOpiaceos      = (req["ResultadoOpiaceos"]      ?? "").ToLower().Contains("true"),
                    AplicaOpiaceos         = (req["AplicaOpiaceos"]         ?? "").ToLower().Contains("true"),
                    ResultadoMetilfenidato = (req["ResultadoMetilfenidato"] ?? "").ToLower().Contains("true"),
                    AplicaMetilfenidato    = (req["AplicaMetilfenidato"]    ?? "").ToLower().Contains("true"),
                    ResultadoFentanilo     = (req["ResultadoFentanilo"]     ?? "").ToLower().Contains("true"),
                    AplicaFentanilo        = (req["AplicaFentanilo"]        ?? "").ToLower().Contains("true"),
                    ResultadoBenzodiacepinas = (req["ResultadoBenzodiacepinas"] ?? "").ToLower().Contains("true"),
                    AplicaBenzodiacepinas  = (req["AplicaBenzodiacepinas"]  ?? "").ToLower().Contains("true"),
                    VeredictoFinal         = req["VeredictoFinal"],
                    Comentarios            = req["Comentarios"]
                };

                // Evidence photo handling
                if (Request.Files["FileEvidencia"] != null && Request.Files["FileEvidencia"].ContentLength > 0)
                {
                    var file = Request.Files["FileEvidencia"];
                    string ext = System.IO.Path.GetExtension(file.FileName);
                    string nombre = _medicalService.GenerarNombreArchivoAntidoping(pkOrden, ext);

                    string carpeta = Server.MapPath("~/Content/Evidencias/Antidoping/");
                    if (!System.IO.Directory.Exists(carpeta))
                        System.IO.Directory.CreateDirectory(carpeta);

                    file.SaveAs(System.IO.Path.Combine(carpeta, nombre));
                    model.UrlFotoEvidencia = "/Content/Evidencias/Antidoping/" + nombre;
                }

                _medicalService.GuardarAntidoping(model);
                _medicalService.CompletarOrden(pkOrden);

                return Json(new { success = true, message = "Antidoping guardado y solicitud COMPLETADA." });
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
                return Json(new { success = false, message = "Error: " + msg });
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
                _medicalService.CompletarOrden(pkOrdenMedico);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: /ServicioMedico/ImprimirEvaluacion/
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
                ViewBag.Paciente = _medicalService.ObtenerInfoPaciente(orden);
                return View("~/Views/ServicioMedico/FormatoImpresionEvaluacionMedica.cshtml", evaluacion);
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
                ViewBag.Paciente = _medicalService.ObtenerInfoPaciente(orden);
                return View("~/Views/ServicioMedico/FormatoImpresionAntidoping.cshtml", antidoping);
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

                var paciente = _medicalService.ObtenerInfoPaciente(orden);

                // Lógica de Expediente Clínico:
                // Intentamos buscar si ya existe una evaluación guardada para ESTA orden
                var evaluacionActual = EvaluacionDal.ObtenerPorOrden(idOrden);
                
                // Si NO hay evaluación guardada para esta orden, buscamos la ÚLTIMA historial del paciente
                var evaluacionPrevia = (evaluacionActual == null) 
                    ? EvaluacionDal.ObtenerUltimaEvaluacionPorPaciente(orden.FkCandidato, orden.FkEmpleado)
                    : null;

                return Json(new { 
                    success = true, 
                    paciente, 
                    evaluacionActual, 
                    evaluacionPrevia,
                    esNuevoExpediente = (evaluacionActual == null && evaluacionPrevia != null)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // End of MedicalService migration


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
                        orden.ProyectoDesc,
                        EmpresaNombre = orden.EmpresaNombre ?? orden.EmpresaCandidato
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

                    // Resolver nombres para el candidato
                    string areaNombre = model.ProyectoDesc;
                    string empresaNombre = model.EmpresaDesc;

                    using (var db = new ApplicationDbContext())
                    {
                        if (string.IsNullOrEmpty(areaNombre) && model.FkProyecto.HasValue && model.FkProyecto > 0)
                        {
                            var pr = db.Proyectos.Find(model.FkProyecto.Value);
                            if (pr != null) areaNombre = pr.descripcion;
                        }
                        if (string.IsNullOrEmpty(empresaNombre) && model.FkEmpresa.HasValue && model.FkEmpresa > 0)
                        {
                            var emp = db.Empresas.Find(model.FkEmpresa.Value);
                            if (emp != null) empresaNombre = emp.nombre;
                        }
                    }

                    // Crear candidato nuevo
                    int pkCandidato = CandidatoDal.Insertar(
                        model.NombreCandidato,
                        model.ApellidoPaterno,
                        model.ApellidoMaterno,
                        model.PuestoDesc ?? model.PuestoDeseado,
                        areaNombre,
                        empresaNombre,
                        model.Sexo ?? "" // Sexo desde la solicitud - No defaultear a M
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
                    nombreCandidato = (model.NombreCandidato + " " + model.ApellidoPaterno + " " + (model.ApellidoMaterno ?? "")).Trim()
                });
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                {
                    msg += " | Detalle: " + ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                        msg += " | Sub-detalle: " + ex.InnerException.InnerException.Message;
                }
                return Json(new { success = false, message = "Error al crear la solicitud: " + msg });
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
                int totalRegistros;
                var solicitudes = OrdenServicioMedicoDal.ObtenerTodas(out totalRegistros, 1, 10);

                List<CatalogoItem> tiposServicio;
                List<CatalogoItem> empresas;
                CatalogoDal.ObtenerCatalogosParaSolicitud(out tiposServicio, out empresas);

                return Json(new
                {
                    success = true,
                    data = solicitudes,
                    total = totalRegistros,
                    tiposServicio = tiposServicio,
                    empresas = empresas
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText(Server.MapPath("~/error.txt"), ex.ToString());
                return Json(new { success = false, message = "Error al cargar datos iniciales: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult CargarPagina(
            int? pagina,
            int tamanoPagina,
            int? filtroNumEmpleado,
            string filtroModalidad,
            int? filtroEstatus,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int? filtroEmpresa = null,
            int? filtroArea = null,
            int? filtroAnio = null,
            int? filtroSemana = null)
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
                    filtroSemana
                );

                return Json(new { success = true, data = solicitudes, total = totalRegistros });
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText(Server.MapPath("~/error_pagina.txt"), ex.ToString());
                return Json(new { success = false, message = "Error al cargar la página: " + ex.Message });
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

            return View("~/Views/Solicitud/FormatoImpresionInstruccionesRH.cshtml", orden);
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

        [HttpGet]
        public JsonResult ObtenerFiltrosSidebar()
        {
            try
            {
                var empresas = CatalogoDal.ObtenerEmpresas();
                var areas = CatalogoDal.ObtenerAreas();
                
                // Generar años (2024 al actual + 1)
                var anios = new List<object>();
                int anioActual = DateTime.Now.Year;
                for (int i = anioActual + 1; i >= 2024; i--)
                    anios.Add(new { Id = i, Descripcion = i.ToString() });

                // Semanas (1 a 53)
                var semanas = new List<object>();
                for (int i = 1; i <= 53; i++)
                    semanas.Add(new { Id = i, Descripcion = "SEMANA " + i });

                return Json(new { success = true, empresas, areas, anios, semanas }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // --- ENDPOINTS CATÁLOGOS GEOGRÁFICOS ---
        [HttpGet]
        public JsonResult ObtenerPaises()
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var data = db.Paises
                        .OrderBy(p => p.descripcion)
                        .Select(p => new CatalogoItem { Id = p.pkPais, Descripcion = p.descripcion })
                        .ToList();
                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if(ex.InnerException != null) msg += " | " + ex.InnerException.Message;
                return Json(new { success = false, message = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult ObtenerEstados(int idPais)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var data = db.Estados
                        .Where(e => e.fkPais == idPais)
                        .OrderBy(e => e.descripcion)
                        .Select(e => new CatalogoItem { Id = e.pkEstado, Descripcion = e.descripcion })
                        .ToList();
                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult ObtenerMunicipios(int idEstado)
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
                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult ObtenerColonias(int idMunicipio)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var data = db.Colonias
                        .Where(c => c.fkMunicipio == idMunicipio)
                        .OrderBy(c => c.descripcion)
                        .Select(c => new {
                            Id = c.pkColonia,
                            Descripcion = c.descripcion,
                            CodigoPostal = ""
                        })
                        .ToList();
                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}

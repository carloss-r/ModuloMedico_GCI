    using System;
using Telerik.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Telerik.Models;
using Telerik.Models.ViewModels;

namespace Telerik.Models.DAL
{
    public class OrdenServicioMedicoDal
    {
        public static List<OrdenServicioMedicoVm> ObtenerTodas(
            out int totalRegistros,
            int pagina = 1, 
            int tamanoPagina = 10,
            int? filtroNumEmpleado = null,
            string filtroModalidad = null,
            int? filtroEstatus = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? filtroEmpresa = null,
            int? filtroArea = null,
            int? filtroAnio = null,
            int? filtroSemana = null)
        {
            using (var db = new ApplicationDbContext())
            {
                // Pre-cargar descripciones para filtros de Candidatos (que usan strings)
                string areaDesc = null;
                if (filtroArea.HasValue && filtroArea.Value > 0)
                {
                    var a = db.Areas.Find(filtroArea.Value);
                    if (a != null) areaDesc = a.descripcion;
                }

                string empresaNombre = null;
                if (filtroEmpresa.HasValue && filtroEmpresa.Value > 0)
                {
                    var e = db.Empresas.Find(filtroEmpresa.Value);
                    if (e != null) empresaNombre = e.nombre;
                }

                var query = from o in db.OrdenesMedicas
                            // Tipo de servicio (INNER JOIN, siempre existe)

                            join ts in db.TiposServicio on o.fkTipoServicio equals ts.pkTipoServicio
                            // Estatus (LEFT JOIN)
                            join es in db.EstatusSolicitudes on o.fkEstatus equals es.pkEstatus into esjoin
                            from es in esjoin.DefaultIfEmpty()
                            // Empleado (LEFT JOIN)
                            join emp in db.Empleados on o.fkEmpleado equals emp.pkEmpleado into empjoin
                            from emp in empjoin.DefaultIfEmpty()
                            // Candidato (LEFT JOIN)
                            join cand in db.Candidatos on o.fkCandidato equals cand.pkCandidato into candjoin
                            from cand in candjoin.DefaultIfEmpty()
                            // Proyecto (LEFT JOIN)
                            join pr in db.Proyectos on o.fkProyecto equals pr.pkProyecto into prjoin
                            from pr in prjoin.DefaultIfEmpty()

                            // Empresa del Proyecto (LEFT JOIN)
                            join emp_pr in db.Empresas on pr.fkEmpresa equals emp_pr.pkEmpresa into emp_pr_join
                            from emp_pr in emp_pr_join.DefaultIfEmpty()

                            // Empresa del Empleado (LEFT JOIN)
                            join emp_e in db.Empresas on emp.fkEmpresa equals emp_e.pkEmpresa into emp_e_join
                            from emp_e in emp_e_join.DefaultIfEmpty()

                            // Proyecto del Empleado (LEFT JOIN - Fallback)
                            join p_emp in db.Proyectos on emp.fkProyecto equals p_emp.pkProyecto into p_emp_join
                            from p_emp in p_emp_join.DefaultIfEmpty()
                            // Evaluación Clínica (LEFT JOIN) para obtener Aptitud
                            join eval in db.EvaluacionesClinicas on o.pkOrdenMedico equals eval.fkOrdenMedico into evaljoin
                            from eval in evaljoin.DefaultIfEmpty()
                            // Antidoping (LEFT JOIN) para veredictos de antidoping puros
                            join anti in db.PruebasToxicologicas on o.pkOrdenMedico equals anti.fkOrdenMedico into antijoin
                            from anti in antijoin.DefaultIfEmpty()
                            // Puesto (para Area del empleado)
                            join p in db.Puestos on emp.fkPuesto equals p.pkPuesto into pjoin
                            from p in pjoin.DefaultIfEmpty()
                            // Area (para nombre de area del empleado)
                            join a in db.Areas on p.fkArea equals a.pkArea into ajoin
                            from a in ajoin.DefaultIfEmpty()
                            select new { o, ts, es, emp, cand, pr, eval, anti, p, a, emp_pr, emp_e, p_emp };

                // IQueryable dinámico. No evaluado aún.
                var q = query.AsQueryable();

                // Aplicar Filtros SERVER-SIDE
                if (filtroNumEmpleado.HasValue)
                    q = q.Where(x => x.o.fkEmpleado == filtroNumEmpleado.Value);

                if (!string.IsNullOrEmpty(filtroModalidad))
                {
                    if (filtroModalidad == "PERIODICO")
                        q = q.Where(x => x.o.fkEmpleado != null);
                    else if (filtroModalidad == "INGRESO")
                        q = q.Where(x => x.o.fkEmpleado == null && x.o.fkTipoServicio != 3); // 3=Antidoping
                    else if (filtroModalidad == "ANTIDOPING")
                        q = q.Where(x => x.o.fkTipoServicio == 3);
                }

                // -1 es el filtro especial "Activas" = Pendiente(1) + En Proceso(2)
                if (filtroEstatus.HasValue)
                {
                    if (filtroEstatus.Value == -1)
                        q = q.Where(x => x.o.fkEstatus == 1 || x.o.fkEstatus == 2);
                    else
                        q = q.Where(x => x.o.fkEstatus == filtroEstatus.Value);
                }

                if (fechaDesde.HasValue)
                {
                    var fh = fechaDesde.Value.Date;
                    q = q.Where(x => x.o.fechaOrden >= fh);
                }
                
                if (fechaHasta.HasValue)
                {
                    var fh = fechaHasta.Value.Date.AddDays(1).AddTicks(-1);
                    q = q.Where(x => x.o.fechaOrden <= fh);
                }

                // Nuevos filtros del Sidebar
                if (filtroEmpresa.HasValue && filtroEmpresa.Value > 0)
                {
                    q = q.Where(x => (x.pr != null && x.pr.fkEmpresa == filtroEmpresa.Value) || 
                                     (x.emp != null && x.emp.fkEmpresa == filtroEmpresa.Value) ||
                                     (x.cand != null && x.cand.empresa == empresaNombre));
                }

                if (filtroArea.HasValue && filtroArea.Value > 0)
                {
                    // Comparar por ID de área (para empleados vía puesto) o por descripción (candidatos)
                    q = q.Where(x => (x.p != null && x.p.fkArea == filtroArea.Value) ||
                                     (x.cand != null && x.cand.area == areaDesc));
                }

                if (filtroAnio.HasValue && filtroAnio.Value > 0)
                {
                    int anioVal = filtroAnio.Value;
                    if (filtroSemana.HasValue && filtroSemana.Value > 0)
                    {
                        // Filtro por semana (simplificado: 1er día del año + (semana-1)*7)
                        DateTime start = new DateTime(anioVal, 1, 1);
                        while (start.DayOfWeek != DayOfWeek.Monday) start = start.AddDays(1);
                        start = start.AddDays((filtroSemana.Value - 1) * 7);
                        DateTime end = start.AddDays(7).AddTicks(-1);
                        
                        q = q.Where(x => x.o.fechaOrden >= start && x.o.fechaOrden <= end);
                    }
                    else
                    {
                        q = q.Where(x => x.o.fechaOrden.Value.Year == anioVal);
                    }
                }

                // Ejecuta COUNT de acuerdo a los filtros aplicados y devuelve el total AL CONTROLADOR
                totalRegistros = q.Count();

                // Aplicar Proyección y Paginación, y ejecutar en Base de Datos
                var resultados = q
                    .OrderByDescending(x => x.o.fechaOrden)
                    .ThenByDescending(x => x.o.pkOrdenMedico)
                    .Skip((pagina - 1) * tamanoPagina)
                    .Take(tamanoPagina)
                    .AsNoTracking() // LIBERAR de memoria. Ultra rápido
                    .Select(x => new OrdenServicioMedicoVm
                    {
                        PkOrdenMedico    = x.o.pkOrdenMedico,
                        FechaOrden       = x.o.fechaOrden,
                        FkEstatus        = x.o.fkEstatus,
                        EstatusDesc      = x.es != null ? x.es.descripcion : "Sin Estatus",
                        FkTipoServicio   = x.o.fkTipoServicio,
                        TipoServicioDesc = x.ts.descripcion,
                        Modalidad        = x.ts.descripcion,
                        FkEmpleado       = x.o.fkEmpleado,
                        FkCandidato      = x.o.fkCandidato,
                        NombrePersona    = x.emp != null
                            ? (x.emp.nombre + " " + x.emp.aPaterno + " " + x.emp.aMaterno).Trim()
                            : x.cand != null ? (x.cand.nombre + " " + x.cand.aPaterno + " " + (x.cand.aMaterno ?? "")).Trim() : "Sin Nombre",
                        ProyectoDesc     = x.pr != null ? x.pr.descripcion : (x.p_emp != null ? x.p_emp.descripcion : (x.cand != null ? x.cand.area : null)),
                        FkProyecto       = x.o.fkProyecto,
                        EmpresaNombre    = x.pr != null ? x.emp_pr.nombre : (x.emp != null ? x.emp_e.nombre : null),
                        _FkEmpresa       = x.pr != null ? (int?)x.pr.fkEmpresa : (x.emp != null ? x.emp.fkEmpresa : null),
                        PuestoCandidato  = x.cand != null ? x.cand.puestoDeseado : (x.p != null ? x.p.descripcion : null),
                        AreaCandidato    = x.cand != null ? x.cand.area : (x.a != null ? x.a.descripcion : null),
                        EmpresaCandidato = x.cand != null ? x.cand.empresa : null,
                        SexoCandidato    = x.emp != null ? x.emp.fkSexo : (x.cand != null ? x.cand.fkSexo : null),
                        // Determinar Aptitud Médica
                        FkAptitudMedica  = x.o.fkTipoServicio == 3 
                            ? (x.anti != null 
                                ? (x.anti.veredictoFinal != null && x.anti.veredictoFinal.ToUpper().Contains("APTO") && !x.anti.veredictoFinal.ToUpper().Contains("NO APTO") ? 1 : 
                                   x.anti.veredictoFinal != null && x.anti.veredictoFinal.ToUpper().Contains("NO APTO") ? 3 : (int?)null) 
                                : (int?)null)
                            : (x.eval != null ? x.eval.fkAptitudMedica : (int?)null),
                        Recomendaciones = x.o.fkTipoServicio == 3 ? (x.anti != null ? x.anti.veredictoFinal : null) : (x.eval != null ? x.eval.recomendaciones : null)
                    })
                    .ToList();

                return resultados;
            }
        }

        public static int ContarTodas()
        {
            using (var db = new ApplicationDbContext())
            {
                return db.OrdenesMedicas.Count();
            }
        }

        public static OrdenServicioMedicoVm ObtenerPorId(int pkOrden)
        {
            using (var db = new ApplicationDbContext())
            {
                var vm = (from o in db.OrdenesMedicas
                          join ts in db.TiposServicio on o.fkTipoServicio equals ts.pkTipoServicio
                          join es in db.EstatusSolicitudes on o.fkEstatus equals es.pkEstatus into esjoin
                          from es in esjoin.DefaultIfEmpty()
                          join emp in db.Empleados on o.fkEmpleado equals emp.pkEmpleado into empjoin
                          from emp in empjoin.DefaultIfEmpty()
                          join cand in db.Candidatos on o.fkCandidato equals cand.pkCandidato into candjoin
                          from cand in candjoin.DefaultIfEmpty()
                          join pr in db.Proyectos on o.fkProyecto equals pr.pkProyecto into prjoin
                          from pr in prjoin.DefaultIfEmpty()

                          // Empresa del Proyecto
                          join emp_pr in db.Empresas on pr.fkEmpresa equals emp_pr.pkEmpresa into emp_pr_join
                          from emp_pr in emp_pr_join.DefaultIfEmpty()

                          // Empresa del Empleado
                          join emp_e in db.Empresas on emp.fkEmpresa equals emp_e.pkEmpresa into emp_e_join
                          from emp_e in emp_e_join.DefaultIfEmpty()

                          // Proyecto del Empleado (Fallback)
                          join p_emp in db.Proyectos on emp.fkProyecto equals p_emp.pkProyecto into p_emp_join
                          from p_emp in p_emp_join.DefaultIfEmpty()

                          join eval in db.EvaluacionesClinicas on o.pkOrdenMedico equals eval.fkOrdenMedico into evaljoin
                          from eval in evaljoin.DefaultIfEmpty()
                          join anti in db.PruebasToxicologicas on o.pkOrdenMedico equals anti.fkOrdenMedico into antijoin
                          from anti in antijoin.DefaultIfEmpty()

                          // Puesto y Area (para AreaCandidato fallback si aplica o datos de empleado)
                          join p in db.Puestos on emp.fkPuesto equals p.pkPuesto into pjoin
                          from p in pjoin.DefaultIfEmpty()
                          join a in db.Areas on p.fkArea equals a.pkArea into ajoin
                          from a in ajoin.DefaultIfEmpty()

                          where o.pkOrdenMedico == pkOrden
                          select new OrdenServicioMedicoVm
                          {
                              PkOrdenMedico    = o.pkOrdenMedico,
                              FechaOrden       = o.fechaOrden,
                              FkEstatus        = o.fkEstatus,
                              EstatusDesc      = es != null ? es.descripcion : "Sin Estatus",
                              FkTipoServicio   = o.fkTipoServicio,
                              TipoServicioDesc = ts.descripcion,
                              Modalidad        = ts.descripcion,
                              FkEmpleado       = o.fkEmpleado,
                              FkCandidato      = o.fkCandidato,
                              NombrePersona    = emp != null
                                  ? (emp.nombre + " " + emp.aPaterno + " " + emp.aMaterno).Trim()
                                  : cand != null ? (cand.nombre + " " + cand.aPaterno + " " + (cand.aMaterno ?? "")).Trim() : "Sin Nombre",
                              ProyectoDesc     = pr != null ? pr.descripcion : (p_emp != null ? p_emp.descripcion : (cand != null ? cand.area : null)),
                              FkProyecto       = o.fkProyecto,
                              EmpresaNombre    = pr != null ? emp_pr.nombre : (emp != null ? emp_e.nombre : null),
                              _FkEmpresa       = pr != null ? (int?)pr.fkEmpresa : (emp != null ? emp.fkEmpresa : null),
                              PuestoCandidato  = cand != null ? cand.puestoDeseado : (p != null ? p.descripcion : null),
                              AreaCandidato    = cand != null ? cand.area : (a != null ? a.descripcion : null),
                              EmpresaCandidato = cand != null ? cand.empresa : null,
                              SexoCandidato    = emp != null ? emp.fkSexo : (cand != null ? cand.fkSexo : null),
                              FkAptitudMedica  = o.fkTipoServicio == 3 
                                  ? (anti != null 
                                      ? (anti.veredictoFinal != null && anti.veredictoFinal.ToUpper().Contains("APTO") && !anti.veredictoFinal.ToUpper().Contains("NO APTO") ? 1 : 
                                         anti.veredictoFinal != null && anti.veredictoFinal.ToUpper().Contains("NO APTO") ? 3 : (int?)null) 
                                      : (int?)null)
                                  : (eval != null ? eval.fkAptitudMedica : (int?)null),
                              Recomendaciones = o.fkTipoServicio == 3 ? (anti != null ? anti.veredictoFinal : null) : (eval != null ? eval.recomendaciones : null)
                          }).FirstOrDefault();

                return vm;
            }
        }

        public static int Insertar(int? fkEmpleado, int? fkCandidato, int? fkProyecto, int fkTipoServicio)
        {
            using (var db = new ApplicationDbContext())
            {
                var orden = new OrdenServicioMedico
                {
                    fkEmpleado     = fkEmpleado,
                    fkCandidato    = fkCandidato,
                    fkTipoServicio = fkTipoServicio,
                    fkProyecto     = fkProyecto,
                    fkEstatus      = 1,
                    fechaOrden     = DateTime.Now
                };
                db.OrdenesMedicas.Add(orden);
                db.SaveChanges();
                return orden.pkOrdenMedico;
            }
        }

        public static int CrearParaEmpleado(int pkEmpleado, int fkTipoServicio, int? fkProyecto)
        {
            return Insertar(pkEmpleado, null, fkProyecto, fkTipoServicio);
        }

        public static int CrearParaCandidato(int pkCandidato, int fkTipoServicio, int? fkProyecto)
        {
            return Insertar(null, pkCandidato, fkProyecto, fkTipoServicio);
        }

        public static void ActualizarEstatus(int pkOrden, int nuevoEstatus)
        {
            using (var db = new ApplicationDbContext())
            {
                var orden = db.OrdenesMedicas.Find(pkOrden);
                if (orden != null)
                {
                    orden.fkEstatus = nuevoEstatus;
                    db.SaveChanges();
                }
            }
        }

        public static void Eliminar(int pkOrden)
        {
            using (var db = new ApplicationDbContext())
            {
                var orden = db.OrdenesMedicas.Find(pkOrden);
                if (orden != null)
                {
                    db.OrdenesMedicas.Remove(orden);
                    db.SaveChanges();
                }
            }
        }
    }
}

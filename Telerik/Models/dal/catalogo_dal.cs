using System.Collections.Generic;
using System.Linq;
using Telerik.Models;
using Telerik.Models.ViewModels;

namespace Telerik.Models.Dal
{
    public class CatalogoDal
    {
        public static List<CatalogoItem> ObtenerTiposServicio()
        {
            using (var db = new ApplicationDbContext())
            {
                return db.TiposServicio
                    .Select(t => new CatalogoItem { Id = t.pkTipoServicio, Descripcion = t.descripcion })
                    .OrderBy(t => t.Descripcion)
                    .ToList();
            }
        }

        public static List<CatalogoItem> ObtenerEstatusOrdenes()
        {
            using (var db = new ApplicationDbContext())
            {
                return db.EstatusSolicitudes
                    .Select(e => new CatalogoItem { Id = e.pkEstatus, Descripcion = e.descripcion })
                    .OrderBy(e => e.Id)
                    .ToList();
            }
        }

        public static List<CatalogoItem> ObtenerProyectos()
        {
            using (var db = new ApplicationDbContext())
            {
                return db.Proyectos
                    .Select(p => new CatalogoItem { Id = p.pkProyecto, Descripcion = p.descripcion })
                    .OrderBy(p => p.Descripcion)
                    .ToList();
            }
        }

        public static List<CatalogoItem> ObtenerEmpresas()
        {
            using (var db = new ApplicationDbContext())
            {
                return db.Empresas
                    .Select(e => new CatalogoItem { Id = e.pkEmpresa, Descripcion = e.nombre })
                    .OrderBy(e => e.Descripcion)
                    .ToList();
            }
        }

        public static List<CatalogoItem> ObtenerPuestos()
        {
            using (var db = new ApplicationDbContext())
            {
                return db.Puestos
                    .Select(p => new CatalogoItem { Id = p.pkPuesto, Descripcion = p.descripcion })
                    .OrderBy(p => p.Descripcion)
                    .ToList();
            }
        }

        public static List<CatalogoItem> ObtenerAreas()
        {
            using (var db = new ApplicationDbContext())
            {
                return db.Areas
                    .Select(a => new CatalogoItem { Id = a.pkArea, Descripcion = a.descripcion })
                    .OrderBy(a => a.Descripcion)
                    .ToList();
            }
        }

        /// <summary>
        /// Método de compatibilidad con el Controller actual.
        /// Retorna TiposServicio y Empresas juntos via parámetros out.
        /// </summary>
        public static void ObtenerCatalogosParaSolicitud(out List<CatalogoItem> tiposServicio, out List<CatalogoItem> empresas)
        {
            tiposServicio = ObtenerTiposServicio();
            empresas      = ObtenerEmpresas();
        }

        /// <summary>
        /// Proyectos filtrados por empresa, en LINQ puro.
        /// </summary>
        public static List<CatalogoItem> ObtenerProyectosPorEmpresa(int fkEmpresa)
        {
            using (var db = new ApplicationDbContext())
            {
                return db.Proyectos
                    .Where(p => p.fkEmpresa == fkEmpresa)
                    .Select(p => new CatalogoItem { Id = p.pkProyecto, Descripcion = p.descripcion })
                    .OrderBy(p => p.Descripcion)
                    .ToList();
            }
        }

        /// <summary>
        /// Puestos filtrados por empresa, en LINQ puro.
        /// </summary>
        public static List<CatalogoItem> ObtenerPuestosPorEmpresa(int fkEmpresa)
        {
            using (var db = new ApplicationDbContext())
            {
                return db.Puestos
                    .Where(p => p.fkEmpresa == fkEmpresa)
                    .Select(p => new CatalogoItem { Id = p.pkPuesto, Descripcion = p.descripcion })
                    .OrderBy(p => p.Descripcion)
                    .ToList();
            }
        }
    }
}

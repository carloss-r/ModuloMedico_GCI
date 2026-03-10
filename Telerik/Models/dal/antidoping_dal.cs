using System;
using Telerik.Models.Entities;
using System.Linq;
using Telerik.Models;
using Telerik.Models.ViewModels;

namespace Telerik.Models.Dal
{
    public class AntidopingDal
    {
        /// <summary>
        /// Guarda una prueba toxicológica nueva y actualiza el estatus de la orden a Completado (3).
        /// Usa EF puro: Add + Find + property set + SaveChanges.
        /// </summary>
        public static void GuardarAntidoping(AntidopingVm vm)
        {
            using (var db = new ApplicationDbContext())
            {
                using (var transaccion = db.Database.BeginTransaction())
                {
                    try
                    {
                        // 1. Insertar prueba toxicológica
                        var prueba = new PruebaToxicologica
                        {
                            fkOrdenMedico         = vm.PkOrdenMedico,
                            codigoMuestra         = vm.CodigoMuestra,
                            consentimientoFirmado = vm.ConsentimientoFirmado,
                            resultadoOpiaceos     = vm.ResultadoOpiaceos,
                            resultadoCocaina      = vm.ResultadoCocaina,
                            resultadoTHC          = vm.ResultadoTHC,
                            veredictoFinal        = vm.VeredictoFinal,
                            comentarios           = vm.Comentarios,
                            urlFotoEvidencia      = vm.UrlFotoEvidencia
                        };
                        db.PruebasToxicologicas.Add(prueba);

                        // 2. Actualizar estatus de la orden a Completado (3)
                        var orden = db.OrdenesMedicas.Find(vm.PkOrdenMedico);
                        if (orden != null)
                        {
                            orden.fkEstatus = 3; // 3 = Completado
                        }

                        db.SaveChanges();
                        transaccion.Commit();
                    }
                    catch (Exception)
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Obtiene los datos de antidoping de una orden existente.
        /// </summary>
        public static AntidopingVm ObtenerPorOrden(int pkOrden)
        {
            using (var db = new ApplicationDbContext())
            {
                return db.PruebasToxicologicas
                    .Where(p => p.fkOrdenMedico == pkOrden)
                    .Select(p => new AntidopingVm
                    {
                        PkOrdenMedico         = p.fkOrdenMedico,
                        CodigoMuestra         = p.codigoMuestra,
                        ConsentimientoFirmado = p.consentimientoFirmado ?? false,
                        ResultadoOpiaceos     = p.resultadoOpiaceos ?? false,
                        ResultadoCocaina      = p.resultadoCocaina ?? false,
                        ResultadoTHC          = p.resultadoTHC ?? false,
                        VeredictoFinal        = p.veredictoFinal,
                        Comentarios           = p.comentarios,
                        UrlFotoEvidencia      = p.urlFotoEvidencia
                    })
                    .FirstOrDefault();
            }
        }
    }
}

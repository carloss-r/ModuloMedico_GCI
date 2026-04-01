using System;
using Telerik.Models.Entities;
using System.Linq;
using Telerik.Models;
using Telerik.Models.ViewModels;

namespace Telerik.Models.DAL
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
                            fkOrdenMedico          = vm.PkOrdenMedico,
                            codigoMuestra          = vm.CodigoMuestra ?? "N/A",
                            consentimientoFirmado  = vm.ConsentimientoFirmado,
                            resultadoAlcohol       = vm.AplicaAlcohol ? vm.ResultadoAlcohol : (bool?)null,
                            resultadoCocaina       = vm.AplicaCocaina ? vm.ResultadoCocaina : (bool?)null,
                            resultadoTHC           = vm.AplicaTHC     ? vm.ResultadoTHC     : (bool?)null,
                            resultadoAnfetaminas   = vm.AplicaAnfetaminas ? vm.ResultadoAnfetaminas : (bool?)null,
                            resultadoMetanfetaminas = vm.AplicaMetanfetaminas ? vm.ResultadoMetanfetaminas : (bool?)null,
                            resultadoOpiaceos      = vm.AplicaOpiaceos    ? vm.ResultadoOpiaceos    : (bool?)null,
                            resultadoMetilfenidato = vm.AplicaMetilfenidato ? vm.ResultadoMetilfenidato : (bool?)null,
                            resultadoFentanilo     = vm.AplicaFentanilo    ? vm.ResultadoFentanilo    : (bool?)null,
                            resultadoBenzodiacepinas = vm.AplicaBenzodiacepinas ? vm.ResultadoBenzodiacepinas : (bool?)null,
                            veredictoFinal         = vm.VeredictoFinal ?? "NEGATIVO",
                            comentarios           = vm.Comentarios ?? "",
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
                        PkOrdenMedico          = p.fkOrdenMedico,
                        CodigoMuestra          = p.codigoMuestra,
                        ConsentimientoFirmado  = p.consentimientoFirmado ?? false,
                        ResultadoAlcohol       = p.resultadoAlcohol ?? false,
                        AplicaAlcohol          = p.resultadoAlcohol != null,
                        ResultadoCocaina       = p.resultadoCocaina ?? false,
                        AplicaCocaina          = p.resultadoCocaina != null,
                        ResultadoTHC           = p.resultadoTHC ?? false,
                        AplicaTHC              = p.resultadoTHC != null,
                        ResultadoAnfetaminas   = p.resultadoAnfetaminas ?? false,
                        AplicaAnfetaminas      = p.resultadoAnfetaminas != null,
                        ResultadoMetanfetaminas = p.resultadoMetanfetaminas ?? false,
                        AplicaMetanfetaminas   = p.resultadoMetanfetaminas != null,
                        ResultadoOpiaceos      = p.resultadoOpiaceos ?? false,
                        AplicaOpiaceos         = p.resultadoOpiaceos != null,
                        ResultadoMetilfenidato = p.resultadoMetilfenidato ?? false,
                        AplicaMetilfenidato    = p.resultadoMetilfenidato != null,
                        ResultadoFentanilo     = p.resultadoFentanilo ?? false,
                        AplicaFentanilo        = p.resultadoFentanilo != null,
                        ResultadoBenzodiacepinas = p.resultadoBenzodiacepinas ?? false,
                        AplicaBenzodiacepinas  = p.resultadoBenzodiacepinas != null,
                        VeredictoFinal         = p.veredictoFinal,
                        Comentarios           = p.comentarios,
                        UrlFotoEvidencia      = p.urlFotoEvidencia
                    })
                    .FirstOrDefault();
            }
        }
    }
}

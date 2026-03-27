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
                            codigoMuestra          = vm.CodigoMuestra,
                            consentimientoFirmado  = vm.ConsentimientoFirmado,
                            resultadoAlcohol       = vm.ResultadoAlcohol,
                            aplicaAlcohol          = vm.AplicaAlcohol,
                            resultadoCocaina       = vm.ResultadoCocaina,
                            aplicaCocaina          = vm.AplicaCocaina,
                            resultadoTHC           = vm.ResultadoTHC,
                            aplicaTHC              = vm.AplicaTHC,
                            resultadoAnfetaminas   = vm.ResultadoAnfetaminas,
                            aplicaAnfetaminas      = vm.AplicaAnfetaminas,
                            resultadoMetanfetaminas= vm.ResultadoMetanfetaminas,
                            aplicaMetanfetaminas   = vm.AplicaMetanfetaminas,
                            resultadoOpiaceos      = vm.ResultadoOpiaceos,
                            aplicaOpiaceos         = vm.AplicaOpiaceos,
                            resultadoMetilfenidato = vm.ResultadoMetilfenidato,
                            aplicaMetilfenidato    = vm.AplicaMetilfenidato,
                            resultadoFentanilo     = vm.ResultadoFentanilo,
                            aplicaFentanilo        = vm.AplicaFentanilo,
                            resultadoBenzodiacepinas = vm.ResultadoBenzodiacepinas,
                            aplicaBenzodiacepinas  = vm.AplicaBenzodiacepinas,
                            veredictoFinal         = vm.VeredictoFinal,
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
                        PkOrdenMedico          = p.fkOrdenMedico,
                        CodigoMuestra          = p.codigoMuestra,
                        ConsentimientoFirmado  = p.consentimientoFirmado ?? false,
                        ResultadoAlcohol       = p.resultadoAlcohol ?? false,
                        AplicaAlcohol          = p.aplicaAlcohol ?? false,
                        ResultadoCocaina       = p.resultadoCocaina ?? false,
                        AplicaCocaina          = p.aplicaCocaina ?? false,
                        ResultadoTHC           = p.resultadoTHC ?? false,
                        AplicaTHC              = p.aplicaTHC ?? false,
                        ResultadoAnfetaminas   = p.resultadoAnfetaminas ?? false,
                        AplicaAnfetaminas      = p.aplicaAnfetaminas ?? false,
                        ResultadoMetanfetaminas= p.resultadoMetanfetaminas ?? false,
                        AplicaMetanfetaminas   = p.aplicaMetanfetaminas ?? false,
                        ResultadoOpiaceos      = p.resultadoOpiaceos ?? false,
                        AplicaOpiaceos         = p.aplicaOpiaceos ?? false,
                        ResultadoMetilfenidato = p.resultadoMetilfenidato ?? false,
                        AplicaMetilfenidato    = p.aplicaMetilfenidato ?? false,
                        ResultadoFentanilo     = p.resultadoFentanilo ?? false,
                        AplicaFentanilo        = p.aplicaFentanilo ?? false,
                        ResultadoBenzodiacepinas = p.resultadoBenzodiacepinas ?? false,
                        AplicaBenzodiacepinas  = p.aplicaBenzodiacepinas ?? false,
                        VeredictoFinal         = p.veredictoFinal,
                        Comentarios           = p.comentarios,
                        UrlFotoEvidencia      = p.urlFotoEvidencia
                    })
                    .FirstOrDefault();
            }
        }
    }
}

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
                        // 1. Buscar si ya existe la prueba para esta orden
                        var prueba = db.PruebasToxicologicas.FirstOrDefault(p => p.fkOrdenMedico == vm.PkOrdenMedico);
                        if (prueba == null)
                        {
                            prueba = new PruebaToxicologica { fkOrdenMedico = vm.PkOrdenMedico };
                            db.PruebasToxicologicas.Add(prueba);
                        }

                        prueba.codigoMuestra          = vm.CodigoMuestra ?? "N/A";
                        prueba.consentimientoFirmado  = vm.ConsentimientoFirmado;
                        prueba.resultadoAlcohol       = vm.AplicaAlcohol ? vm.ResultadoAlcohol : (bool?)null;
                        prueba.resultadoCocaina       = vm.AplicaCocaina ? vm.ResultadoCocaina : (bool?)null;
                        prueba.resultadoTHC           = vm.AplicaTHC     ? vm.ResultadoTHC     : (bool?)null;
                        prueba.resultadoAnfetaminas   = vm.AplicaAnfetaminas ? vm.ResultadoAnfetaminas : (bool?)null;
                        prueba.resultadoMetanfetaminas = vm.AplicaMetanfetaminas ? vm.ResultadoMetanfetaminas : (bool?)null;
                        prueba.resultadoOpiaceos      = vm.AplicaOpiaceos    ? vm.ResultadoOpiaceos    : (bool?)null;
                        prueba.resultadoMetilfenidato = vm.AplicaMetilfenidato ? vm.ResultadoMetilfenidato : (bool?)null;
                        prueba.resultadoFentanilo     = vm.AplicaFentanilo    ? vm.ResultadoFentanilo    : (bool?)null;
                        prueba.resultadoBenzodiacepinas = vm.AplicaBenzodiacepinas ? vm.ResultadoBenzodiacepinas : (bool?)null;
                        prueba.veredictoFinal         = vm.VeredictoFinal ?? "NEGATIVO";
                        prueba.comentarios           = vm.Comentarios ?? "";
                        
                        if(!string.IsNullOrEmpty(vm.UrlFotoEvidencia))
                            prueba.urlFotoEvidencia = vm.UrlFotoEvidencia;

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

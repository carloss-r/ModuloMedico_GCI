using System;
using Telerik.Models.Entities;
using Telerik.Models;

namespace Telerik.Models.Dal
{
    public class CandidatoDal
    {
        public static int Insertar(string nombre, string aPaterno, string puestoDeseado)
        {
            using (var db = new ApplicationDbContext())
            {
                var candidato = new Candidato
                {
                    nombre         = nombre,
                    aPaterno       = aPaterno,
                    puestoDeseado  = puestoDeseado,
                    fechaRegistro  = DateTime.Now
                };

                db.Candidatos.Add(candidato);
                db.SaveChanges();

                return candidato.pkCandidato;
            }
        }
    }
}

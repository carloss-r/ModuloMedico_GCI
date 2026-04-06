using System;
using Telerik.Models.Entities;
using Telerik.Models;

namespace Telerik.Models.DAL
{
    public class CandidatoDal
    {
        public static int Insertar(string nombre, string aPaterno, string aMaterno = null, string puestoDeseado = null, string area = null, string empresa = null, string sexo = "")
        {
            using (var db = new ApplicationDbContext())
            {
                var candidato = new Candidato
                {
                    nombre         = nombre,
                    aPaterno       = aPaterno,
                    aMaterno       = aMaterno,
                    puestoDeseado  = puestoDeseado,
                    area           = area,
                    empresa        = empresa,
                    fkSexo         = !string.IsNullOrEmpty(sexo) ? sexo : (string)null,
                    fechaRegistro  = DateTime.Now
                };

                db.Candidatos.Add(candidato);
                db.SaveChanges();

                return candidato.pkCandidato;
            }
        }
    }
}

    using System;
using System.Data.SqlClient;

namespace Telerik.Models.Dal
{
    public class CandidatoDal
    {
        public static int Insertar(string nombre, string aPaterno, string puestoDeseado)
        {
            string query = @"
                INSERT INTO Candidatos (nombre, aPaterno, puestoDeseado, fechaRegistro)
                VALUES (@nombre, @aPaterno, @puestoDeseado, GETDATE());
                SELECT SCOPE_IDENTITY();";

            var parametros = new[]
            {
                new SqlParameter("@nombre", (object)nombre ?? DBNull.Value),
                new SqlParameter("@aPaterno", (object)aPaterno ?? DBNull.Value),
                new SqlParameter("@puestoDeseado", (object)puestoDeseado ?? DBNull.Value)
            };

            object resultado = ConexionBd.EjecutarEscalar(query, parametros);
            return Convert.ToInt32(resultado);
        }
    }
}

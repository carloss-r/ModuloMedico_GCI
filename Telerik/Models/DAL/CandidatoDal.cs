using System;
using System.Data.SqlClient;

namespace Telerik.Models.DAL
{
    public class CandidatoDal
    {
        public static int Insertar(string nombre, string aPaterno, string aMaterno = null, string puestoDeseado = null, string area = null, string empresa = null, string sexo = "", int? fkEmpresa = null, int? fkProyecto = null)
        {
            string sql = @"
                INSERT INTO Candidatos (nombre, aPaterno, aMaterno, puestoDeseado, area, empresa, fkSexo, fechaRegistro)
                VALUES (@nom, @pat, @mat, @pue, @are, @emp, @sex, GETDATE());
                SELECT SCOPE_IDENTITY();";

            return Convert.ToInt32(SqlHelper.ExecuteScalar(sql,
                new SqlParameter("@nom", nombre),
                new SqlParameter("@pat", aPaterno),
                new SqlParameter("@mat", (object)aMaterno ?? DBNull.Value),
                new SqlParameter("@pue", (object)puestoDeseado ?? DBNull.Value),
                new SqlParameter("@are", (object)area ?? DBNull.Value),
                new SqlParameter("@emp", (object)empresa ?? DBNull.Value),
                new SqlParameter("@sex", !string.IsNullOrEmpty(sexo) ? (object)sexo : DBNull.Value)
            ));
        }
    }
}

using System;
using System.Data;
using System.Data.SqlClient;
using Telerik.Models.ViewModels;

namespace Telerik.Models.DAL
{
    public class EmpleadoDal
    {
        public static EmpleadoVm BuscarPorNumero(int pkEmpleado)
        {
            // Empleado -> Empleados
            // Empresa -> Empresas
            // Proyecto -> Proyectos
            // Puesto -> Puesto (Singular)
            // Cat_Pais -> Pais (Sin prefijo)
            string sql = @"
                SELECT e.*, 
                       p.descripcion as PuestoDesc, 
                       emp.nombre as EmpresaDesc, 
                       pr.descripcion as ProyectoDesc,
                       pais.descripcion as PaisDesc,
                       edo.descripcion as EstadoDesc,
                       mun.descripcion as MunicipioDesc,
                       col.descripcion as ColoniaDesc,
                       cp.descripcion as CPDesc,
                       ec.descripcion as EstadoCivilDesc,
                       ts.descripcion as TipoSangreDesc
                FROM Empleados e
                LEFT JOIN Puesto p ON e.fkPuesto = p.pkPuesto
                LEFT JOIN Empresas emp ON e.fkEmpresa = emp.pkEmpresa
                LEFT JOIN Proyectos pr ON e.fkProyecto = pr.pkProyecto
                LEFT JOIN Pais pais ON e.fkPais = pais.pkPais
                LEFT JOIN Estado edo ON e.fkEstado = edo.pkEstado
                LEFT JOIN Municipio mun ON e.fkMunicipio = mun.pkMunicipio
                LEFT JOIN Colonia col ON e.fkColonia = col.pkColonia
                LEFT JOIN CP cp ON e.fkCP = cp.pkCP
                LEFT JOIN EstadoCivil ec ON e.fkEstadoCivil = ec.pkEstadoCivil
                LEFT JOIN TipoSangre ts ON e.fkTipoSangre = ts.pkTipoSangre
                WHERE e.pkEmpleado = @id";

            DataTable dt = SqlHelper.ExecuteDataTable(sql, new SqlParameter("@id", pkEmpleado));
            if (dt.Rows.Count == 0) return null;

            DataRow r = dt.Rows[0];
            return new EmpleadoVm
            {
                PkEmpleado      = (int)r["pkEmpleado"],
                Nombre          = r["nombre"]?.ToString(),
                APaterno        = r["aPaterno"]?.ToString(),
                AMaterno        = r["aMaterno"]?.ToString(),
                Nss             = r["numeroSeguroSocial"]?.ToString(),
                Rfc             = r["rfc"]?.ToString(),
                Curp            = r["curp"]?.ToString(),
                Telefono        = r["telefono"]?.ToString(),
                FechaNacimiento = r["fechaNacimiento"] != DBNull.Value ? (DateTime?)r["fechaNacimiento"] : null,
                Sexo            = r["fkSexo"]?.ToString(),
                FkPuesto        = r["fkPuesto"] != DBNull.Value ? (int?)r["fkPuesto"] : null,
                FkEmpresa       = r["fkEmpresa"] != DBNull.Value ? (int?)r["fkEmpresa"] : null,
                FkProyecto      = r["fkProyecto"] != DBNull.Value ? (int?)r["fkProyecto"] : null,
                PuestoDesc      = r["PuestoDesc"]?.ToString(),
                EmpresaDesc     = r["EmpresaDesc"]?.ToString(),
                ProyectoDesc    = r["ProyectoDesc"]?.ToString(),
                FkPais          = r["fkPais"] != DBNull.Value ? (int?)r["fkPais"] : null,
                FkEstado        = r["fkEstado"] != DBNull.Value ? (int?)r["fkEstado"] : null,
                FkMunicipio     = r["fkMunicipio"] != DBNull.Value ? (int?)r["fkMunicipio"] : null,
                FkColonia       = r["fkColonia"] != DBNull.Value ? (int?)r["fkColonia"] : null,
                FkCP            = r["fkCP"] != DBNull.Value ? (int?)r["fkCP"] : null,
                Calle           = r["calle"]?.ToString(),
                NumExterior     = r["numExterior"]?.ToString(),
                NumInterior     = r["numInterior"]?.ToString(),
                PaisDesc        = r["PaisDesc"]?.ToString(),
                EstadoDesc      = r["EstadoDesc"]?.ToString(),
                MunicipioDesc   = r["MunicipioDesc"]?.ToString(),
                ColoniaDesc     = r["ColoniaDesc"]?.ToString(),
                CPDesc          = r["CPDesc"]?.ToString(),
                EstadoCivil     = r["EstadoCivilDesc"]?.ToString(),
                TipoSangre      = r["TipoSangreDesc"]?.ToString(),
                FkTipoSangre    = r["fkTipoSangre"] != DBNull.Value ? (int?)r["fkTipoSangre"] : null,
                TieneHijos      = r["tieneHijos"] != DBNull.Value && (bool)r["tieneHijos"]
            };
        }
    }
}

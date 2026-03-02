using System;
using System.Data;
using System.Data.SqlClient;
using Telerik.Models.ViewModels;

namespace Telerik.Models.Dal
{
    public class EmpleadoDal
    {
        public static EmpleadoVm BuscarPorNumero(int pkEmpleado)
        {
            string query = @"
                SELECT 
                    e.pkEmpleado,
                    e.nombre,
                    e.aPaterno,
                    e.aMaterno,
                    e.rfc,
                    e.curp,
                    e.numeroSeguroSocial,
                    e.edad,
                    e.fechaNacimiento,
                    e.fkSexo,
                    p.descripcion AS puestoDesc,
                    a.descripcion AS areaDesc,
                    pr.descripcion AS proyectoDesc,
                    emp.nombre AS empresaDesc,
                    e.telefono,
                    e.calle,
                    e.numExterior,
                    e.numInterior,
                    ec.descripcion AS estadoCivilDesc,
                    ts.descripcion AS tipoSangreDesc,
                    col.descripcion AS coloniaDesc,
                    m.descripcion AS municipioDesc,
                    est.descripcion AS estadoDesc,
                    pais.descripcion AS paisDesc,
                    cp.descripcion AS cpDesc,
                    ISNULL(e.tieneHijos, 0) AS tieneHijos,
                    nh.descripcion AS numeroHijosDesc,
                    ne.descripcion AS escolaridadDesc
                FROM Empleados e
                LEFT JOIN Puesto p ON e.fkPuesto = p.pkPuesto
                LEFT JOIN Areas a ON p.fkArea = a.pkArea
                LEFT JOIN Proyectos pr ON e.fkProyecto = pr.pkProyecto
                LEFT JOIN Empresas emp ON pr.fkEmpresa = emp.pkEmpresa
                LEFT JOIN EstadoCivil ec ON e.fkEstadoCivil = ec.pkEstadoCivil
                LEFT JOIN TipoSangre ts ON e.fkTipoSangre = ts.pkTipoSangre
                LEFT JOIN Colonia col ON e.fkColonia = col.pkColonia
                LEFT JOIN Municipio m ON e.fkMunicipio = m.pkMunicipio
                LEFT JOIN Estado est ON e.fkEstado = est.pkEstado
                LEFT JOIN Pais pais ON e.fkPais = pais.pkPais
                LEFT JOIN CP cp ON e.fkCP = cp.pkCP
                LEFT JOIN NumeroHijos nh ON e.fkNumeroHijos = nh.pkNumeroHijos
                LEFT JOIN DatosEscolares de ON e.pkEmpleado = de.fkEmpleado
                LEFT JOIN NivelEscolaridad ne ON de.fkNivelEscolaridad = ne.pkNivelEscolaridad
                WHERE e.pkEmpleado = @pk";

            DataTable dt = ConexionBd.EjecutarConsulta(query, new SqlParameter("@pk", pkEmpleado));

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new EmpleadoVm
                {
                    PkEmpleado = Convert.ToInt32(row["pkEmpleado"]),
                    Nombre = row["nombre"] != DBNull.Value ? row["nombre"].ToString().Trim() : "",
                    APaterno = row["aPaterno"] != DBNull.Value ? row["aPaterno"].ToString().Trim() : "",
                    AMaterno = row["aMaterno"] != DBNull.Value ? row["aMaterno"].ToString().Trim() : "",
                    Rfc = row["rfc"] != DBNull.Value ? row["rfc"].ToString().Trim() : "",
                    Curp = row["curp"] != DBNull.Value ? row["curp"].ToString().Trim() : "",
                    Nss = row["numeroSeguroSocial"] != DBNull.Value ? row["numeroSeguroSocial"].ToString().Trim() : "",
                    Edad = row["edad"] != DBNull.Value ? row["edad"].ToString().Trim() : "",
                    FechaNacimiento = row["fechaNacimiento"] != DBNull.Value ? Convert.ToDateTime(row["fechaNacimiento"]) : (DateTime?)null,
                    Sexo = row["fkSexo"] != DBNull.Value ? row["fkSexo"].ToString().Trim() : "",
                    PuestoDesc = row["puestoDesc"] != DBNull.Value ? row["puestoDesc"].ToString().Trim() : "",
                    AreaDesc = row["areaDesc"] != DBNull.Value ? row["areaDesc"].ToString().Trim() : "",
                    ProyectoDesc = row["proyectoDesc"] != DBNull.Value ? row["proyectoDesc"].ToString().Trim() : "",
                    EmpresaDesc = row["empresaDesc"] != DBNull.Value ? row["empresaDesc"].ToString().Trim() : "",
                    
                    Telefono = row["telefono"] != DBNull.Value ? row["telefono"].ToString().Trim() : "",
                    Calle = row["calle"] != DBNull.Value ? row["calle"].ToString().Trim() : "",
                    NumExterior = row["numExterior"] != DBNull.Value ? row["numExterior"].ToString().Trim() : "",
                    NumInterior = row["numInterior"] != DBNull.Value ? row["numInterior"].ToString().Trim() : "",
                    EstadoCivil = row["estadoCivilDesc"] != DBNull.Value ? row["estadoCivilDesc"].ToString().Trim() : "",
                    TipoSangre = row["tipoSangreDesc"] != DBNull.Value ? row["tipoSangreDesc"].ToString().Trim() : "",
                    ColoniaDesc = row["coloniaDesc"] != DBNull.Value ? row["coloniaDesc"].ToString().Trim() : "",
                    MunicipioDesc = row["municipioDesc"] != DBNull.Value ? row["municipioDesc"].ToString().Trim() : "",
                    EstadoDesc = row["estadoDesc"] != DBNull.Value ? row["estadoDesc"].ToString().Trim() : "",
                    PaisDesc = row["paisDesc"] != DBNull.Value ? row["paisDesc"].ToString().Trim() : "",
                    CPDesc = row["cpDesc"] != DBNull.Value ? row["cpDesc"].ToString().Trim() : "",
                    TieneHijos = row["tieneHijos"] != DBNull.Value ? Convert.ToBoolean(row["tieneHijos"]) : false,
                    NumeroHijosDesc = row["numeroHijosDesc"] != DBNull.Value ? row["numeroHijosDesc"].ToString().Trim() : "",
                    EscolaridadDesc = row["escolaridadDesc"] != DBNull.Value ? row["escolaridadDesc"].ToString().Trim() : ""
                };
            }

            return null;
        }
    }
}

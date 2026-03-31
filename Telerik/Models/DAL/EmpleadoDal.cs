using System.Linq;
using Telerik.Models;
using Telerik.Models.ViewModels;

namespace Telerik.Models.DAL
{
    public class EmpleadoDal
    {
        public static EmpleadoVm BuscarPorNumero(int pkEmpleado)
        {
            using (var db = new ApplicationDbContext())
            {
                return (from e in db.Empleados
                        join p in db.Puestos on e.fkPuesto equals p.pkPuesto into pjoin
                        from p in pjoin.DefaultIfEmpty()
                        join emp in db.Empresas on e.fkEmpresa equals emp.pkEmpresa into ejoin
                        from emp in ejoin.DefaultIfEmpty()
                        join pr in db.Proyectos on e.fkProyecto equals pr.pkProyecto into prjoin
                        from pr in prjoin.DefaultIfEmpty()
                        join pais in db.Paises on e.fkPais equals pais.pkPais into paisjoin
                        from pais in paisjoin.DefaultIfEmpty()
                        join estado in db.Estados on e.fkEstado equals estado.pkEstado into estjoin
                        from estado in estjoin.DefaultIfEmpty()
                        join mun in db.Municipios on e.fkMunicipio equals mun.pkMunicipio into munjoin
                        from mun in munjoin.DefaultIfEmpty()
                        join col in db.Colonias on e.fkColonia equals col.pkColonia into coljoin
                        from col in coljoin.DefaultIfEmpty()
                        join cp in db.CodigosPostales on e.fkCP equals cp.pkCP into cpjoin
                        from cp in cpjoin.DefaultIfEmpty()
                        where e.pkEmpleado == pkEmpleado
                        select new EmpleadoVm
                        {
                            PkEmpleado      = e.pkEmpleado,
                            Nombre          = e.nombre,
                            APaterno        = e.aPaterno,
                            AMaterno        = e.aMaterno,
                            Nss             = e.numeroSeguroSocial,
                            Rfc             = e.rfc,
                            Curp            = e.curp,
                            Telefono        = e.telefono,
                            FechaNacimiento = e.fechaNacimiento,
                            Sexo            = e.fkSexo,
                            PuestoDesc      = p != null ? p.descripcion : null,
                            EmpresaDesc     = emp != null ? emp.nombre : null,
                            ProyectoDesc    = pr != null ? pr.descripcion : null,
                            // Geographic FKs
                            FkPais          = e.fkPais,
                            FkEstado        = e.fkEstado,
                            FkMunicipio     = e.fkMunicipio,
                            FkColonia       = e.fkColonia,
                            FkCP            = e.fkCP,
                            Calle           = e.calle,
                            NumExterior     = e.numExterior,
                            NumInterior     = e.numInterior,
                            // Geographic Descriptions
                            PaisDesc        = pais != null ? pais.descripcion : null,
                            EstadoDesc      = estado != null ? estado.descripcion : null,
                            MunicipioDesc   = mun != null ? mun.descripcion : null,
                            ColoniaDesc     = col != null ? col.descripcion : null,
                            CPDesc          = cp != null ? cp.descripcion : null,
                            // Otros
                            TieneHijos      = e.tieneHijos ?? false
                        }).FirstOrDefault();
            }
        }
    }
}

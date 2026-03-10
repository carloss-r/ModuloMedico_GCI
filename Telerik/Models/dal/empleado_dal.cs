using System.Linq;
using Telerik.Models;
using Telerik.Models.ViewModels;

namespace Telerik.Models.Dal
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
                            ProyectoDesc    = pr != null ? pr.descripcion : null
                        }).FirstOrDefault();
            }
        }
    }
}

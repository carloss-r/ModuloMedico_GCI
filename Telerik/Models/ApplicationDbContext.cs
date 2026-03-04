using System.Data.Entity;

namespace Telerik.Models
{
    public class ApplicationDbContext : DbContext
    {
        static ApplicationDbContext()
        {
            // CRÍTICO: Evita que EF Code First intente crear/validar el esquema.
            // La BD ya existe y está administrada externamente (DATA_GCI.sql).
            Database.SetInitializer<ApplicationDbContext>(null);
        }

        public ApplicationDbContext() : base("GCI_ModuloMedico") 
        { 
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
        }

        // Entidades existentes
        public DbSet<Empleado>              Empleados               { get; set; }
        public DbSet<PruebaToxicologica>    PruebasToxicologicas    { get; set; }

        // Entidades de Solicitudes
        public DbSet<OrdenServicioMedico>   OrdenesMedicas          { get; set; }
        public DbSet<Candidato>             Candidatos              { get; set; }

        // Catálogos
        public DbSet<TipoServicio>          TiposServicio           { get; set; }
        public DbSet<EstatusSolicitud>      EstatusSolicitudes      { get; set; }
        public DbSet<Proyecto>              Proyectos               { get; set; }
        public DbSet<Empresa>               Empresas                { get; set; }
        public DbSet<Puesto>                Puestos                 { get; set; }
        public DbSet<Area>                  Areas                   { get; set; }

        // Evaluación clínica y subtablas
        public DbSet<EvaluacionClinica>     EvaluacionesClinicas    { get; set; }
        public DbSet<HabitoPersonal>        HabitosPersonales       { get; set; }
        public DbSet<HistoriaMedica>        HistoriasMedicas        { get; set; }
        public DbSet<AntecedenteLaboral>    AntecedentesLaborales   { get; set; }
        public DbSet<ExamenFisico>          ExamenesFisicos         { get; set; }
        public DbSet<EvaluacionColumna>     EvaluacionesColumna     { get; set; }
        public DbSet<DetalleGineco>         DetallesGineco          { get; set; }
        public DbSet<DetalleMasculino>      DetallesMasculino       { get; set; }
        // NOTA: NO existe DbSet<Vacunacion> — las vacunas están en HabitosPersonales directamente.

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapeo manual para relaciones donde EF no puede inferir el lado principal de forma automática.
            // EvaluacionClinica es el Principal (1), los detalles son los Dependientes (*).
            
            modelBuilder.Entity<EvaluacionColumna>()
                .HasRequired(c => c.Evaluacion)
                .WithMany() // Dejamos WithMany sin propiedad de navegación de regreso para forzar 1-a-Muchos en BD en lugar de 1-a-1 estricto que requiere PK compartida en EF
                .HasForeignKey(c => c.fkEvaluacion);

            modelBuilder.Entity<HabitoPersonal>()
                .HasRequired(h => h.Evaluacion)
                .WithMany()
                .HasForeignKey(h => h.fkEvaluacion);

            modelBuilder.Entity<DetalleGineco>()
                .HasRequired(d => d.Evaluacion)
                .WithMany()
                .HasForeignKey(d => d.fkEvaluacion);

            modelBuilder.Entity<DetalleMasculino>()
                .HasRequired(d => d.Evaluacion)
                .WithMany()
                .HasForeignKey(d => d.fkEvaluacion);

            // IGNORAR las propiedades de navegación "1-a-1" en el modelo EvaluacionClinica 
            // para que no intenten generar foreing keys inesperadas, 
            // ya que están mapeadas como colecciones anónimas arriba con 'WithMany()'.
            modelBuilder.Entity<EvaluacionClinica>().Ignore(e => e.Columna);
            modelBuilder.Entity<EvaluacionClinica>().Ignore(e => e.Habitos);
            modelBuilder.Entity<EvaluacionClinica>().Ignore(e => e.DetalleGineco);
            modelBuilder.Entity<EvaluacionClinica>().Ignore(e => e.DetalleMasculino);
        }
    }
}

namespace Telerik.Models.ViewModels
{
    /// <summary>
    /// Clase genérica para catálogos simples (Id + Descripción).
    /// Usada por CatalogoDal para devolver listas de dropdowns.
    /// </summary>
    public class CatalogoItem
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
    }
}

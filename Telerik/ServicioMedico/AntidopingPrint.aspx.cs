using System;
using System.Web.UI;
using Telerik.Models.DAL;           // Capa de acceso a datos para obtener información de la BD
using Telerik.Models.ViewModels;   // Modelos para transferencia de datos (como AntidopingVm)

namespace Telerik.ServicioMedico
{
    // Página especializada para generar el formato de impresión del examen de antidoping
    public partial class AntidopingPrint : System.Web.UI.Page
    {
        // Evento que se ejecuta al cargar la página de impresión
        protected void Page_Load(object sender, EventArgs e)
        {
            // Solo ejecutar en la primera carga (no en postbacks)
            if (!IsPostBack)
            {
                // Obtener el ID de la orden desde los parámetros de la URL (ej: AntidopingPrint.aspx?id=123)
                string idOrdenParam = Request.QueryString["id"];
                if (!string.IsNullOrEmpty(idOrdenParam) && int.TryParse(idOrdenParam, out int idOrden))
                {
                    // Si se proporcionó un ID válido, cargar los datos del antidoping
                    CargarDatosAntidoping(idOrden);
                }
            }
        }

        // Método para cargar y mostrar todos los datos del examen de antidoping en el formato
        private void CargarDatosAntidoping(int idOrden)
        {
            try
            {
                // Obtener datos principales de la orden de servicio médico
                var orden = OrdenServicioMedicoDal.ObtenerPorId(idOrden);
                if (orden != null)
                {
                    // Generar script JavaScript para llenar los campos del formato con los datos del paciente
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "cargarDatos", $@"
                        // Llenar campos del encabezado del formato
                        document.getElementById('proyecto').innerHTML = '{orden.ProyectoDesc ?? ''}';
                        document.getElementById('empresa').innerHTML = '{orden.EmpresaNombre ?? orden.EmpresaCandidato ?? ''}';
                        document.getElementById('fecha').innerHTML = '{DateTime.Now:dd/MM/yyyy}';
                        document.getElementById('nombreTrabajador').innerHTML = '{orden.NombrePersona ?? ''}';
                        document.getElementById('puesto').innerHTML = '{orden.PuestoCandidato ?? ''}';
                    ", true);
                }

                // NOTA: Aquí se pueden cargar los datos específicos del antidoping
                // (resultados de drogas, veredicto, evidencia fotográfica, etc.)
                // desde la base de datos cuando se implemente la tabla correspondiente
            }
            catch (Exception ex)
            {
                // Si ocurre algún error al cargar los datos, mostrar alerta al usuario
                Page.ClientScript.RegisterStartupScript(this.GetType(), "error", $@"
                    alert('Error al cargar los datos: {ex.Message}');
                ", true);
            }
        }

        // Método auxiliar para marcar checkboxes en el formato desde el servidor
        protected void MarcarCheckbox(string controlId, bool marcado)
        {
            // Generar script JavaScript para llamar a la función marcarCheckbox() del frontend
            string script = $"marcarCheckbox('{controlId}', {marcado.ToString().ToLower()});";
            Page.ClientScript.RegisterStartupScript(this.GetType(), $"chk_{controlId}", script, true);
        }

        // Método principal para llenar todos los datos del examen de antidoping en el formato de impresión
        protected void LlenarDatosAntidoping(AntidopingVm datos)
        {
            if (datos != null)
            {
                // Marcar checkboxes de "APLICA" para cada sustancia evaluada
                MarcarCheckbox("chkAplicaCocaina", datos.AplicaCocaina);
                MarcarCheckbox("chkAplicaTHC", datos.AplicaTHC);
                MarcarCheckbox("chkAplicaAlcohol", datos.AplicaAlcohol);
                MarcarCheckbox("chkAplicaAnfetaminas", datos.AplicaAnfetaminas);
                MarcarCheckbox("chkAplicaMetanfetaminas", datos.AplicaMetanfetaminas);
                MarcarCheckbox("chkAplicaOpiaceos", datos.AplicaOpiaceos);
                MarcarCheckbox("chkAplicaBenzodiacepinas", datos.AplicaBenzodiacepinas);

                // Marcar checkboxes de "RESULTADO POSITIVO" para cada sustancia
                MarcarCheckbox("chkPositivoCocaina", datos.ResultadoCocaina);
                MarcarCheckbox("chkPositivoTHC", datos.ResultadoTHC);
                MarcarCheckbox("chkPositivoAlcohol", datos.ResultadoAlcohol);
                MarcarCheckbox("chkPositivoAnfetaminas", datos.ResultadoAnfetaminas);
                MarcarCheckbox("chkPositivoMetanfetaminas", datos.ResultadoMetanfetaminas);
                MarcarCheckbox("chkPositivoOpiaceos", datos.ResultadoOpiaceos);
                MarcarCheckbox("chkPositivoBenzodiacepinas", datos.ResultadoBenzodiacepinas);

                // Marcar checkbox de consentimiento informado del trabajador
                MarcarCheckbox("chkConsentimiento", datos.ConsentimientoFirmado);

                // Determinar y marcar el veredicto final (APTO/NO APTO)
                if (!string.IsNullOrEmpty(datos.VeredictoFinal))
                {
                    if (datos.VeredictoFinal.ToUpper().Contains("APTO") && !datos.VeredictoFinal.ToUpper().Contains("NO APTO"))
                    {
                        MarcarCheckbox("chkVeredictoApto", true);  // Marcar como APTO
                    }
                    else if (datos.VeredictoFinal.ToUpper().Contains("NO APTO"))
                    {
                        MarcarCheckbox("chkVeredictoNoApto", true);  // Marcar como NO APTO
                    }
                }

                // Llenar campo de observaciones/comentarios adicionales
                if (!string.IsNullOrEmpty(datos.Comentarios))
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "observaciones", $@"
                        document.getElementById('observaciones').innerHTML = '{datos.Comentarios.Replace("\r\n", "<br/>")}';
                    ", true);
                }

                // Cargar foto si existe
                if (!string.IsNullOrEmpty(datos.UrlFotoEvidencia))
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "foto", $@"
                        document.getElementById('fotoContainer').innerHTML = '<img src=""{datos.UrlFotoEvidencia}"" style=""max-width: 100%; max-height: 100%;"" />';
                    ", true);
                }
            }
        }
    }
}

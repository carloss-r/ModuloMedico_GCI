using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using Telerik.Models.DAL;
using Telerik.Models.ViewModels;
using Telerik.Services;

namespace Telerik.ServicioMedico
{
    public partial class ImpresionFormatos : System.Web.UI.Page
    {
        public int    IdOrden      { get; set; }
        public string TipoDoc      { get; set; }
        public PacienteInfoVm     Paciente   { get; set; }
        public EvaluacionMedicaVm Evaluacion { get; set; }
        public string PaseHtml     { get; set; }
        public string ErrorMessage { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(Request.QueryString["id"], out int id))
                    throw new Exception("ID inv&aacute;lido.");

                IdOrden = id;
                TipoDoc = (Request.QueryString["tipo"] ?? "").ToUpper();

                var ms    = new MedicalService();
                var orden = OrdenServicioMedicoDal.ObtenerPorId(id);
                if (orden == null)
                    throw new Exception("Solicitud #" + id + " no encontrada.");

                Paciente   = ms.ObtenerInfoPaciente(orden);
                Evaluacion = EvaluacionDal.ObtenerPorOrden(id);

                if      (TipoDoc == "PASE")       GenerarPaseHtml();
                else if (TipoDoc == "EXAMEN")     GenerarExamenDesdeHtml();
                else if (TipoDoc == "ANTIDOPING") GenerarAntidopingDesdeHtml();
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
        }

        // ── PASE DE SERVICIO MÉDICO — lee el HTML y sustituye tokens ──
        private void GenerarPaseHtml()
        {
            // Ruta al template HTML
            string templatePath = Server.MapPath("~/ServicioMedico/Formatos/PaseMedico.html");
            if (!File.Exists(templatePath))
                throw new Exception("Template de pase no encontrado: " + templatePath);

            string html = File.ReadAllText(templatePath, Encoding.UTF8);

            string H(string s) => HttpUtility.HtmlEncode(s ?? "");

            string emp = !string.IsNullOrEmpty(Paciente?.Empresa) ? Paciente.Empresa : (Paciente?.Proyecto ?? "");
            int apt = Evaluacion?.FkAptitudMedica ?? 0;

            var rep = new Dictionary<string, string>
            {
                { "{{EMPRESA}}",         H(emp.ToUpper()) },
                { "{{FECHA}}",           DateTime.Now.ToString("dd/MM/yyyy") },
                { "{{PROYECTO}}",        H((Paciente?.Proyecto ?? "").ToUpper()) },
                { "{{NOMBRE}}",          H((Paciente?.NombreCompleto ?? "").ToUpper()) },
                { "{{PUESTO}}",          H((Paciente?.Puesto ?? "").ToUpper()) },
                { "{{RES_APTO}}",        apt == 1 ? "&#10004;" : "" },
                { "{{RES_CONDICIONADO}}",apt == 2 ? "&#10004;" : "" },
                { "{{RES_NO_APTO}}",     apt == 3 ? "&#10004;" : "" },
                { "{{RECOMENDACIONES}}", H(Evaluacion?.Recomendaciones) },
            };

            foreach (var kv in rep)
                html = html.Replace(kv.Key, kv.Value);

            PaseHtml = html;
        }

        // ── EXAMEN MÉDICO — lee el HTML y sustituye tokens ─────────────
        private void GenerarExamenDesdeHtml()
        {
            // Ruta al template HTML
            string templatePath = Server.MapPath("~/ServicioMedico/Formatos/EvaluacionMedica.html");
            if (!File.Exists(templatePath))
                throw new Exception("Template no encontrado: " + templatePath);

            string html = File.ReadAllText(templatePath, Encoding.UTF8);

            // ── Helpers ──────────────────────────────────────────────
            string H(string s)  => HttpUtility.HtmlEncode(s ?? "");
            string Chk(bool v)  => v ? "X" : "";
            string CV(int? v)   => v == 1 ? "N" : v == 2 ? "A" : v == 3 ? "D" : "";

            bool esM  = ((Paciente?.Sexo ?? "").ToUpper().StartsWith("M") || (Paciente?.Sexo ?? "").ToUpper().Contains("MASC"));
            bool esF  = ((Paciente?.Sexo ?? "").ToUpper().StartsWith("F") || (Paciente?.Sexo ?? "").ToUpper().Contains("FEM"));
            bool esC  = ((Paciente?.Tipo ?? "").ToUpper().Contains("CAND"));

            string lugar = string.IsNullOrWhiteSpace(Evaluacion?.LugarEvaluacion) ? "Tula de Allende" : Evaluacion.LugarEvaluacion;

            // Antecedente positivo → "X"
            string Ant(string kw)
            {
                var a = Evaluacion?.Antecedentes?.FirstOrDefault(x =>
                    (x.NombreCondicion ?? "").ToUpperInvariant().Contains(kw.ToUpperInvariant()));
                return (a != null && a.EsPositivo) ? "X" : "";
            }

            // Tipo de sangre
            string ts;
            switch (Evaluacion?.FkTipoSangre)
            { case 1: ts="O+"; break; case 2: ts="O-"; break; case 3: ts="A+"; break; case 4: ts="A-"; break;
              case 5: ts="B+"; break; case 6: ts="B-"; break; case 7: ts="AB+"; break; case 8: ts="AB-"; break;
              default: ts = ""; break; }

            // Estado civil
            string ec = (Evaluacion?.EstadoCivil ?? "").ToUpper();

            // Escolaridad
            string ew = (Evaluacion?.Escolaridad ?? "").ToUpper();

            // Domicilio compuesto
            string dom = Evaluacion?.Domicilio ?? "";
            if (string.IsNullOrWhiteSpace(dom))
            {
                var pp = new List<string>();
                if (!string.IsNullOrWhiteSpace(Evaluacion?.Calle))       pp.Add(Evaluacion.Calle);
                if (!string.IsNullOrWhiteSpace(Evaluacion?.NumExterior)) pp.Add("#" + Evaluacion.NumExterior);
                if (!string.IsNullOrWhiteSpace(Evaluacion?.NumInterior)) pp.Add("Int. " + Evaluacion.NumInterior);
                dom = string.Join(" ", pp);
            }

            var col = Evaluacion?.Columna;
            var hb  = Evaluacion?.Habitos;
            var vac = Evaluacion?.Vacunacion;
            var gf  = Evaluacion?.DetalleFemenino;
            var gm  = Evaluacion?.DetalleMasculino;
            int apt = Evaluacion?.FkAptitudMedica ?? 0;

            // ── Antecedentes laborales: generar filas HTML ────────────
            var laborales = new StringBuilder();
            if (Evaluacion?.AntecedentesLaborales != null && Evaluacion.AntecedentesLaborales.Any())
            {
                foreach (var al in Evaluacion.AntecedentesLaborales)
                    laborales.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>\n",
                        H(al?.Empresa), H(al?.TiempoLaborado), H(al?.Puesto), H(al?.AgentesExpuesto), H(al?.AccidentesPrevios));
            }
            else
            {
                laborales.Append("<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>\n");
                laborales.Append("<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>\n");
            }

            // ── Examen físico: 20 sistemas (en orden fijo) ────────────
            var sistNombres = new[] {
                "Cabeza","Ojos","Nariz","Boca","Dentadura","Faringe",
                "Amigdalas","Otoscopia","Cuello","Columna-espalda","Extremidades",
                "Piel","Ap. Respiratorio","Cardiaco","Vascular periferico",
                "Abdomen","Neurologico","Genitales","Hernias","Otro"
            };

            // ── Reemplazos ────────────────────────────────────────────
            var rep = new Dictionary<string, string>
            {
                // Identificación
                {"{{LUGAR_FECHA}}",   H(lugar + ", " + DateTime.Now.ToString("dd/MM/yyyy"))},
                {"{{CARGO}}",         H(Paciente?.Puesto)},
                {"{{NOMBRE}}",        H(Paciente?.NombreCompleto)},
                {"{{NSS}}",           H(Evaluacion?.Nss)},
                {"{{FECHA_NAC}}",     H(Evaluacion?.FechaNacimiento?.ToString("dd/MM/yyyy"))},
                {"{{EDAD}}",          H(Paciente?.Edad)},
                {"{{LUGAR_NAC}}",     H(Evaluacion?.LugarNacimiento)},
                // Estado civil
                {"{{EC_SOLTERO}}",    ec.Contains("SOLTER") || ec == "1" ? "X" : ""},
                {"{{EC_CASADO}}",     ec.Contains("CASAD")  || ec == "2" ? "X" : ""},
                {"{{EC_UNION}}",      ec.Contains("UNION")  || ec.Contains("UNI\u00d3N") || ec == "3" ? "X" : ""},
                {"{{EC_SEPARADO}}",   ec.Contains("SEPAR")  || ec.Contains("DIVOR") || ec.Contains("VIUD") || ec == "4" ? "X" : ""},
                {"{{MANO}}",          H(Evaluacion?.ManoDominante)},
                {"{{TELEFONO}}",      H(Evaluacion?.Telefono)},
                {"{{DOMICILIO}}",     H(dom)},
                // Escolaridad
                {"{{ESC_PRIM}}",      ew.Contains("PRIM") ? "X" : ""},
                {"{{ESC_SEC}}",       ew.Contains("SECUN") ? "X" : ""},
                {"{{ESC_MED}}",       ew.Contains("MEDIA") || ew.Contains("PREPA") || ew.Contains("BACH") ? "X" : ""},
                {"{{ESC_UNI}}",       ew.Contains("UNIV")  || ew.Contains("LIC") || ew.Contains("PROF") || ew.Contains("POSG") ? "X" : ""},
                {"{{PROFESION}}",     H(Evaluacion?.Profesion)},
                // Tipo de examen / sexo / sangre
                {"{{EX_INGRESO}}",    esC ? "X" : ""},
                {"{{EX_PERIODICO}}",  !esC ? "X" : ""},
                {"{{SEXO_MASC_BG}}",  esM ? "background:#333;color:#fff;" : "background:#eee;"},
                {"{{SEXO_FEM_BG}}",   esF ? "background:#333;color:#fff;" : "background:#eee;"},
                {"{{TIPO_SANGRE}}",   ts},
                // Antecedentes heredo-familiares
                {"{{AHF_HTA}}",       Ant("HTA")},
                {"{{AHF_CORONARIA}}", Ant("CORONARIA")},
                {"{{AHF_ACV}}",       Ant("ACV")},
                {"{{AHF_DIABETES}}",  Ant("DIABETES")},
                {"{{AHF_TIROIDES}}",  Ant("TIROIDES")},
                {"{{AHF_ASMA}}",      Ant("ASMA")},
                {"{{AHF_ALERGIA}}",   Ant("ALERGIA")},
                {"{{AHF_TBC}}",       Ant("TBC")},
                {"{{AHF_ALCOHOL}}",   Ant("ALCOHOL")},
                {"{{AHF_EPILEPSIA}}", Ant("EPILEPSIA")},
                {"{{AHF_MENTALES}}",  Ant("MENTALES")},
                {"{{AHF_CONGENITAS}}",Ant("CONGENITA")},
                {"{{AHF_CANCER}}",    Ant("CANCER")},
                {"{{AHF_VARICES}}",   Ant("VARICES")},
                // Antecedentes personales patológicos
                {"{{APP_HIPERTENSION}}",    Ant("HIPERTENSION")},
                {"{{APP_QUIRURGICOS}}",     Ant("QUIRURGICO")},
                {"{{APP_TRAUMATICOS}}",     Ant("TRAUMATICO")},
                {"{{APP_ALERGICOS}}",       Ant("ALERGICO")},
                {"{{APP_CONGENITOS}}",      Ant("CONGENITO")},
                {"{{APP_METABOLICOS}}",     Ant("METABOLICO")},
                {"{{APP_INFECCIOSOS}}",     Ant("INFECCIOSO")},
                {"{{APP_TUMORALES}}",       Ant("TUMORAL")},
                {"{{APP_RESPIRATORIAS}}",   Ant("RESPIRATORIA")},
                {"{{APP_MEDICAMENTOS}}",    Ant("MEDICAMENTO")},
                {"{{APP_TRANSFUSIONALES}}", Ant("TRANSFUSION")},
                {"{{APP_LITIASIS}}",        Ant("LITIASIS")},
                {"{{APP_HACINAMIENTO}}",    Ant("HACINAMIENTO")},
                {"{{APP_AGUA}}",            Ant("AGUA")},
                {"{{APP_ALCANTARILLADO}}",  Ant("ALCANTARILLADO")},
                {"{{APP_OTROS}}",           Ant("OTRO")},
                {"{{APP_OBSERVACIONES}}",   H(Evaluacion?.SintomasPaciente)},
                // Antecedentes laborales
                {"{{LABORALES_ROWS}}", laborales.ToString()},
                // Hábitos
                {"{{HAB_FUMA}}",          Chk(hb != null && hb.Fuma)},
                {"{{HAB_ANOS_FUMA}}",     H(hb?.AnosFumando?.ToString())},
                {"{{HAB_CIGARROS}}",      H(hb?.CigarrosDiarios?.ToString())},
                {"{{HAB_EX_FUMADOR}}",    hb != null && hb.EsExFumador ? "Si" : ""},
                {"{{HAB_DROGAS}}",        Chk(hb != null && hb.UsaDrogas)},
                {"{{HAB_TIPO_DROGA}}",    H(hb?.TipoDrogas)},
                {"{{HAB_ALCOHOL}}",       Chk(hb != null && hb.BebeAlcohol)},
                {"{{HAB_FREC_ALCOHOL}}",  H(hb?.FrecuenciaAlcohol)},
                {"{{HAB_DEPORTE}}",       Chk(hb != null && hb.HaceDeporte)},
                {"{{HAB_TIPO_DEPORTE}}",  H(hb?.TipoDeporte)},
                {"{{HAB_TIEMPO_LIBRE}}",  H(hb?.DescripcionTiempoLibre)},
                // Vacunas
                {"{{VAC_T1}}",   Chk(vac != null && vac.TetanosDosis1)},
                {"{{VAC_T2}}",   Chk(vac != null && vac.TetanosDosis2)},
                {"{{VAC_T3}}",   Chk(vac != null && vac.TetanosDosis3)},
                {"{{VAC_H1}}",   Chk(vac != null && vac.HepatitisDosis1)},
                {"{{VAC_H2}}",   Chk(vac != null && vac.HepatitisDosis2)},
                {"{{VAC_H1N1}}", Chk(vac != null && vac.InfluenzaH1N1)},
                // Exploración física – signos vitales
                {"{{EF_TA}}",       H(Evaluacion?.PresionSistolica + "/" + Evaluacion?.PresionDiastolica)},
                {"{{EF_FC}}",       H(Evaluacion?.FrecuenciaCardiaca?.ToString())},
                {"{{EF_FR}}",       H(Evaluacion?.FrecuenciaRespiratoria?.ToString())},
                {"{{EF_PESO}}",     H(Evaluacion?.PesoKg?.ToString())},
                {"{{EF_TALLA}}",    H(Evaluacion?.AlturaMetros?.ToString())},
                {"{{EF_IMC}}",      H(Evaluacion?.Imc?.ToString())},
                {"{{EF_TEMP}}",     H(Evaluacion?.Temperatura?.ToString())},
                {"{{EF_APARATOS}}", H(Evaluacion?.AparatosSistemas)},
                {"{{EF_SINTOMAS}}", H(Evaluacion?.SintomasPaciente)},
                // Ginecológicos
                {"{{GF_MENARCA}}",      H(gf?.EdadMenarca?.ToString())},
                {"{{GF_CICLOS}}",       H(gf?.Ciclos)},
                {"{{GF_FUM}}",          H(gf?.FechaUltimaMenstruacion?.ToString("dd/MM/yyyy"))},
                {"{{GF_HIJOS}}",        H(gf?.NumeroHijosEdades)},
                {"{{GF_PLANIFICACION}}",H(gf?.MetodoPlanificacion)},
                {"{{GF_IVSA}}",         H(gf?.Ivsa?.ToString())},
                {"{{GF_CITVAG}}",       H(gf?.FechaUltimoPapanicolau?.ToString("dd/MM/yyyy"))},
                {"{{GF_ETS}}",          H(gf?.Ets)},
                {"{{GF_GESTAS}}",       H(gf?.Gestas?.ToString())},
                {"{{GF_PARTOS}}",       H(gf?.Partos?.ToString())},
                {"{{GF_ABORTOS}}",      H(gf?.Abortos?.ToString())},
                {"{{GF_CESAREAS}}",     H(gf?.Cesareas?.ToString())},
                // Genitourinario masculino
                {"{{GM_PREPUCIO}}",     Chk(gm != null && gm.PrepucioRetractil)},
                {"{{GM_TESTICULOS}}",   Chk(gm != null && gm.TesticulosDescendidos)},
                {"{{GM_FIMOSIS}}",      Chk(gm != null && gm.Fimosis)},
                {"{{GM_CRIPTORQUIDIA}}",Chk(gm != null && gm.Criptorquidia)},
                {"{{GM_VARICOCELE}}",   Chk(gm != null && gm.Varicocele)},
                {"{{GM_HIDROCELE}}",    Chk(gm != null && gm.Hidrocele)},
                {"{{GM_HERNIA}}",       Chk(gm != null && gm.Hernia)},
                {"{{GM_IVSA}}",         H(gm?.Ivsa)},
                {"{{GM_PSA}}",          H(gm?.Psa)},
                {"{{GM_MPF}}",          H(gm?.MetodoPlanificacion)},
                // Columna vertebral
                {"{{CV_LORD_CERV}}", CV(col?.LordosisCervical)},
                {"{{CV_LORD_DORS}}", CV(col?.LordosisDorsal)},
                {"{{CV_LORD_LUMB}}", CV(col?.LordosisLumbar)},
                {"{{CV_CIF_CERV}}",  CV(col?.CifosisCervical)},
                {"{{CV_CIF_DORS}}",  CV(col?.CifosisDorsal)},
                {"{{CV_CIF_LUMB}}",  CV(col?.CifosisLumbar)},
                // Escoliosis
                {"{{ESC_DORS_DER}}",  Chk(col != null && col.EscoliosisDorsalDerecha)},
                {"{{ESC_LUMB_DER}}",  Chk(col != null && col.EscoliosisLumbarDerecha)},
                {"{{ESC_DOBLE_DER}}", Chk(col != null && col.EscoliosisDobleDerecha)},
                {"{{ESC_DORS_IZQ}}",  Chk(col != null && col.EscoliosisDorsalIzquierda)},
                {"{{ESC_LUMB_IZQ}}",  Chk(col != null && col.EscoliosisLumbarIzquierda)},
                {"{{ESC_DOBLE_IZQ}}", Chk(col != null && col.EscoliosisDobleIzquierda)},
                // Diagnóstico / resultado
                {"{{DIAGNOSTICO}}",       H(Evaluacion?.Observaciones)},
                {"{{RES_APTO}}",          apt == 1 ? "X" : ""},
                {"{{RES_NO_APTO}}",       apt == 3 ? "X" : ""},
                {"{{RES_RESTRICCIONES}}", apt == 2 ? "X" : ""},
                {"{{RECOMENDACIONES}}",   H(Evaluacion?.Recomendaciones)},
            };

            // Aplicar todos los reemplazos
            foreach (var kv in rep)
                html = html.Replace(kv.Key, kv.Value);

            PaseHtml = html;
        }
        // ── ANTIDOPING — lee el HTML y sustituye tokens ──────────────────────
        private void GenerarAntidopingDesdeHtml()
        {
            string templatePath = Server.MapPath("~/ServicioMedico/Formatos/Antidoping.html");
            if (!File.Exists(templatePath))
                throw new Exception("Template de antidoping no encontrado: " + templatePath);

            string html = File.ReadAllText(templatePath, Encoding.UTF8);

            string H(string s)  => HttpUtility.HtmlEncode(s ?? "");
            string Chk(bool neg, bool pos, string tipo) =>
                tipo == "N" ? (neg && !pos ? "X" : "") : (pos ? "X" : "");

            var anti = AntidopingDal.ObtenerPorOrden(IdOrden);

            // Helpers de resultado: si no hay datos guardados se muestra vacío (niño)
            bool ResultNeg(bool aplica, bool positivo) => aplica && !positivo;
            bool ResultPos(bool aplica, bool positivo) => aplica && positivo;

            // Veredicto
            string veredictoHtml = "";
            if (anti != null && !string.IsNullOrWhiteSpace(anti.VeredictoFinal))
            {
                bool esNoApto = anti.VeredictoFinal.ToUpper().Contains("NO APTO");
                veredictoHtml = esNoApto
                    ? "<span style='border:2px solid #000; padding:4px 12px; font-size:11px; font-weight:bold;'>NO APTO PARA REALIZAR ACTIVIDADES OPERACIONALES</span>"
                    : "<span style='border:2px solid #000; padding:4px 12px; font-size:11px; font-weight:bold;'>APTO PARA REALIZAR ACTIVIDADES OPERACIONALES</span>";
            }

            // Foto de evidencia
            string fotoHtml = "";
            if (anti != null && !string.IsNullOrWhiteSpace(anti.UrlFotoEvidencia))
                fotoHtml = "<img src='" + anti.UrlFotoEvidencia + "' style='max-width:100%;max-height:100%;object-fit:contain;' />";

            var rep = new Dictionary<string, string>
            {
                { "{{FECHA}}",        DateTime.Now.ToString("dd/MM/yyyy") },
                { "{{PROYECTO}}",     H(Paciente?.Proyecto ?? "") },
                { "{{EMPRESA}}",      H(!string.IsNullOrEmpty(Paciente?.Empresa) ? Paciente.Empresa : (Paciente?.Proyecto ?? "")) },
                { "{{NOMBRE}}",       H(Paciente?.NombreCompleto ?? "") },
                { "{{NUM_TRABAJADOR}}",H(Paciente?.NumeroEmpleado ?? "") },
                { "{{FOTO_HTML}}",    fotoHtml },
                { "{{VEREDICTO_HTML}}",veredictoHtml },
                { "{{COMENTARIOS}}", H(anti?.Comentarios) },
                { "{{MEDICO}}",       "LIC. NATALY MARTINEZ PUGA" },
                // Opiáceos
                { "{{OPI_NEG}}", anti != null && ResultNeg(anti.AplicaOpiaceos, anti.ResultadoOpiaceos) ? "X" : "" },
                { "{{OPI_POS}}", anti != null && ResultPos(anti.AplicaOpiaceos, anti.ResultadoOpiaceos) ? "X" : "" },
                // Cocaína
                { "{{COC_NEG}}", anti != null && ResultNeg(anti.AplicaCocaina, anti.ResultadoCocaina) ? "X" : "" },
                { "{{COC_POS}}", anti != null && ResultPos(anti.AplicaCocaina, anti.ResultadoCocaina) ? "X" : "" },
                // Benzodiacepinas
                { "{{BZO_NEG}}", anti != null && ResultNeg(anti.AplicaBenzodiacepinas, anti.ResultadoBenzodiacepinas) ? "X" : "" },
                { "{{BZO_POS}}", anti != null && ResultPos(anti.AplicaBenzodiacepinas, anti.ResultadoBenzodiacepinas) ? "X" : "" },
                // Anfetaminas
                { "{{AMP_NEG}}", anti != null && ResultNeg(anti.AplicaAnfetaminas, anti.ResultadoAnfetaminas) ? "X" : "" },
                { "{{AMP_POS}}", anti != null && ResultPos(anti.AplicaAnfetaminas, anti.ResultadoAnfetaminas) ? "X" : "" },
                // Metanfetaminas
                { "{{MET_NEG}}", anti != null && ResultNeg(anti.AplicaMetanfetaminas, anti.ResultadoMetanfetaminas) ? "X" : "" },
                { "{{MET_POS}}", anti != null && ResultPos(anti.AplicaMetanfetaminas, anti.ResultadoMetanfetaminas) ? "X" : "" },
                // THC
                { "{{THC_NEG}}", anti != null && ResultNeg(anti.AplicaTHC, anti.ResultadoTHC) ? "X" : "" },
                { "{{THC_POS}}", anti != null && ResultPos(anti.AplicaTHC, anti.ResultadoTHC) ? "X" : "" },
                // Alcohol
                { "{{ALC_NEG}}", anti != null && ResultNeg(anti.AplicaAlcohol, anti.ResultadoAlcohol) ? "X" : "" },
                { "{{ALC_POS}}", anti != null && ResultPos(anti.AplicaAlcohol, anti.ResultadoAlcohol) ? "X" : "" },
            };

            foreach (var kv in rep)
                html = html.Replace(kv.Key, kv.Value);

            PaseHtml = html;
        }
    }
}

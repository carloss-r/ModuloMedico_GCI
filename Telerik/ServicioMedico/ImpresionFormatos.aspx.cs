using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Telerik.Models.ViewModels;
using Telerik.Models.DAL;

namespace Telerik.ServicioMedico
{
    public partial class ImpresionFormatos : System.Web.UI.Page
    {
        public int            IdOrden      { get; set; }
        public string         TipoDoc      { get; set; }
        public string         PaseHtml     { get; set; }
        public string         ErrorMessage { get; set; }
        public PacienteInfoVm Paciente     { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(Request.QueryString["id"], out int id))
                    throw new Exception("ID inv&aacute;lido.");

                IdOrden = id;
                TipoDoc = (Request.QueryString["tipo"] ?? "").ToUpper();

                if      (TipoDoc == "PASE")       GenerarPaseHtml();
                else if (TipoDoc == "EXAMEN")     GenerarExamenDesdeHtml();
                else if (TipoDoc == "ANTIDOPING") GenerarAntidopingDesdeHtml();
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
        }

        // ── PASE DE SERVICIO MÉDICO ──
        private void GenerarPaseHtml()
        {
            var orden = OrdenServicioMedicoDal.ObtenerPorId(IdOrden);
            if (orden == null) throw new Exception("No se encontró la información del pase.");

            string templatePath = Server.MapPath("~/ServicioMedico/Formatos/PaseMedico.html");
            if (!File.Exists(templatePath)) throw new Exception("Template de pase no encontrado.");

            string html = File.ReadAllText(templatePath, Encoding.UTF8);
            string H(object s) => HttpUtility.HtmlEncode(s?.ToString() ?? "");

            int apt = orden.FkAptitudMedica ?? 0;
            string empresa = !string.IsNullOrEmpty(orden.EmpresaCandidato) ? orden.EmpresaCandidato :
                            (!string.IsNullOrEmpty(orden.EmpresaNombre) ? orden.EmpresaNombre : orden.ProyectoDesc);
            empresa = string.IsNullOrWhiteSpace(empresa) ? "-" : empresa;

            var rep = new Dictionary<string, string>
            {
                { "{{EMPRESA}}",         H(empresa).ToUpper() },
                { "{{FECHA}}",           orden.FechaOrden?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy") },
                { "{{PROYECTO}}",        H(orden.ProyectoDesc ?? "-").ToUpper() },
                { "{{NOMBRE}}",          H(orden.NombrePersona ?? "-").ToUpper() },
                { "{{PUESTO}}",          H(orden.PuestoCandidato ?? "-").ToUpper() },
                { "{{RES_APTO}}",        apt == 1 ? "&#10004;" : "" },
                { "{{RES_CONDICIONADO}}",apt == 2 ? "&#10004;" : "" },
                { "{{RES_NO_APTO}}",     apt == 3 ? "&#10004;" : "" },
                { "{{RECOMENDACIONES}}", H(orden.Recomendaciones ?? "").ToUpper() },
            };

            foreach (var kv in rep) html = html.Replace(kv.Key, kv.Value);
            PaseHtml = html;
        }

        // ── EXAMEN MÉDICO ──
        private void GenerarExamenDesdeHtml()
        {
            var orden = OrdenServicioMedicoDal.ObtenerPorId(IdOrden);
            var eval = EvaluacionDal.ObtenerPorOrden(IdOrden);
            if (orden == null || eval == null) throw new Exception("Información médica incompleta.");

            string templatePath = Server.MapPath("~/ServicioMedico/Formatos/EvaluacionMedica.html");
            if (!File.Exists(templatePath)) throw new Exception("Template no encontrado.");
            string html = File.ReadAllText(templatePath, Encoding.UTF8);

            string H(object s) => HttpUtility.HtmlEncode(s?.ToString() ?? "");
            string Chk(bool v) => v ? "X" : "";
            string CV(int? i)  => i == 1 ? "N" : i == 2 ? "A" : i == 3 ? "D" : "";

            string sexo = (orden.SexoCandidato ?? "").ToUpper();
            bool esM = sexo.StartsWith("M") || sexo.Contains("MAS");
            bool esF = sexo.StartsWith("F") || sexo.Contains("FEM");
            bool esC = orden.FkEmpleado == null;

            string ec = (eval.EstadoCivil ?? "").ToUpper();
            string ew = (eval.Escolaridad ?? "").ToUpper();

            // Reemplazos Base
            var rep = new Dictionary<string, string> {
                {"{{LUGAR_FECHA}}",   H((eval.LugarEvaluacion ?? "Tula de Allende") + ", " + DateTime.Now.ToString("dd/MM/yyyy"))},
                {"{{CARGO}}",         H(orden.PuestoCandidato)}, {"{{NOMBRE}}", H(orden.NombrePersona)}, {"{{NSS}}", H(eval.Nss)},
                {"{{FECHA_NAC}}",     eval.FechaNacimiento?.ToString("dd/MM/yyyy") ?? ""}, 
                {"{{EDAD}}",          CalcularEdad(eval.FechaNacimiento)}, 
                {"{{LUGAR_NAC}}",     H(eval.LugarNacimiento)},
                {"{{EC_SOLTERO}}",    ec.Contains("SOLTER") || ec == "1" ? "X" : ""},
                {"{{EC_CASADO}}",     ec.Contains("CASAD")  || ec == "2" ? "X" : ""},
                {"{{EC_UNION}}",      ec.Contains("UNION")  || ec.Contains("UNI\u00d3N") || ec == "3" ? "X" : ""},
                {"{{EC_SEPARADO}}",   ec.Contains("SEPAR")  || ec.Contains("DIVOR") || ec.Contains("VIUD") || ec == "4" ? "X" : ""},
                {"{{MANO}}",          H(eval.ManoDominante)}, {"{{TELEFONO}}", H(eval.Telefono)}, {"{{DOMICILIO}}", H(eval.Domicilio)},
                {"{{ESC_PRIM}}",      ew.Contains("PRIM") ? "X" : ""}, {"{{ESC_SEC}}", ew.Contains("SECUN") ? "X" : ""},
                {"{{ESC_MED}}",       ew.Contains("MEDIA") || ew.Contains("PREPA") || ew.Contains("BACH") ? "X" : ""},
                {"{{ESC_UNI}}",       ew.Contains("UNIV")  || ew.Contains("LIC") || ew.Contains("PROF") || ew.Contains("POSG") ? "X" : ""},
                {"{{PROFESION}}",     H(eval.Profesion)}, {"{{EX_INGRESO}}", esC ? "X" : ""}, {"{{EX_PERIODICO}}", !esC ? "X" : ""},
                {"{{SEXO_MASC_BG}}",  esM ? "background:#333;color:#fff;" : "background:#eee;"}, {"{{SEXO_FEM_BG}}", esF ? "background:#333;color:#fff;" : "background:#eee;"}, {"{{TIPO_SANGRE}}", H(GetSangreText(eval.FkTipoSangre))},
                {"{{APP_OBSERVACIONES}}", H(eval.SintomasPaciente)}, {"{{DIAGNOSTICO}}", H(eval.Observaciones)}, {"{{RECOMENDACIONES}}", H(eval.Recomendaciones)},
                {"{{RES_APTO}}",      eval.FkAptitudMedica == 1 ? "X" : ""}, {"{{RES_NO_APTO}}", eval.FkAptitudMedica == 3 ? "X" : ""}, {"{{RES_RESTRICCIONES}}", eval.FkAptitudMedica == 2 ? "X" : ""},
                // Signos
                {"{{EF_TA}}",         eval.PresionSistolica + "/" + eval.PresionDiastolica}, {"{{EF_FC}}", H(eval.FrecuenciaCardiaca)}, {"{{EF_FR}}", H(eval.FrecuenciaRespiratoria)},
                {"{{EF_PESO}}",       H(eval.PesoKg)}, {"{{EF_TALLA}}", H(eval.AlturaMetros)}, {"{{EF_IMC}}", H(eval.Imc)}, {"{{EF_TEMP}}", H(eval.Temperatura)}, {"{{EF_APARATOS}}", H(eval.AparatosSistemas)}, {"{{EF_SINTOMAS}}", H(eval.SintomasPaciente)},
                // Habitos / Vacunas
                {"{{HAB_FUMA}}",      eval.Habitos != null ? Chk(eval.Habitos.Fuma) : ""}, {"{{HAB_ANOS_FUMA}}", H(eval.Habitos?.AnosFumando)}, {"{{HAB_CIGARROS}}", H(eval.Habitos?.CigarrosDiarios)}, {"{{HAB_EX_FUMADOR}}", eval.Habitos != null && eval.Habitos.EsExFumador ? "Si" : ""},
                {"{{HAB_DROGAS}}",    eval.Habitos != null ? Chk(eval.Habitos.UsaDrogas) : ""}, {"{{HAB_TIPO_DROGA}}", H(eval.Habitos?.TipoDrogas)}, {"{{HAB_ALCOHOL}}", eval.Habitos != null ? Chk(eval.Habitos.BebeAlcohol) : ""}, {"{{HAB_FREC_ALCOHOL}}", H(eval.Habitos?.FrecuenciaAlcohol)},
                {"{{HAB_DEPORTE}}",   eval.Habitos != null ? Chk(eval.Habitos.HaceDeporte) : ""}, {"{{HAB_TIPO_DEPORTE}}", H(eval.Habitos?.TipoDeporte)}, {"{{HAB_TIEMPO_LIBRE}}", H(eval.Habitos?.DescripcionTiempoLibre)},
                {"{{VAC_T1}}",        eval.Vacunacion != null ? Chk(eval.Vacunacion.TetanosDosis1) : ""}, {"{{VAC_T2}}", eval.Vacunacion != null ? Chk(eval.Vacunacion.TetanosDosis2) : ""}, {"{{VAC_T3}}", eval.Vacunacion != null ? Chk(eval.Vacunacion.TetanosDosis3) : ""},
                {"{{VAC_H1}}",        eval.Vacunacion != null ? Chk(eval.Vacunacion.HepatitisDosis1) : ""}, {"{{VAC_H2}}", eval.Vacunacion != null ? Chk(eval.Vacunacion.HepatitisDosis2) : ""}, {"{{VAC_H1N1}}", eval.Vacunacion != null ? Chk(eval.Vacunacion.InfluenzaH1N1) : ""},
                // Columna / Ginec
                {"{{CV_LORD_CERV}}",  CV(eval.Columna?.LordosisCervical)}, {"{{CV_LORD_DORS}}", CV(eval.Columna?.LordosisDorsal)}, {"{{CV_LORD_LUMB}}", CV(eval.Columna?.LordosisLumbar)}, {"{{CV_CIF_CERV}}", CV(eval.Columna?.CifosisCervical)}, {"{{CV_CIF_DORS}}", CV(eval.Columna?.CifosisDorsal)}, {"{{CV_CIF_LUMB}}", CV(eval.Columna?.CifosisLumbar)},
                {"{{ESC_DORS_DER}}",  eval.Columna != null ? Chk(eval.Columna.EscoliosisDorsalDerecha) : ""}, {"{{ESC_LUMB_DER}}", eval.Columna != null ? Chk(eval.Columna.EscoliosisLumbarDerecha) : ""}, {"{{ESC_DOBLE_DER}}", eval.Columna != null ? Chk(eval.Columna.EscoliosisDobleDerecha) : ""},
                {"{{ESC_DORS_IZQ}}",  eval.Columna != null ? Chk(eval.Columna.EscoliosisDorsalIzquierda) : ""}, {"{{ESC_LUMB_IZQ}}", eval.Columna != null ? Chk(eval.Columna.EscoliosisLumbarIzquierda) : ""}, {"{{ESC_DOBLE_IZQ}}", eval.Columna != null ? Chk(eval.Columna.EscoliosisDobleIzquierda) : ""},
                {"{{GF_MENARCA}}",    H(eval.DetalleFemenino?.EdadMenarca)}, {"{{GF_CICLOS}}", H(eval.DetalleFemenino?.Ciclos)}, {"{{GF_FUM}}", eval.DetalleFemenino?.FechaUltimaMenstruacion?.ToString("dd/MM/yyyy") ?? ""}, {"{{GF_PLANIFICACION}}", H(eval.DetalleFemenino?.MetodoPlanificacion)}, {"{{GF_CITVAG}}", eval.DetalleFemenino?.FechaUltimoPapanicolau?.ToString("dd/MM/yyyy") ?? ""}, {"{{GF_GESTAS}}", H(eval.DetalleFemenino?.Gestas)}, {"{{GF_PARTOS}}", H(eval.DetalleFemenino?.Partos)}, {"{{GF_ABORTOS}}", H(eval.DetalleFemenino?.Abortos)}, {"{{GF_CESAREAS}}", H(eval.DetalleFemenino?.Cesareas)},
                {"{{GM_PREPUCIO}}",   eval.DetalleMasculino != null ? Chk(eval.DetalleMasculino.PrepucioRetractil) : ""}, {"{{GM_TESTICULOS}}", eval.DetalleMasculino != null ? Chk(eval.DetalleMasculino.TesticulosDescendidos) : ""}, {"{{GM_FIMOSIS}}", eval.DetalleMasculino != null ? Chk(eval.DetalleMasculino.Fimosis) : ""}, {"{{GM_CRIPTORQUIDIA}}", eval.DetalleMasculino != null ? Chk(eval.DetalleMasculino.Criptorquidia) : ""}, {"{{GM_VARICOCELE}}", eval.DetalleMasculino != null ? Chk(eval.DetalleMasculino.Varicocele) : ""}, {"{{GM_HIDROCELE}}", eval.DetalleMasculino != null ? Chk(eval.DetalleMasculino.Hidrocele) : ""}, {"{{GM_HERNIA}}", eval.DetalleMasculino != null ? Chk(eval.DetalleMasculino.Hernia) : ""}, {"{{GM_PSA}}", H(eval.DetalleMasculino?.Psa)}, {"{{GM_MPF}}", H(eval.DetalleMasculino?.MetodoPlanificacion)},
            };

            // Antecedentes (Tags)
            string Ant(string kw) => eval.Antecedentes.Any(a => a.NombreCondicion.ToUpper().Contains(kw.ToUpper()) && a.EsPositivo) ? "X" : "";
            
            // AHF - Heredo Familiares
            var ahfKeys = new[] { "HTA", "CORONARIA", "ACV", "DIABETES", "TIROIDES", "ASMA", "ALERGIA", "TBC", "ALCOHOL", "EPILEPSIA", "MENTALES", "CONGENITAS", "CANCER", "VARICES" };
            foreach (var k in ahfKeys) rep["{{AHF_" + k + "}}"] = Ant(k);
            
            // APP - Personales Patológicos
            var appKeys = new[] { "HIPERTENSION", "QUIRURGICOS", "TRAUMATICOS", "ALERGICOS", "CONGENITOS", "METABOLICOS", "INFECCIOSOS", "TUMORALES", "RESPIRATORIAS", "MEDICAMENTOS", "TRANSFUSIONALES", "LITIASIS", "HACINAMIENTO", "AGUA", "ALCANTARILLADO", "OTROS" };
            foreach (var k in appKeys) rep["{{APP_" + k + "}}"] = Ant(k);

            // Gineco adicionales
            rep["{{GF_ETS}}"] = H(eval.DetalleFemenino?.Ets);
            rep["{{GF_IVSA}}"] = H(eval.DetalleFemenino?.Ivsa);
            rep["{{GF_HIJOS}}"] = H(eval.DetalleFemenino?.NumeroHijosEdades);

            // Campo Realizó (Doctor) - Por ahora línea en blanco o nombre si lo tuviéramos
            rep["{{REALIZO}}"] = "________________________________________________";

            // Laborales Rows
            var laborales = new StringBuilder();
            if (eval.AntecedentesLaborales != null && eval.AntecedentesLaborales.Count > 0) {
                foreach (var al in eval.AntecedentesLaborales)
                    laborales.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>\n",
                        H(al.Empresa), H(al.TiempoLaborado), H(al.Puesto), H(al.AgentesExpuesto), H(al.AccidentesPrevios));
            } else {
                laborales.Append("<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>\n");
            }
            rep["{{LABORALES_ROWS}}"] = laborales.ToString();

            // Examen Fisico (20 filas)
            AgregarTokensExamenFisico20(rep, eval.OrdenExamenFisico, H);

            foreach (var kv in rep) html = html.Replace(kv.Key, kv.Value ?? "");
            PaseHtml = html;
        }

        private static void AgregarTokensExamenFisico20(Dictionary<string, string> rep, List<OrdenExamenFisicoVm> exfs, Func<object, string> H)
        {
            var examSystems = new[] {
                "Cabeza", "Ojos", "Nariz", "Boca", "Dentadura", "Faringe", "Amígdalas", "Otoscopia",
                "Cuello", "Columna-espalda", "Extremidades", "Piel", "Ap. Respiratorio", "Cardiaco", "Vascular periférico",
                "Abdomen", "Neurológico", "Genitales", "Hernias", "Otro"
            };
            for (int i = 0; i < 20; i++) {
                string n = "", a = "", d = "";
                string target = examSystems[i].ToUpper();
                var item = exfs.FirstOrDefault(x => x.SistemaCuerpo.ToUpper().Contains(target));
                if (item != null) {
                    if (item.EsNormal) n = "X";
                    else { a = "X"; d = H(item.Hallazgos); }
                }
                int shipNum = i + 1;
                rep["{{EF_" + shipNum + "_N}}"] = n; rep["{{EF_" + shipNum + "_A}}"] = a; rep["{{EF_" + shipNum + "_D}}"] = d;
            }
        }

        // ── ANTIDOPING ──
        private void GenerarAntidopingDesdeHtml()
        {
            var orden = OrdenServicioMedicoDal.ObtenerPorId(IdOrden);
            var anti = AntidopingDal.ObtenerPorOrden(IdOrden);
            if (orden == null || anti == null) throw new Exception("Orden o prueba toxicológica no encontrada.");

            string templatePath = Server.MapPath("~/ServicioMedico/Formatos/Antidoping.html");
            if (!File.Exists(templatePath)) throw new Exception("Template de antidoping no encontrado.");
            string html = File.ReadAllText(templatePath, Encoding.UTF8);

            string H(object s) => HttpUtility.HtmlEncode(s?.ToString() ?? "");
            string ChkBox(bool marcado) => marcado ? "&#9745;" : "&#9744;";

            string veredictoHtml = "";
            bool esNoApto = (anti.VeredictoFinal ?? "").ToUpper().Contains("NO APTO");
            veredictoHtml = "<span style='border:2px solid #000; padding:4px 12px; font-weight:bold;'>" + 
                            (esNoApto ? "NO APTO PARA REALIZAR ACTIVIDADES OPERACIONALES" : "APTO PARA REALIZAR ACTIVIDADES OPERACIONALES") + "</span>";

            string fotoHtml = string.IsNullOrEmpty(anti.UrlFotoEvidencia) ? "" : $"<img src='{anti.UrlFotoEvidencia}' style='max-width:100%;max-height:100%;object-fit:contain;' />";

            // Helpers para construir filas dinámicas
            string BuildRow(string label, bool aplica, bool resultado) {
                if (!aplica) return "";
                string neg = !resultado ? "&#9745;" : "&#9744;";
                string pos =  resultado ? "&#9745;" : "&#9744;";
                return $"<tr><td class='substance-col'>{label}</td><td class='mark-col'>{neg}</td><td class='mark-col'>{pos}</td></tr>";
            }

            var rep = new Dictionary<string, string>
            {
                { "{{FECHA}}",        DateTime.Now.ToString("dd/MM/yyyy") },
                { "{{PROYECTO}}",     H(orden.ProyectoDesc) }, 
                { "{{EMPRESA}}",      H(orden.EmpresaNombre ?? orden.EmpresaCandidato) },
                { "{{NOMBRE}}",       H(orden.NombrePersona).ToUpper() }, 
                { "{{NUM_TRABAJADOR}}", orden.FkEmpleado?.ToString() ?? "-" },
                { "{{FOTO_HTML}}",    fotoHtml }, 
                { "{{VEREDICTO_HTML}}", veredictoHtml },
                { "{{COMENTARIOS}}",  H(anti.Comentarios) }, 
                { "{{MEDICO}}",       "LIC. NATALY MARTINEZ PUGA" },
                
                // Filas Dinámicas
                { "{{ROW_OPI}}", BuildRow("OPI (Opiaceos)",        anti.AplicaOpiaceos,        anti.ResultadoOpiaceos) },
                { "{{ROW_COC}}", BuildRow("COC (Cocaina)",         anti.AplicaCocaina,         anti.ResultadoCocaina) },
                { "{{ROW_BZO}}", BuildRow("BZO (Benzodiacepinas)", anti.AplicaBenzodiacepinas, anti.ResultadoBenzodiacepinas) },
                { "{{ROW_AMP}}", BuildRow("AMP (Anfetaminas)",      anti.AplicaAnfetaminas,      anti.ResultadoAnfetaminas) },
                { "{{ROW_MET}}", BuildRow("MET (Metanfetaminas)",   anti.AplicaMetanfetaminas,   anti.ResultadoMetanfetaminas) },
                { "{{ROW_THC}}", BuildRow("THC (Marihuana)",       anti.AplicaTHC,       anti.ResultadoTHC) },
                { "{{ROW_ALC}}", BuildRow("ALCOHOL",               anti.AplicaAlcohol,           anti.ResultadoAlcohol) },
            };

            foreach (var kv in rep) html = html.Replace(kv.Key, kv.Value);
            PaseHtml = html;
        }

        private string CalcularEdad(DateTime? fechaNac)
        {
            if (!fechaNac.HasValue) return "-";
            int edad = DateTime.Today.Year - fechaNac.Value.Year;
            if (fechaNac.Value.Date > DateTime.Today.AddYears(-edad)) edad--;
            return edad.ToString();
        }

        private string GetSangreText(int? id)
        {
            switch (id) {
                case 1: return "O+"; case 2: return "O-"; case 3: return "A+"; case 4: return "A-";
                case 5: return "B+"; case 6: return "B-"; case 7: return "AB+"; case 8: return "AB-";
                default: return "-";
            }
        }
    }
}

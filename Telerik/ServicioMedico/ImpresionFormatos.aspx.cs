using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Models.DAL;
using Telerik.Models.ViewModels;
using Telerik.Services;

namespace Telerik.ServicioMedico
{           
    public partial class ImpresionFormatos : System.Web.UI.Page
    {
        public int IdOrden { get; set; }
        public string TipoDoc { get; set; }
        public PacienteInfoVm Paciente { get; set; }
        public EvaluacionMedicaVm Evaluacion { get; set; }

        public string PaseHtml { get; set; }
        public string ErrorMessage { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (int.TryParse(Request.QueryString["id"], out int id))
                {
                    IdOrden = id;
                    TipoDoc = Request.QueryString["tipo"]?.ToUpper();

                    var ms = new MedicalService();
                    var orden = OrdenServicioMedicoDal.ObtenerPorId(id);
                    
                    if (orden == null) throw new Exception("La solicitud médica #" + id + " no fue encontrada.");

                    Paciente = ms.ObtenerInfoPaciente(orden);
                    Evaluacion = EvaluacionDal.ObtenerPorOrden(id);
                    
                    if (TipoDoc == "PASE")
                    {
                        GenerarPaseHtml();
                    }
                    else if (TipoDoc == "EXAMEN")
                    {
                        if (Evaluacion == null) throw new Exception("La evaluación completa aún no ha sido registrada.");
                        GenerarExamenHtml();
                    }
                }
                else
                {
                    throw new Exception("ID de solicitud inválido.");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private void GenerarPaseHtml()
        {
            string checkApto = (Evaluacion != null && Evaluacion.FkAptitudMedica == 1) ? "✔" : "";
            string checkCond = (Evaluacion != null && Evaluacion.FkAptitudMedica == 2) ? "✔" : "";
            string checkNoAp = (Evaluacion != null && Evaluacion.FkAptitudMedica == 3) ? "✔" : "";

            string empresa = !string.IsNullOrEmpty(Paciente?.Empresa) ? Paciente.Empresa : Paciente?.Proyecto ?? "";

            string template = @"<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width' />
    <title>Pase de Servicio Medico</title>
    <style>
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; color: #333; margin: 0; padding: 20px; }
        .print-content { max-width: 700px; margin: 0 auto; }
        table { width: 100%; border-collapse: collapse; }
        .main-table { border: 2px solid #333; margin-bottom: 0; }
        .main-table td { border-right: 1px solid #333; border-bottom: 1px solid #333; padding: 6px 10px; }
        .label-cell { font-size: 11px; font-weight: bold; width: 15%; background-color: #f9f9f9; }
        .value-cell { font-size: 12px; width: 35%; }
        .header-cell { text-align: center; padding: 8px; font-size: 15px; font-weight: bold; border-bottom: 2px solid #333; letter-spacing: 1px; background-color: #eee; -webkit-print-color-adjust: exact; print-color-adjust: exact; }
        .instruction-table { border-left: 2px solid #333; border-right: 2px solid #333; margin-top: 0; }
        .instruction-header { text-align: center; padding: 6px; background: #444; color: #fff; font-size: 10px; font-weight: bold; letter-spacing: 0.5px; -webkit-print-color-adjust: exact; print-color-adjust: exact; }
        .instruction-subheader { text-align: center; padding: 3px; font-size: 10px; color: #555; border-bottom: 1px solid #333; }
        .option-cell { width: 33%; text-align: center; padding: 12px; font-size: 12px; font-weight: bold; border-right: 1px solid #333; border-bottom: 1px solid #333; }
        .option-cell:last-child { border-right: none; }
        .special-table { border-left: 2px solid #333; border-right: 2px solid #333; }
        .fill-instruction { padding: 3px 10px; font-size: 9px; color: #888; text-align: center; border-bottom: 1px solid #999; }
        .special-label { padding: 8px 10px; font-size: 11px; color: #333; }
        .line-row td { padding: 5px 10px; border-bottom: 1px solid #999; }
        .signature-table { border: 2px solid #333; margin-top: 0; }
        .signature-space { height: 50px; border-right: 1px solid #333; border-bottom: 1px solid #333; }
        .signature-space:last-child { border-right: none; }
        .signature-label { width: 33%; text-align: center; padding: 6px; font-size: 9px; font-weight: bold; border-right: 1px solid #333; vertical-align: top; }
        .signature-label:last-child { border-right: none; }
        @media print { body { padding: 0; } .no-print { display: none !important; } * { -webkit-print-color-adjust: exact !important; print-color-adjust: exact !important; } }
    </style>
</head>
<body>
    <div class='print-content'>
        <table class='main-table'>
            <tr>
                <td colspan='4' class='header-cell'>SERVICIO MEDICO</td>
            </tr>
            <tr>
                <td class='label-cell'>EMPRESA</td>
                <td class='value-cell'>{{EMPRESA}}</td>
                <td class='label-cell'>FECHA:</td>
                <td class='value-cell'>{{FECHA}}</td>
            </tr>
            <tr>
                <td class='label-cell'>PROYECTO</td>
                <td colspan='3' class='value-cell'>{{PROYECTO}}</td>
            </tr>
            <tr>
                <td colspan='4' style='padding:6px 10px; font-size:11px; border-bottom:1px solid #333;'>
                    <span style='font-weight:bold;'>POR ESTE CONDUCTO LE ENVIO AL SR.(A):</span>
                    <span style='margin-left:8px; font-size:12px; text-transform:uppercase;'>{{PACIENTE}}</span>
                </td>
            </tr>
            <tr>
                <td colspan='4' style='padding:6px 10px; font-size:11px; border-bottom:1px solid #333;'>
                    <span style='font-weight:bold;'>CANDIDATO(A) A OCUPAR EL PUESTO DE:</span>
                    <span style='margin-left:8px; font-size:12px; text-transform:uppercase;'>{{PUESTO}}</span>
                </td>
            </tr>
        </table>

        <table class='instruction-table'>
            <tr>
                <td colspan='3' class='instruction-header'>INSTRUCCIÓN AL AREA DE RECLUTAMIENTO Y SELECCIÓN</td>
            </tr>
            <tr>
                <td colspan='3' class='instruction-subheader'>El candidato es considerado como:</td>
            </tr>
            <tr>
                <td class='option-cell' style='vertical-align: middle;'>APTO <br/><span style='font-size: 20px; color: #1a5276;'>{{CHECK_APTO}}</span></td>
                <td class='option-cell' style='vertical-align: middle;'>APTO CONDICIONADO <br/><span style='font-size: 20px; color: #1a5276;'>{{CHECK_COND}}</span></td>
                <td class='option-cell' style='vertical-align: middle;'>NO APTO <br/><span style='font-size: 20px; color: #1a5276;'>{{CHECK_NOAP}}</span></td>
            </tr>
        </table>

        <table class='special-table'>
            <tr>
                <td class='fill-instruction'>RELLENAR O MARCAR LA CASILLA INDICADA</td>
            </tr>
            <tr>
                <td class='special-label'>Existe alguna indicación especial para el candidato:</td>
            </tr>
            <tr class='line-row'>
                <td style='height: 60px; vertical-align: top; font-size: 11px; padding: 5px 15px;'>{{RECOMENDACIONES}}</td>
            </tr>
        </table>

        <table class='signature-table'>
            <tr>
                <td class='signature-space'>&nbsp;</td>
                <td class='signature-space'>&nbsp;</td>
                <td class='signature-space'>&nbsp;</td>
            </tr>
            <tr>
                <td class='signature-label'>Nombre y firma<br/>Reclutamiento</td>
                <td class='signature-label'>Vo. Bo. Nombre y firma<br/>del médico</td>
                <td class='signature-label'>EN CASO DE SER CONDICIONADO (NOMBRE Y FIRMA DE QUIEN AUTORIZA)</td>
            </tr>
        </table>
    </div>
</body>
</html>";

            PaseHtml = template
                .Replace("{{EMPRESA}}", HttpUtility.HtmlEncode(empresa.ToUpper()))
                .Replace("{{FECHA}}", DateTime.Now.ToString("dd/MM/yyyy"))
                .Replace("{{PROYECTO}}", HttpUtility.HtmlEncode((Paciente?.Proyecto ?? "").ToUpper()))
                .Replace("{{PACIENTE}}", HttpUtility.HtmlEncode((Paciente?.NombreCompleto ?? "").ToUpper()))
                .Replace("{{PUESTO}}", HttpUtility.HtmlEncode((Paciente?.Puesto ?? "").ToUpper()))
                .Replace("{{CHECK_APTO}}", checkApto)
                .Replace("{{CHECK_COND}}", checkCond)
                .Replace("{{CHECK_NOAP}}", checkNoAp)
                .Replace("{{RECOMENDACIONES}}", HttpUtility.HtmlEncode((Evaluacion?.Recomendaciones ?? "").ToUpper()));
        }

        private void GenerarExamenHtml()
        {
            string E(string s) => HttpUtility.HtmlEncode(s ?? "");
            bool isCand = (Paciente?.Tipo ?? "").Trim().ToUpper() == "CANDIDATO";
            string lugar = string.IsNullOrWhiteSpace(Evaluacion?.LugarEvaluacion) ? "Tula de Allende" : Evaluacion.LugarEvaluacion;
            string fecha = DateTime.Now.ToString("dd/MM/yyyy");
            bool esM = Paciente != null && ((Paciente.Sexo ?? "").ToUpper() == "M" || (Paciente.Sexo ?? "").ToUpper().Contains("MASC"));
            bool esF = Paciente != null && ((Paciente.Sexo ?? "").ToUpper() == "F" || (Paciente.Sexo ?? "").ToUpper().Contains("FEM"));
            string Ant(string c){if(Evaluacion?.Antecedentes==null)return"";var u=c.ToUpperInvariant();var a=Evaluacion.Antecedentes.FirstOrDefault(x=>x?.NombreCondicion!=null&&(x.NombreCondicion.ToUpperInvariant().Contains(u)||u.Contains(x.NombreCondicion.ToUpperInvariant())));return(a!=null&&a.EsPositivo)?"X":"";}
            string ts="";switch(Evaluacion?.FkTipoSangre){case 1:ts="O+";break;case 2:ts="O-";break;case 3:ts="A+";break;case 4:ts="A-";break;case 5:ts="B+";break;case 6:ts="B-";break;case 7:ts="AB+";break;case 8:ts="AB-";break;}
            string ec=(Evaluacion?.EstadoCivil??"").Trim().ToUpper();
            string esc=(Evaluacion?.Escolaridad??"").Trim().ToUpper();
            string ecS=(ec.Contains("SOLTER")||ec=="1")?"X":"";
            string ecC=(ec.Contains("CASAD")||ec=="2")?"X":"";
            string ecU=(ec.Contains("UNION")||ec.Contains("UNIÓN")||ec=="3")?"X":"";
            string ecP=(ec.Contains("SEPAR")||ec.Contains("DIVOR")||ec.Contains("VIUD")||ec=="4")?"X":"";
            string nP=esc.Contains("PRIM")?"X":"";
            string nS=esc.Contains("SECUN")?"X":"";
            string nM=(esc.Contains("MEDIA")||esc.Contains("PREPA")||esc.Contains("BACH"))?"X":"";
            string nU=(esc.Contains("UNIV")||esc.Contains("LIC")||esc.Contains("PROF")||esc.Contains("POSG"))?"X":"";
            string dir=Evaluacion?.Domicilio;
            if(string.IsNullOrWhiteSpace(dir)){var pp=new List<string>();if(!string.IsNullOrWhiteSpace(Evaluacion?.Calle))pp.Add(Evaluacion.Calle);if(!string.IsNullOrWhiteSpace(Evaluacion?.NumExterior))pp.Add("#"+Evaluacion.NumExterior);if(!string.IsNullOrWhiteSpace(Evaluacion?.NumInterior))pp.Add("Int. "+Evaluacion.NumInterior);if(pp.Count>0)dir=string.Join(" ",pp);}
            string Chk(bool v)=>v?"X":"";
            var sb=new StringBuilder();
            sb.Append("<style>");
            sb.Append("@page{size:letter;margin:8mm;}");
            sb.Append("*{margin:0;padding:0;box-sizing:border-box;}");
            sb.Append("body{font-family:Arial,sans-serif;font-size:7.5pt;color:#000;}");
            sb.Append(".pg{width:100%;max-width:780px;margin:0 auto;page-break-after:always;position:relative;min-height:260mm;padding-bottom:30px;}");
            sb.Append(".pg:last-child{page-break-after:auto;}");
            sb.Append("table{border-collapse:collapse;}");
            sb.Append(".hdr{width:100%;border:1.5px solid #000;}");
            sb.Append(".hdr td{border:1px solid #000;text-align:center;vertical-align:middle;}");
            sb.Append(".hdr .logo{width:18%;padding:3px;}");
            sb.Append(".hdr .logo img{max-height:45px;}");
            sb.Append(".hdr .ttl{font-weight:bold;font-size:9pt;}");
            sb.Append(".hj{text-align:right;font-weight:bold;font-size:7.5pt;margin:1px 5px 3px 0;}");
            sb.Append(".fm{width:100%;border:1px solid #000;border-collapse:collapse;}");
            sb.Append(".fm td{border:1px solid #000;padding:2px 4px;font-size:7.5pt;vertical-align:middle;}");
            sb.Append(".lb{font-weight:bold;white-space:nowrap;}");
            sb.Append(".bx{display:inline-block;width:11px;height:11px;border:1.5px solid #000;text-align:center;line-height:10px;font-size:7pt;font-weight:bold;vertical-align:middle;margin:0 2px;}");
            sb.Append(".bar{background:#d9d9d9;border:1px solid #000;text-align:center;font-weight:bold;font-size:7.5pt;padding:2px 0;-webkit-print-color-adjust:exact;print-color-adjust:exact;}");
            sb.Append(".ul{display:inline-block;min-width:22px;border-bottom:1px solid #000;text-align:center;font-weight:bold;padding:0 3px;font-size:7pt;}");
            sb.Append(".dt{width:100%;border:1px solid #000;border-collapse:collapse;font-size:7.5pt;}");
            sb.Append(".dt th,.dt td{border:1px solid #000;padding:2px 4px;}");
            sb.Append(".dt th{background:#d9d9d9;font-weight:bold;text-align:center;-webkit-print-color-adjust:exact;print-color-adjust:exact;}");
            sb.Append(".ftr{font-weight:bold;font-size:9pt;position:absolute;bottom:5px;left:5px;}");
            sb.Append("@media print{*{-webkit-print-color-adjust:exact!important;print-color-adjust:exact!important;}}");
            sb.Append("</style>");
            // PAGE 1
            sb.Append("<div class='pg'>");
            sb.Append("<table class='hdr'><tr><td class='logo' rowspan='2'><img src='/Content/Images/Logo_GCI.png' alt='GCI'/></td>");
            sb.Append("<td class='ttl'>SEGURIDAD, SALUD Y MEDIO AMBIENTE</td>");
            sb.Append("<td class='logo' rowspan='2'><img src='/Content/Images/Logo_HSE.png' alt='HSE'/></td></tr>");
            sb.Append("<tr><td class='ttl' style='padding:2px;'>EXAMEN M&#201;DICO<br/><span style='font-size:7.5pt;font-weight:normal;'>GRUPO CONSTRUCTOR INDUSTRIAL O&#205;L &amp; GAS S.A. DE C.V.</span></td></tr></table>");
            sb.Append("<div class='hj'>HOJA: <span style='border:1px solid #000;padding:0 8px;'>1</span> DE <span style='border:1px solid #000;padding:0 8px;'>2</span></div>");
            // Form fields
            sb.Append("<table class='fm'>");
            sb.Append("<tr><td class='lb'>Lugar y fecha del Examen:</td><td colspan='5'>"+E(lugar)+", "+E(fecha)+"</td><td class='lb'>Cargo:</td><td>"+E(Paciente?.Puesto)+"</td></tr>");
            sb.Append("<tr><td class='lb'>Nombre:</td><td colspan='5'>"+E(Paciente?.NombreCompleto)+"</td><td class='lb'>No. IMSS</td><td>"+E(Evaluacion?.Nss)+"</td></tr>");
            sb.Append("<tr><td class='lb'>Fecha de Nacimiento:</td><td colspan='2'>"+E(Evaluacion?.FechaNacimiento?.ToString("dd/MM/yyyy"))+"</td><td class='lb' style='text-align:center;'>Edad:</td><td style='text-align:center;'>"+E(Paciente?.Edad)+"</td><td>a&#241;os</td><td class='lb'>Lugar de nacimiento:</td><td>"+E(Evaluacion?.LugarNacimiento)+"</td></tr>");
            sb.Append("<tr><td class='lb'>Estado Civil:</td><td colspan='3'><span class='bx'>"+ecS+"</span>soltero <span class='bx'>"+ecC+"</span>casado <span class='bx'>"+ecU+"</span>union libre <span class='bx'>"+ecP+"</span>separado</td><td colspan='2'>Mano dominante "+E(Evaluacion?.ManoDominante)+"</td><td class='lb'>Tel&#233;fono:</td><td>"+E(Evaluacion?.Telefono)+"</td></tr>");
            sb.Append("<tr><td class='lb'>Domicilio:</td><td colspan='7'>"+E(dir)+"</td></tr>");
            sb.Append("<tr><td class='lb'>Nivel Acad&#233;mico:</td><td colspan='3'><span class='bx'>"+nP+"</span>Primaria <span class='bx'>"+nS+"</span>Secundaria <span class='bx'>"+nM+"</span>Media Sup. <span class='bx'>"+nU+"</span>Universidad</td><td colspan='2' class='lb'>Profesi&#243;n:</td><td colspan='2'>"+E(Evaluacion?.Profesion)+"</td></tr>");
            sb.Append("<tr><td class='lb'>Examen de:</td><td><span class='bx'>"+(isCand?"X":"")+"</span>Ingreso <span class='bx'>"+(!isCand?"X":"")+"</span>Peri&#243;dico</td><td class='lb' style='text-align:center;'>Sexo:</td><td><span class='bx' style='"+(esM?"background:#000;color:#fff;":"")+"'>"+( esM?"X":"")+"</span>Masc <span class='bx' style='"+(esF?"background:#000;color:#fff;":"")+"'>"+(esF?"X":"")+"</span>Fem</td><td colspan='2' class='lb'>Tipo de Sangre:</td><td colspan='2' style='text-align:center;'>"+E(ts)+"</td></tr>");
            sb.Append("</table>");
            // ANTECEDENTE HEREDO FAMILIARES
            sb.Append("<div class='bar'>ANTECEDENTE HEREDO FAMILIARES</div>");
            sb.Append("<table class='dt' style='border-top:none;'>");
            sb.Append("<tr><td><span class='ul'>"+Ant("HTA")+"</span> HTA</td><td><span class='ul'>"+Ant("DIABETES")+"</span> DIABETES</td><td><span class='ul'>"+Ant("ALERGIA")+"</span> ALERGIA</td><td><span class='ul'>"+Ant("EPILEPSIA")+"</span> EPILEPSIA</td><td><span class='ul'>"+Ant("CANCER")+"</span> C&#193;NCER</td></tr>");
            sb.Append("<tr><td><span class='ul'>"+Ant("CORONARIA")+"</span> ENF CORONARIA</td><td><span class='ul'>"+Ant("TIROIDES")+"</span> TIROIDES</td><td><span class='ul'>"+Ant("TBC")+"</span> TBC</td><td><span class='ul'>"+Ant("MENTALES")+"</span> MENTALES</td><td><span class='ul'>"+Ant("VARICES")+"</span> V&#193;RICES</td></tr>");
            sb.Append("<tr><td><span class='ul'>"+Ant("ACV")+"</span> ACV</td><td><span class='ul'>"+Ant("ASMA")+"</span> ASMA</td><td><span class='ul'>"+Ant("ALCOHOL")+"</span> ALCOHOL</td><td><span class='ul'>"+Ant("CONGENITA")+"</span> CONG&#201;NITAS</td><td></td></tr>");
            sb.Append("</table>");
            // ANTECEDENTES PERSONALES Patologicos
            sb.Append("<div class='bar'>ANTECEDENTES PERSONALES Patol&#243;gicos</div>");
            sb.Append("<table class='dt' style='border-top:none;'>");
            sb.Append("<tr><td><span class='ul'>"+Ant("HIPERTENSION")+"</span> HIPERTENSI&#211;N</td><td><span class='ul'>"+Ant("CONGENITO")+"</span> CONG&#201;NITOS</td><td><span class='ul'>"+Ant("RESPIRATORIA")+"</span> ENF. RESPIRATORIAS</td><td><span class='ul'>"+Ant("HACINAMIENTO")+"</span> HACINAMIENTO</td></tr>");
            sb.Append("<tr><td><span class='ul'>"+Ant("QUIRURGICO")+"</span> QUIR&#218;RGICOS</td><td><span class='ul'>"+Ant("METABOLICO")+"</span> METAB&#211;LICOS</td><td><span class='ul'>"+Ant("MEDICAMENTO")+"</span> MEDICAMENTOS</td><td><span class='ul'>"+Ant("AGUA")+"</span> AGUA POTABLE</td></tr>");
            sb.Append("<tr><td><span class='ul'>"+Ant("TRAUMATICO")+"</span> TRAUM&#193;TICOS</td><td><span class='ul'>"+Ant("INFECCIOSO")+"</span> INFECCIOSOS</td><td><span class='ul'>"+Ant("TRANSFUSION")+"</span> TRANSFUSIONALES</td><td><span class='ul'>"+Ant("ALCANTARILLADO")+"</span> ALCANTARILLADO</td></tr>");
            sb.Append("<tr><td><span class='ul'>"+Ant("ALERGICO")+"</span> AL&#201;RGICOS</td><td><span class='ul'>"+Ant("TUMORAL")+"</span> TUMORALES</td><td><span class='ul'>"+Ant("LITIASIS")+"</span> LITIASIS</td><td><span class='ul'>"+Ant("OTRO")+"</span> OTROS:</td></tr>");
            sb.Append("<tr><td colspan='4' style='font-size:7pt;'>Observaciones: "+E(Evaluacion?.Observaciones)+"</td></tr>");
            sb.Append("<tr><td colspan='4' style='font-size:7pt;'>ANTECEDENTES PERSONALES NO PATOL&#211;GICOS:</td></tr>");
            sb.Append("</table>");
            // ANTECEDENTES LABORALES
            sb.Append("<div class='bar'>ANTECEDENTES LABORALES</div>");
            sb.Append("<table class='dt' style='border-top:none;'><tr><th>EMPRESA</th><th>TIEMPO</th><th>PUESTO</th><th>AGENTES EXPUESTOS</th><th>ACCIDENTES</th></tr>");
            if(Evaluacion?.AntecedentesLaborales!=null&&Evaluacion.AntecedentesLaborales.Any()){foreach(var al in Evaluacion.AntecedentesLaborales){sb.Append("<tr><td>"+E(al?.Empresa)+"</td><td>"+E(al?.TiempoLaborado)+"</td><td>"+E(al?.Puesto)+"</td><td>"+E(al?.AgentesExpuesto)+"</td><td>"+E(al?.AccidentesPrevios)+"</td></tr>");}}
            else{sb.Append("<tr><td>&nbsp;</td><td></td><td></td><td></td><td></td></tr>");}
            sb.Append("</table>");
            // HABITOS
            sb.Append("<div class='bar'>HABITOS</div>");
            var h=Evaluacion?.Habitos;
            sb.Append("<table class='dt' style='border-top:none;'>");
            sb.Append("<tr><td style='width:70px;'>Fuma:</td><td>A&#241;os de h&#225;bito <span class='ul'>"+E(h?.AnosFumando?.ToString())+"</span></td><td>No. De Cigarros/dia: <span class='ul'>"+E(h?.CigarrosDiarios?.ToString())+"</span></td><td></td><td>EX <span class='ul'>"+(h!=null&&h.EsExFumador?"Si":"")+"</span></td></tr>");
            sb.Append("<tr><td>Drogas:</td><td colspan='4'>Tipo de droga: <span class='ul' style='min-width:200px;'>"+E(h?.TipoDrogas)+"</span></td></tr>");
            sb.Append("<tr><td>Alcohol:</td><td colspan='4'><span class='ul' style='min-width:200px;'>"+E(h?.FrecuenciaAlcohol)+"</span></td></tr>");
            sb.Append("<tr><td>Deporte:</td><td colspan='2'></td><td colspan='2'>Frecuencia <span class='ul'>"+E(h?.TipoDeporte)+"</span></td></tr>");
            sb.Append("<tr><td>Tiempo Libre:</td><td colspan='4'><span class='ul' style='min-width:300px;'>"+E(h?.DescripcionTiempoLibre)+"</span></td></tr>");
            sb.Append("</table>");
            // VACUNAS
            var vac=Evaluacion?.Vacunacion;
            sb.Append("<div style='border:1px solid #000;border-top:none;padding:3px 5px;font-size:7.5pt;'>");
            sb.Append("VACUNAS &nbsp;&nbsp;&nbsp; T&#201;TANOS &nbsp;<span class='ul'>"+(vac!=null&&vac.TetanosDosis1?"X":"")+"</span> 1 &nbsp;<span class='ul'>"+(vac!=null&&vac.TetanosDosis2?"X":"")+"</span> 2 &nbsp;<span class='ul'>"+(vac!=null&&vac.TetanosDosis3?"X":"")+"</span> 3");
            sb.Append(" &nbsp;&nbsp;&nbsp;&nbsp; Hepatitis &nbsp;<span class='ul'>"+(vac!=null&&vac.HepatitisDosis1?"X":"")+"</span> 1 &nbsp;<span class='ul'>"+(vac!=null&&vac.HepatitisDosis2?"X":"")+"</span> 2");
            sb.Append(" &nbsp;&nbsp;&nbsp;&nbsp; H1N1: <span class='ul'>"+(vac!=null&&vac.InfluenzaH1N1?"X":"")+"</span>");
            sb.Append("</div>");
            // EXPLORACION FISICA
            sb.Append("<div class='bar'>EXPLORACION FISICA</div>");
            sb.Append("<table class='fm' style='border-top:none;'>");
            sb.Append("<tr><td class='lb'>TA:</td><td>"+E(Evaluacion?.PresionSistolica?.ToString())+"/"+E(Evaluacion?.PresionDiastolica?.ToString())+"</td><td>mmHg</td><td class='lb'>FC:</td><td>"+E(Evaluacion?.FrecuenciaCardiaca?.ToString())+"</td><td>x min</td><td class='lb'>FR:</td><td>"+E(Evaluacion?.FrecuenciaRespiratoria?.ToString())+"</td><td>x min</td><td class='lb'>Peso:</td><td>"+E(Evaluacion?.PesoKg?.ToString())+"</td><td>kgs</td><td class='lb'>Estatura:</td><td>"+E(Evaluacion?.AlturaMetros?.ToString())+"</td><td>m</td></tr>");
            sb.Append("<tr><td class='lb'>IMC:</td><td>"+E(Evaluacion?.Imc?.ToString())+"</td><td colspan='3'></td><td class='lb'>Temp:</td><td>"+E(Evaluacion?.Temperatura?.ToString())+"</td><td colspan='3'>Aparatos y sistemas:</td><td colspan='5'>"+E(Evaluacion?.AparatosSistemas)+"</td></tr>");
            sb.Append("<tr><td class='lb'>S&#237;ntomas:</td><td colspan='14'>"+E(Evaluacion?.SintomasPaciente)+"</td></tr>");
            sb.Append("</table>");
            // EXAMEN FISICO TABLE
            string[] sist={"1. Cabeza:","2. Ojos:","3. Nariz:","4. Boca:","5. Dentadura:","6. Faringe:","7. Amigdalas:","8. Otoscopia:","9. Cuello:","10. Columna-espalda:","11. Extremidades:","12. Piel:","13. Ap. Respiratorio","14. Cardiaco:","15. Vascular periferico","16. Abdomen:","17. Neurologico:","18. Genitales:","19. Hernias:","20. Otro:"};
            sb.Append("<table class='dt' style='border-top:none;'>");
            sb.Append("<tr><th style='width:22%;'></th><th style='width:10%;'>Normal</th><th style='width:10%;'>Anormal</th><th>Descripci&#243;n de Hallazgos</th></tr>");
            foreach(var s in sist){var ef=Evaluacion?.OrdenExamenFisico?.FirstOrDefault(x=>x?.SistemaCuerpo!=null&&(x.SistemaCuerpo.Trim().Equals(s.TrimEnd(':').Trim(),StringComparison.OrdinalIgnoreCase)||s.ToUpperInvariant().Contains(x.SistemaCuerpo.Trim().ToUpperInvariant())));string n=(ef!=null&&ef.EsNormal)?"X":"";string a=(ef!=null&&!ef.EsNormal&&ef.Hallazgos!=null)?"X":"";sb.Append("<tr><td style='font-weight:bold;padding-left:3px;'>"+E(s)+"</td><td style='text-align:center;'>"+n+"</td><td style='text-align:center;'>"+a+"</td><td style='font-size:7pt;'>"+E(ef?.Hallazgos)+"</td></tr>");}
            sb.Append("</table>");
            sb.Append("<div class='ftr'>GCI-FOR-SYM-45 REV. 0</div>");
            sb.Append("</div>"); // end page 1
            // PAGE 2
            sb.Append("<div class='pg'>");
            sb.Append("<table class='hdr'><tr><td class='logo' rowspan='2'><img src='/Content/Images/Logo_GCI.png' alt='GCI'/></td>");
            sb.Append("<td class='ttl'>SEGURIDAD, SALUD Y MEDIO AMBIENTE</td>");
            sb.Append("<td class='logo' rowspan='2'><img src='/Content/Images/Logo_HSE.png' alt='HSE'/></td></tr>");
            sb.Append("<tr><td class='ttl' style='padding:2px;'>EXAMEN MEDICO<br/><span style='font-size:7.5pt;font-weight:normal;'>GRUPO CONSTRUCTOR INDUSTRIAL OIL &amp; GAS S.A DE C.V.</span></td></tr></table>");
            sb.Append("<div class='hj'>HOJA: <span style='border:1px solid #000;padding:0 8px;'>2</span> DE <span style='border:1px solid #000;padding:0 8px;'>2</span></div>");
            // GINECO-OBSTETRICOS
            sb.Append("<table class='fm'>");
            sb.Append("<tr><td class='lb' style='font-weight:bold;'>GINECO-OBST&#201;TRICOS:</td>");
            if(esF && Evaluacion?.DetalleFemenino!=null){var gf=Evaluacion.DetalleFemenino;
            sb.Append("<td class='lb'>Menarca:</td><td>"+E(gf.EdadMenarca?.ToString())+"</td><td class='lb'>Ciclos:</td><td>"+E(gf.Ciclos)+"</td><td class='lb'>FUM:</td><td>"+E(gf.FechaUltimaMenstruacion?.ToString("dd/MM/yyyy"))+"</td><td class='lb'>No. Hijos/Edades:</td><td>"+E(gf.NumeroHijosEdades)+"</td></tr>");
            sb.Append("<tr><td class='lb'>Planificaci&#243;n:</td><td>"+E(gf.MetodoPlanificacion)+"</td><td class='lb'>IVSA:</td><td>"+E(gf.Ivsa?.ToString())+"</td><td class='lb'>Cit. Vag:</td><td>"+E(gf.FechaUltimoPapanicolau?.ToString("dd/MM/yyyy"))+"</td><td class='lb'>ETS:</td><td colspan='2'>"+E(gf.Ets)+"</td></tr>");
            sb.Append("<tr><td class='lb'>Gestas:</td><td>"+E(gf.Gestas?.ToString())+"</td><td class='lb'>P:</td><td>"+E(gf.Partos?.ToString())+"</td><td class='lb'>A:</td><td>"+E(gf.Abortos?.ToString())+"</td><td class='lb'>C:</td><td>"+E(gf.Cesareas?.ToString())+"</td><td></td></tr>");}
            else{sb.Append("<td class='lb'>Menarca:</td><td></td><td class='lb'>Ciclos:</td><td></td><td class='lb'>FUM:</td><td></td><td class='lb'>No. Hijos/Edades:</td><td></td></tr>");
            sb.Append("<tr><td class='lb'>Planificaci&#243;n:</td><td></td><td class='lb'>IVSA:</td><td></td><td class='lb'>Cit. Vag:</td><td></td><td class='lb'>ETS:</td><td colspan='2'></td></tr>");
            sb.Append("<tr><td class='lb'>Gestas:</td><td></td><td class='lb'>P:</td><td></td><td class='lb'>A:</td><td></td><td class='lb'>C:</td><td></td><td></td></tr>");}
            sb.Append("</table>");
            // MASCULINO
            sb.Append("<div style='margin-top:10px;padding:5px;border:1px solid #000;'>");
            sb.Append("<div style='font-weight:bold;font-size:7.5pt;margin-bottom:6px;'>ANTECEDENTES DE APARATO GENITOURINARIO MASCULINO:</div>");
            if(esM && Evaluacion?.DetalleMasculino!=null){var gm=Evaluacion.DetalleMasculino;
            sb.Append("<div style='font-size:7.5pt;margin-bottom:3px;'>Examen Cl&#237;nico: Prepicio retr&#225;ctil <span class='bx'>"+Chk(gm.PrepucioRetractil)+"</span>");
            sb.Append(" &nbsp;&nbsp;Test&#237;culos: Descendidos: <span class='bx'>"+Chk(gm.TesticulosDescendidos)+"</span>");
            sb.Append(" &nbsp;&nbsp;Fimosis: <span class='bx'>"+Chk(gm.Fimosis)+"</span>");
            sb.Append(" &nbsp;&nbsp;Criptorquidia: <span class='bx'>"+Chk(gm.Criptorquidia)+"</span>");
            sb.Append(" &nbsp;&nbsp;Varicocele: <span class='bx'>"+Chk(gm.Varicocele)+"</span></div>");
            sb.Append("<div style='font-size:7.5pt;margin-bottom:3px;margin-left:30px;'>Hidrocele: <span class='bx'>"+Chk(gm.Hidrocele)+"</span>");
            sb.Append(" &nbsp;&nbsp;Hernia: <span class='bx'>"+Chk(gm.Hernia)+"</span>");
            sb.Append(" &nbsp;&nbsp;IVSA: <span class='ul' style='min-width:40px;'>"+E(gm.Ivsa)+"</span>");
            sb.Append(" &nbsp;&nbsp;PSA: <span class='bx'>"+E(gm.Psa)+"</span>");
            sb.Append(" &nbsp;&nbsp;MPF: <span class='bx'>"+E(gm.MetodoPlanificacion)+"</span></div>");
            sb.Append("<div style='font-size:7.5pt;margin-left:30px;'>No. de Hijos/Edades: <span class='ul' style='min-width:250px;'></span></div>");}
            else{sb.Append("<div style='font-size:7.5pt;margin-bottom:3px;'>Examen Cl&#237;nico: Prepicio retr&#225;ctil <span class='bx'></span> &nbsp;&nbsp;Test&#237;culos: Descendidos: <span class='bx'></span> &nbsp;&nbsp;Fimosis: <span class='bx'></span> &nbsp;&nbsp;Criptorquidia: <span class='bx'></span> &nbsp;&nbsp;Varicocele: <span class='bx'></span></div>");
            sb.Append("<div style='font-size:7.5pt;margin-bottom:3px;margin-left:30px;'>Hidrocele: <span class='bx'></span> &nbsp;&nbsp;Hernia: <span class='bx'></span> &nbsp;&nbsp;IVSA: <span class='ul'></span> &nbsp;&nbsp;PSA: <span class='bx'></span> &nbsp;&nbsp;MPF: <span class='bx'></span></div>");
            sb.Append("<div style='font-size:7.5pt;margin-left:30px;'>No. de Hijos/Edades: <span class='ul' style='min-width:250px;'></span></div>");}
            sb.Append("</div>");
            // COLUMNA VERTEBRAL
            var col=Evaluacion?.Columna;
            string CV(int? val){if(val==null)return"";if(val==1)return"N";if(val==2)return"A";if(val==3)return"D";return val.ToString();}
            sb.Append("<div style='margin-top:8px;'>");
            sb.Append("<div style='font-weight:bold;font-size:7.5pt;'>COLUMNA VERTEBRAL: &nbsp;&nbsp;&nbsp;&nbsp; N: Normal &nbsp;&nbsp;&nbsp;&nbsp; A: Aumentada &nbsp;&nbsp;&nbsp;&nbsp; D: Disminuida</div>");
            sb.Append("<table class='dt'>");
            sb.Append("<tr><th>CURVA</th><th>CERVICAL</th><th style='background:#339933;color:#fff;-webkit-print-color-adjust:exact;print-color-adjust:exact;'>DORSAL</th><th>LUMBAR</th></tr>");
            sb.Append("<tr><td style='background:#d9d9d9;font-weight:bold;-webkit-print-color-adjust:exact;print-color-adjust:exact;'>LORDOSIS</td><td style='text-align:center;'>"+CV(col?.LordosisCervical)+"</td><td style='text-align:center;'>"+CV(col?.LordosisDorsal)+"</td><td style='text-align:center;'>"+CV(col?.LordosisLumbar)+"</td></tr>");
            sb.Append("<tr><td style='background:#d9d9d9;font-weight:bold;-webkit-print-color-adjust:exact;print-color-adjust:exact;'>CIFOSIS</td><td style='text-align:center;'>"+CV(col?.CifosisCervical)+"</td><td style='text-align:center;'>"+CV(col?.CifosisDorsal)+"</td><td style='text-align:center;'>"+CV(col?.CifosisLumbar)+"</td></tr>");
            sb.Append("</table>");
            sb.Append("<table class='dt' style='margin-top:6px;'>");
            sb.Append("<tr><th>ESCOLIOSIS</th><th>DORSAL</th><th>LUMBAR</th><th>DOBLE</th></tr>");
            sb.Append("<tr><td style='font-weight:bold;'>DERECHA</td><td style='text-align:center;'>"+Chk(col!=null&&col.EscoliosisDorsalDerecha)+"</td><td style='text-align:center;'>"+Chk(col!=null&&col.EscoliosisLumbarDerecha)+"</td><td style='text-align:center;'>"+Chk(col!=null&&col.EscoliosisDobleDerecha)+"</td></tr>");
            sb.Append("<tr><td style='font-weight:bold;'>IZQUIERDA</td><td style='text-align:center;'>"+Chk(col!=null&&col.EscoliosisDorsalIzquierda)+"</td><td style='text-align:center;'>"+Chk(col!=null&&col.EscoliosisLumbarIzquierda)+"</td><td style='text-align:center;'>"+Chk(col!=null&&col.EscoliosisDobleIzquierda)+"</td></tr>");
            sb.Append("</table></div>");
            // DIAGNOSTICO
            sb.Append("<div style='margin-top:12px;'><table class='fm'>");
            sb.Append("<tr><td class='lb'>DIAGN&#211;STICO</td><td colspan='5' style='border-bottom:1px solid #000;'>"+E(Evaluacion?.Observaciones)+"</td></tr>");
            sb.Append("<tr><td></td><td colspan='5' style='border-bottom:1px solid #000;'>&nbsp;</td></tr>");
            sb.Append("</table></div>");
            // RESULTADO
            sb.Append("<div style='margin-top:8px;'><table class='fm'>");
            sb.Append("<tr><td class='lb'>RESULTADO:</td><td class='lb'>APTO:</td><td style='min-width:80px;border-bottom:1px solid #000;text-align:center;font-weight:bold;'>"+(Evaluacion?.FkAptitudMedica==1?"X":"")+"</td><td class='lb'>NO APTO:</td><td style='min-width:80px;border-bottom:1px solid #000;text-align:center;font-weight:bold;'>"+(Evaluacion?.FkAptitudMedica==3?"X":"")+"</td><td class='lb'>CON RESTRICCIONES:</td><td style='min-width:100px;border-bottom:1px solid #000;text-align:center;font-weight:bold;'>"+(Evaluacion?.FkAptitudMedica==2?"X":"")+"</td></tr>");
            sb.Append("</table></div>");
            // RECOMENDACIONES
            sb.Append("<div style='margin-top:8px;'><table class='fm'>");
            sb.Append("<tr><td class='lb'>RECOMENDACIONES:</td><td colspan='5' style='border-bottom:1px solid #000;'>"+E(Evaluacion?.Recomendaciones)+"</td></tr>");
            sb.Append("<tr><td></td><td colspan='5' style='border-bottom:1px solid #000;'>&nbsp;</td></tr>");
            sb.Append("<tr><td></td><td colspan='5' style='border-bottom:1px solid #000;'>&nbsp;</td></tr>");
            sb.Append("</table></div>");
            // REALIZO
            sb.Append("<div style='margin-top:10px;'><table class='fm'>");
            sb.Append("<tr><td class='lb'>REALIZ&#211;:</td><td style='border-bottom:1px solid #000;min-width:400px;'>&nbsp;</td></tr>");
            sb.Append("<tr><td></td><td style='text-align:center;font-weight:bold;font-size:7.5pt;'>NOMBRE Y FIRMA</td></tr>");
            sb.Append("</table></div>");
            // LEGAL
            sb.Append("<div style='font-size:6.5pt;text-align:justify;margin-top:20px;line-height:1.3;'>");
            sb.Append("Declaro que toda la informaci&#243;n suministrada es ver&#237;dica y que no he ocultado ning&#250;n dato sobre mis antecedentes y/o estado de salud y estoy consciente que cualquier omisi&#243;n o falsificaci&#243;n la empresa tendr&#225; la facultad de anular cualquier tramite relacionado conmigo y autorizo al servicio m&#233;dico (salud ocupacional) de la empresa para que realice los ex&#225;menes necesarios con motivo de mi trabajo y sean utilizados con fines estad&#237;sticos y epidemiol&#243;gicos. As&#237; mismo autorizo al medico referente para poner en conocimiento de la empresa todo lo referente a los resultados del examen f&#237;sico y de las pruebas auxiliares de diagn&#243;stico.");
            sb.Append("</div>");
            // FIRMA TRABAJADOR
            sb.Append("<div style='margin-top:15px;font-size:7.5pt;font-weight:bold;'>NOMBRE Y FIRMA DEL TRABAJADOR (A): <span style='display:inline-block;min-width:55%;border-bottom:1px solid #000;font-weight:bold;text-align:center;'>"+E(Paciente?.NombreCompleto)+"</span></div>");
            sb.Append("<div class='ftr'>GCI-FOR-SYM-45 REV. 0</div>");
            sb.Append("</div>"); // end page 2
            PaseHtml=sb.ToString();
        }
    }
}

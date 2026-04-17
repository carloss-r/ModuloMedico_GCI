    <%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EvaluacionMedica.aspx.cs" Inherits="Telerik.ServicioMedico.EvaluacionMedica" %>
<!DOCTYPE html>
<html lang="es">
<head>
<meta charset="utf-8" />
<title>Gesti&oacute;n Cl&iacute;nica</title>
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.2/css/bootstrap.min.css" />
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.2/css/all.min.css" />
<link rel="stylesheet" href="../RecursosHumanos/styles/DashboardRecursosHumanosSM.css" />
<link rel="stylesheet" href="/ServicioMedico/styles/DashboardServicioMedico.css" />

<script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.2/js/bootstrap.bundle.min.js"></script>
</head>
<body>
<form id="form1" runat="server" onkeydown="return event.keyCode != 13 || event.target.tagName == 'TEXTAREA';">


<div id="gci-modulo-medico-container">
    <script type="text/javascript">
        /* eslint-disable */
        // Variables inyectadas desde servidor - bloque completo deshabilitado para linting
        window.GCI_DATA = window.GCI_DATA || {};
        window.GCI_DATA.idOrden = <%= IdOrden %>;
        window.GCI_DATA.initialSexo = <%= Newtonsoft.Json.JsonConvert.SerializeObject(initialSexo ?? "") %>;
        window.GCI_DATA.currentTipoServicio = <%= currentTipoServicio %>;
        /* eslint-enable */
    </script>
    <script>
        // Variables locales para uso en el código
        var idOrden = window.GCI_DATA.idOrden;
        var initialSexo = window.GCI_DATA.initialSexo;
        var currentTipoServicio = window.GCI_DATA.currentTipoServicio;
    </script>

    <!-- ═══ Banner Identificación del Paciente ═══ -->
    <div class="paciente-banner" id="pacienteBanner">
        <div class="pb-avatar"><i class="fas fa-user-circle"></i></div>
        <div class="pb-info">
            <div class="pb-nombre" id="pbNombre">Cargando paciente...</div>
            <div class="pb-detalle">
                <span id="pbEmpresa">—</span>
                <span class="pb-sep">|</span>
                <span id="pbPuesto">—</span>
                <span class="pb-sep" id="pbNumEmpSep" style="display:none;">|</span>
                <span id="pbNumEmp" style="display:none;">No. Emp: —</span>
            </div>
        </div>
        <div class="pb-badges">
            <span class="pb-badge pb-badge-servicio" id="pbTipoServicio">—</span>
            <span class="pb-badge pb-badge-tipo" id="pbTipo">—</span>
        </div>
    </div>

    <div class="wizard-container" id="mainWizard">
        <div class="wizard-steps">
            <!-- Paso 0: EXPEDIENTE (solo para Empleados) -->
            <div class="wizard-step" onclick="goToStep(0)" id="step0" style="display:none;">
                <i class="fas fa-folder-open"></i> Expediente
            </div>
            <div class="wizard-step active" onclick="goToStep(1)" id="step1">
                <i class="fas fa-user-circle"></i> Datos Generales
            </div>
            <div class="wizard-step" onclick="goToStep(2)" id="step2">
                <i class="fas fa-history"></i> Antecedentes
            </div>
            <div class="wizard-step" onclick="goToStep(3)" id="step3">
                <i class="fas fa-running"></i> H&aacute;bitos
            </div>
            <div class="wizard-step" onclick="goToStep(4)" id="step4">
                <i class="fas fa-stethoscope"></i> Exploraci&oacute;n F&iacute;sica
            </div>
            <div class="wizard-step" onclick="goToStep(5)" id="step5">
                <i class="fas fa-venus-mars"></i> Gineco / Genitourin.
            </div>
            <div class="wizard-step" onclick="goToStep(6)" id="step6">
                <i class="fas fa-clipboard-check"></i> Diagn&oacute;stico
            </div>
        </div>

        <div class="wizard-content">

            <!-- PASO 0 — EXPEDIENTE CLÍNICO (Empleados) -->
            <div class="step-panel" id="panel0" style="display:none;">
                <div style="display:flex; align-items:center; justify-content:space-between; margin-bottom:20px;">
                    <h3 class="section-title" style="margin:0;"><i class="fas fa-folder-open" style="color:#1a5276;"></i> Expediente Clínico del Empleado</h3>
                    <span class="badge-expediente" id="badgeTotalEvals"></span>
                </div>

                <div id="expedienteContainer">
                    <div class="exp-loading"><i class="fas fa-spinner fa-spin"></i> Cargando historial médico...</div>
                </div>
            </div>

            <!-- PASO 1 — DATOS GENERALES -->
<div class="step-panel active" id="panel1">
    <h3 class="section-title">Datos Generales del Paciente</h3>

    <!-- Lugar Evaluación + Fecha | Cargo | No. IMSS -->
    <div class="paper-row">
        <div class="form-group flex-2">
            <label>Lugar y fecha del Examen:</label>
            <input type="text" id="txtLugarEvaluacion" class="form-control" placeholder="Ej. Monterrey, N.L. / Obra Torre Centro" />
        </div>
        <div class="form-group flex-1">
            <label>Fecha de Captura</label>
            <input type="date" id="txtFechaExamen" class="form-control" value="<%= DateTime.Now.ToString("yyyy-MM-dd") %>" />
        </div>
        <div class="form-group flex-1">
            <label>Cargo</label>
            <input type="text" id="txtPuesto" class="form-control" readonly />
        </div>
        <div class="form-group flex-2">
            <label>No. IMSS</label>
            <input type="text" id="txtNss" class="form-control val-num" maxlength="11" placeholder="11 dígitos" />
        </div>
    </div>

    <!-- Nombre | Fecha Nac + Edad | Lugar Nac -->
    <div class="paper-row">
        <div class="form-group flex-1">
            <label>Nombre(s)</label>
            <input type="text" id="txtNombre" class="form-control" readonly />
        </div>
        <div class="form-group flex-1">
            <label>Apellido Paterno</label>
            <input type="text" id="txtApellidoPaterno" class="form-control" readonly />
        </div>
        <div class="form-group flex-1">
            <label>Apellido Materno</label>
            <input type="text" id="txtApellidoMaterno" class="form-control" readonly />
        </div>
    </div>

    <div class="paper-row">
        <div class="form-group flex-1">
            <label>Estado de Nacimiento</label>
            <select id="ddlEstadoNacimiento" class="form-control">
                <option value="">-- Seleccione --</option>
            </select>
        </div>
        <div class="form-group flex-1">
            <label>Fecha de Nacimiento</label>
            <input type="date" id="txtFechaNacimiento" class="form-control" onchange="calcularEdad()" />
        </div>
        <div class="form-group flex-05">
            <label>Edad</label>
            <input type="text" id="txtEdad" class="form-control" readonly style="text-align: center;" />
        </div>
    </div>

    <!-- Row 3: Estado Civil | Mano Dominante | Teléfono | -->
    <div class="paper-row">
        <div class="form-group flex-1">
            <label>Examen de</label>
            <input type="text" id="txtExamenDe" class="form-control" readonly value="<%= (currentTipoServicio == 2 ? "Periódico" : "Ingreso") %>" />
        </div>
        <div class="form-group flex-1">
            <label>Estado Civil</label>
            <select id="ddlEstadoCivil" class="form-control">
                <option value="">-- Seleccione --</option>
            </select>
        </div>
        <div class="form-group flex-1">
            <label>Mano Dominante</label>
            <select id="ddlManoDominante" class="form-control">
                <option value="">-- Seleccione --</option>
                <option value="Diestro">Diestro(a)</option>
                <option value="Zurdo">Zurdo(a)</option>
            </select>
        </div>
        <div class="form-group flex-2">
            <label>Tel&eacute;fono</label>
            <input type="tel" id="txtTelefono" class="form-control val-num" placeholder="10 d&iacute;gitos" maxlength="15" />
        </div>
    </div>

    <!-- Row 4: Domicilio (Catálogos) -->
    <h3 class="section-title">Domiciolio Personal</h3>
    <div id="divGeoError" style="display:none; color:red; background:#fff0f0; border:1px solid red; padding:8px; margin-bottom:8px; border-radius:4px; font-size:0.85rem;"></div>
    <div class="paper-row">
        <div class="form-group flex-1">
            <label>Pa&iacute;s</label>
            <select id="ddlPais" class="form-control">
                <option value="">-- Seleccione --</option>
            </select>
        </div>
        <div class="form-group flex-1">
            <label>Estado</label>
            <select id="ddlEstado" class="form-control">
                <option value="">-- Seleccione --</option>
            </select>
        </div>
        <div class="form-group flex-1">
            <label>Municipio</label>
            <select id="ddlMunicipio" class="form-control">
                <option value="">-- Seleccione --</option>
            </select>
        </div>
        <div class="form-group flex-1">
            <label>Colonia / Localidad</label>
            <select id="ddlColonia" class="form-control">
                <option value="">-- Seleccione --</option>
            </select>
        </div>
    </div>

    <!-- Row 4.1: Calle y Número -->
    <div class="paper-row">
        <div class="form-group flex-2">
            <label>Calle</label>
            <input type="text" id="txtCalle" class="form-control" readonly />
        </div>
        <div class="form-group flex-1">
            <label>No. Ext.</label>
            <input type="text" id="txtNumExt" class="form-control" readonly />
        </div>
        <div class="form-group flex-1">
            <label>No. Int.</label>
            <input type="text" id="txtNumInt" class="form-control" readonly />
        </div>
        <div class="form-group flex-1">
            <label>C.P.</label>
            <input type="text" id="txtCp" class="form-control val-num" readonly />
            <input type="hidden" id="hdnFkCp" />
        </div>
    </div>

    <!-- Domicilio Completo Oculto para fallback -->
    <input type="hidden" id="txtDomicilio" />

    <!-- Row 4.2: Nivel Académico | Profesión -->
    <div class="paper-row">
        <div class="form-group flex-1">
            <label>Nivel Acad&eacute;mico</label>
            <select id="ddlEscolaridad" class="form-control">
                <option value="">-- Seleccione --</option>
            </select>
        </div>
        <div class="form-group flex-2">
            <label>Profesi&oacute;n y/u Oficio</label>
            <input type="text" id="txtProfesion" class="form-control val-text" />
        </div>
    </div>

    <!-- Row 5: Sexo | Tipo Sangre -->
    <div class="paper-row">
        <div class="form-group flex-1">
            <label>Sexo biológico</label>
            <select id="ddlSexo" class="form-control" onchange="window.setSexoDisplay(this.value)">
                <option value="">-- Seleccione --</option>
                <option value="M">Masculino (Masc)</option>
                <option value="F">Femenino (Fem)</option>
            </select>
        </div>
        <div class="form-group flex-1">
            <label>Tipo de Sangre</label>
            <select id="ddlTipoSangre" class="form-control">
                <option value="">-- Seleccione --</option>
            </select>
        </div>
    </div>

    <!-- Empresa (oculta pero necesaria para guardar) -->
    <div class="form-group" style="display:none;">
        <input type="text" id="txtEmpresa" class="form-control" readonly />
    </div>
</div>


            <!-- PASO 2 — ANTECEDENTES -->

<div class="step-panel" id="panel2">
    <h3 class="section-title">Antecedentes Heredo Familiares</h3>
    <table class="exam-grid" style="margin-bottom:15px; width: 100%;">
        <thead><tr><th style="width:30%;">Condici&oacute;n</th><th style="text-align:center; width:15%;">Si / No</th><th>Detalles</th></tr></thead>
        <tbody id="tbAntecedentesHF"><!-- JS generated --></tbody>
    </table>

    <h3 class="section-title">Antecedentes Personales Patol&oacute;gicos</h3>
    <table class="exam-grid" style="margin-bottom:15px; width: 100%;">
        <thead><tr><th style="width:30%;">Condici&oacute;n</th><th style="text-align:center; width:15%;">Si / No</th><th>Detalles</th></tr></thead>
        <tbody id="tbAntecedentesPP"><!-- JS generated --></tbody>
    </table>

    <div class="paper-row">
        <div class="form-group flex-1">
            <label>Observaciones (Antecedentes Patol&oacute;gicos)</label>
            <textarea id="txtAlergias" class="form-control" rows="5" placeholder="Observaciones generales, alergias, otros..."></textarea>
        </div>
    </div>

    <!-- Antecedentes Laborales -->
    <div id="secLaborales" style="margin-top:15px;">
        <h3 class="section-title">Antecedentes de Empresa</h3>
        <table class="exam-grid" style="margin-bottom:10px;">
            <thead>
                <tr>
                    <th>Empresa</th>
                    <th>Puesto</th>
                    <th>Tiempo</th>
                    <th>Agentes Expuestos</th>
                    <th>Accidentes</th>
                    <th style="width:40px;"></th>
                </tr>
            </thead>
            <tbody id="tbAntecedentesLaborales"><!-- JS generated --></tbody>
        </table>
        <button type="button" class="btn-primary" style="padding:5px 10px; font-size:0.8rem;" onclick="addLaboralRow(); return false;">+ A&ntilde;adir Fila</button>
    </div>
</div>


            <!-- PASO 3 — HÁBITOS + VACUNAS -->
           
<div class="step-panel" id="panel3">
    <h3 class="section-title">H&aacute;bitos</h3>
    
    <!-- Row 1: Tabaquismo -->
    <div class="paper-row" style="align-items: center;">
        <div class="form-group flex-1">
            <label class="check-item">
                <input type="checkbox" id="chkFuma" class="toggle-habito" data-target="#divFuma" />
                Fuma
            </label>
        </div>
        <div class="form-group flex-3" id="divFuma" style="display:none;">
            <div style="display:flex; gap:15px; width:100%;">
                <div class="form-group flex-1">
                    <label>A&ntilde;os de h&aacute;bito</label>
                    <input type="number" id="txtAnosFuma" class="form-control val-num" />
                </div>
                <div class="form-group flex-1">
                    <label>No. de Cigarros/d&iacute;a</label>
                    <input type="number" id="txtCigarrillos" class="form-control val-num" />
                </div>
                <div class="form-group flex-05" style="justify-content: flex-end; padding-bottom: 8px;">
                    <label class="check-item"><input type="checkbox" id="chkExFumador" /> EX</label>
                </div>
            </div>
        </div>
    </div>

    <!-- Row 2: Drogas -->
    <div class="paper-row" style="align-items: center;">
        <div class="form-group flex-1">
            <label class="check-item">
                <input type="checkbox" id="chkDrogas" class="toggle-habito" data-target="#divDrogas" />
                Drogas
            </label>
        </div>
        <div class="form-group flex-3" id="divDrogas" style="display:none;">
            <label>Tipo de droga</label>
            <input type="text" id="txtTipoDrogas" class="form-control val-text" />
        </div>
    </div>

    <!-- Row 3: Alcohol | Deporte -->
    <div class="paper-row" style="align-items: center;">
        <div class="form-group flex-1">
            <label class="check-item">
                <input type="checkbox" id="chkAlcohol" class="toggle-habito" data-target="#divAlcohol" />
                Alcohol
            </label>
        </div>
        <div class="form-group flex-1" id="divAlcohol" style="display:none;">
            <label>Frecuencia</label>
            <select id="txtFrecAlcohol" class="form-control">
                <option value="">-- Seleccione --</option>
                <option value="Ocasional">Ocasional</option>
                <option value="Social">Social</option>
                <option value="Semanal">Semanal</option>
                <option value="Frecuente">Frecuente</option>
                <option value="Diario">Diario</option>
            </select>
        </div>
        <div class="form-group flex-1" style="margin-left:20px;">
            <label class="check-item">
                <input type="checkbox" id="chkDeporte" class="toggle-habito" data-target="#divDeporte" />
                Deporte
            </label>
        </div>
        <div class="form-group flex-2" id="divDeporte" style="display:none;">
            <label>Tipo de deporte</label>
            <input type="text" id="txtTipoDeporte" class="form-control" placeholder="¿Qu&eacute; deporte practica?" />
        </div>
    </div>

    <!-- Tiempo Libre -->
    <div class="paper-row">
        <div class="form-group flex-1">
            <label>Tiempo Libre</label>
            <textarea id="txtTiempoLibre" class="form-control" rows="2"></textarea>
        </div>
    </div>

    <h3 class="section-title" style="margin-top:10px;">Vacunaci&oacute;n</h3>
    <div class="paper-row">
        <table class="exam-grid" style="width: 100%;">
            <thead>
                <tr>
                    <th>Inmunizaci&oacute;n</th>
                    <th style="text-align:center;">Dosis 1</th>
                    <th style="text-align:center;">Dosis 2</th>
                    <th style="text-align:center;">Dosis 3</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td>T&eacute;tanos</td>
                    <td style="text-align:center;"><input type="checkbox" id="chkTetanos1" /></td>
                    <td style="text-align:center;"><input type="checkbox" id="chkTetanos2" /></td>
                    <td style="text-align:center;"><input type="checkbox" id="chkTetanos3" /></td>
                </tr>
                <tr>
                    <td>Hepatitis</td>
                    <td style="text-align:center;"><input type="checkbox" id="chkHepatitis1" /></td>
                    <td style="text-align:center;"><input type="checkbox" id="chkHepatitis2" /></td>
                    <td style="text-align:center;">---</td>
                </tr>
                <tr>
                    <td>Influenza (H1N1)</td>
                    <td style="text-align:center;"><input type="checkbox" id="chkH1N1" /></td>
                    <td style="text-align:center;">---</td>
                    <td style="text-align:center;">---</td>
                </tr>
            </tbody>
        </table>
    </div>
    <div class="paper-row">
        <div class="form-group flex-1">
            <label>Observaciones de Vacunaci&oacute;n</label>
            <textarea id="txtObsVacunas" class="form-control" rows="2"></textarea>
        </div>
    </div>
</div>


            <!-- PASO 4 — EXPLORACIÓN FÍSICA -->
            <!-- Paso 4: Exploraci&oacute;n F&iacute;sica -->
<div class="step-panel" id="panel4">
    <h3 class="section-title">EXPLORACIÓN FISICA</h3>

    <!-- Comparativa de Signos Vitales (Historial) -->
    <div id="comparativaSignosContainer" style="display:none; margin-bottom:15px;">
        <div style="background: #f8f9fa; border: 1px dashed #cbd5e0; border-radius: 8px; padding: 10px;">
            <div style="font-size: 0.75rem; font-weight: 700; color: #4a5568; text-transform: uppercase; margin-bottom: 5px; display: flex; align-items: center;">
                <i class="fas fa-history" style="margin-right: 5px; color: #3182ce;"></i> Expediente Cl&iacute;nico de Seguimiento
            </div>
            <div id="comparativaSignosTable" style="overflow-x: auto;">
                <!-- JS populated -->
            </div>
        </div>
    </div>

    <!-- Row 1: TA Sistólica + Diastólica | FC | FR | Peso | Estatura -->
    <div class="paper-row">
        <div class="form-group flex-1">
            <label>TA Sist. (mmHg)</label>
            <input type="number" id="txtSistolica" class="form-control val-num" placeholder="120" />
        </div>
        <div class="form-group flex-1">
            <label>TA Diast. (mmHg)</label>
            <input type="number" id="txtDiastolica" class="form-control val-num" placeholder="80" />
        </div>
        <div class="form-group flex-1">
            <label>FC (x min)</label>
            <input type="number" id="txtFrecCardiaca" class="form-control val-num" placeholder="72" />
        </div>
        <div class="form-group flex-1">
            <label>FR (x min)</label>
            <input type="number" id="txtFrecRespiratoria" class="form-control val-num" placeholder="16" />
        </div>
        <div class="form-group flex-1">
            <label>Peso (kgs)</label>
            <input type="text" id="txtPeso" class="form-control val-dec" onchange="calcImc()" />
        </div>
        <div class="form-group flex-1">
            <label>Estatura (m)</label>
            <input type="text" id="txtEstatura" class="form-control val-dec" onchange="calcImc()" />
        </div>
    </div>

    <!-- Row 2: IMC | Tipo Obesidad | Temp -->
    <div class="paper-row">
        <div class="form-group flex-1">
            <label>IMC</label>
            <input type="text" id="txtImc" class="form-control" readonly />
        </div>
        <div class="form-group flex-2">
            <label>Clasificaci&oacute;n IMC</label>
            <input type="text" id="txtImcDescripcion" class="form-control" readonly style="background-color: #f9f9f9; font-weight: bold; color: #2c3e50;" />
        </div>
        <div class="form-group flex-1">
            <label>Temp (&deg;C)</label>
            <input type="text" id="txtTemperatura" class="form-control val-dec" placeholder="36.5" />
        </div>
    </div>

    <!-- Row 3: Glucosa | Oximetría -->
    <div class="paper-row">
        <div class="form-group flex-1">
            <label>Glucosa (GLC)</label>
            <div style="margin-bottom:8px;">
                <label style="font-weight:normal; margin-right:15px;">
                    <input type="checkbox" id="chkGlucosaAplica" onchange="toggleGlucosaField()" />
                    ¿Aplica glucosa?
                </label>
            </div>
            <div id="glucosaFieldContainer" style="display:none;">
                <div style="display:flex; align-items:center;">
                    <input type="text" id="txtGlucosa" class="form-control val-dec" placeholder="90" disabled />
                    <span style="margin-left:8px; font-size:0.85rem; color:#777;">mg/dl</span>
                </div>
            </div>
        </div>
        <div class="form-group flex-1">
            <label>Oximetr&iacute;a (O2)</label>
            <div style="display:flex; align-items:center;">
                <input type="text" id="txtOximetria" class="form-control val-num" placeholder="98" maxlength="3" />
                <span style="margin-left:8px; font-size:0.85rem; color:#777;">% SpO2</span>
            </div>
        </div>
        <div class="form-group flex-2">
            <!-- Espacio para mantener alineación -->
        </div>
    </div>

    <!-- Síntomas -->
    <div class="paper-row">
        <div class="form-group flex-1">
            <label>S&iacute;ntomas</label>
            <textarea id="txtSintomas" class="form-control" rows="4" placeholder="S&iacute;ntomas referidos por el paciente..."></textarea>
        </div>
    </div>
    <div class="paper-row">
        <div class="form-group flex-1">
            <label>Aparatos y Sistemas (observaci&oacute;n general)</label>
            <textarea id="txtAparatosSistemas" class="form-control" rows="4" placeholder="Observaciones generales de aparatos y sistemas..."></textarea>
        </div>
    </div>

    <!-- Escala de Agudeza Visual Snellen -->
    <h3 class="section-title" style="margin-top:20px;">Agudeza Visual (Escala de Snellen)</h3>
    <div class="paper-row">
        <div class="form-group flex-1">
            <label>Ojo Derecho (OD) Sin Lentes</label>
            <select id="ddlOdSinLentes" class="form-control">
                <option value="">-- Seleccione --</option>
                <option value="20/20">20/20</option>
                <option value="20/25">20/25</option>
                <option value="20/30">20/30</option>
                <option value="20/40">20/40</option>
                <option value="20/50">20/50</option>
                <option value="20/70">20/70</option>
                <option value="20/100">20/100</option>
                <option value="20/200">20/200</option>
            </select>
        </div>
        <div class="form-group flex-1">
            <label>Ojo Izquierdo (OI) Sin Lentes</label>
            <select id="ddlOiSinLentes" class="form-control">
                <option value="">-- Seleccione --</option>
                <option value="20/20">20/20</option>
                <option value="20/25">20/25</option>
                <option value="20/30">20/30</option>
                <option value="20/40">20/40</option>
                <option value="20/50">20/50</option>
                <option value="20/70">20/70</option>
                <option value="20/100">20/100</option>
                <option value="20/200">20/200</option>
            </select>
        </div>
        <div class="form-group flex-1">
            <label>Ambos Ojos (AO) Sin Lentes</label>
            <select id="ddlAoSinLentes" class="form-control">
                <option value="">-- Seleccione --</option>
                <option value="20/20">20/20</option>
                <option value="20/25">20/25</option>
                <option value="20/30">20/30</option>
                <option value="20/40">20/40</option>
                <option value="20/50">20/50</option>
                <option value="20/70">20/70</option>
                <option value="20/100">20/100</option>
                <option value="20/200">20/200</option>
            </select>
        </div>
    </div>
    <div class="paper-row">
        <div class="form-group flex-1">
            <label>OD Con Lentes</label>
            <select id="ddlOdConLentes" class="form-control">
                <option value="">-- N/A --</option>
                <option value="20/20">20/20</option>
                <option value="20/25">20/25</option>
                <option value="20/30">20/30</option>
                <option value="20/40">20/40</option>
                <option value="20/50">20/50</option>
                <option value="20/70">20/70</option>
                <option value="20/100">20/100</option>
                <option value="20/200">20/200</option>
            </select>
        </div>
        <div class="form-group flex-1">
            <label>OI Con Lentes</label>
            <select id="ddlOiConLentes" class="form-control">
                <option value="">-- N/A --</option>
                <option value="20/20">20/20</option>
                <option value="20/25">20/25</option>
                <option value="20/30">20/30</option>
                <option value="20/40">20/40</option>
                <option value="20/50">20/50</option>
                <option value="20/70">20/70</option>
                <option value="20/100">20/100</option>
                <option value="20/200">20/200</option>
            </select>
        </div>
        <div class="form-group flex-1">
            <label>AO Con Lentes</label>
            <select id="ddlAoConLentes" class="form-control">
                <option value="">-- N/A --</option>
                <option value="20/20">20/20</option>
                <option value="20/25">20/25</option>
                <option value="20/30">20/30</option>
                <option value="20/40">20/40</option>
                <option value="20/50">20/50</option>
                <option value="20/70">20/70</option>
                <option value="20/100">20/100</option>
                <option value="20/200">20/200</option>
            </select>
        </div>
    </div>
    <div class="paper-row">
        <div class="form-group flex-1">
            <label>Usa Lentes</label>
            <select id="ddlUsaLentes" class="form-control">
                <option value="No">No</option>
                <option value="Si">S&iacute;</option>
            </select>
        </div>
        <div class="form-group flex-1">
            <label>Referencia Visual</label>
            <select id="ddlReferenciaVisual" class="form-control">
                <option value="Normal">Normal</option>
                <option value="Ceguera">Ceguera al color</option>
                <option value="Astigmatismo">Miop&iacute;a / Astigmatismo</option>
            </select>
        </div>
        <div class="form-group flex-1">
            <label>Test de Ishihara (Daltonismo)</label>
            <select id="ddlDaltonismo" class="form-control">
                <option value="Normal">Normal</option>
                <option value="Anormal">Anormal</option>
                <option value="No Realizado">No Realizado</option>
            </select>
        </div>
    </div>

    <h3 class="section-title" style="margin-top:10px;">Exploraci&oacute;n F&iacute;sica por Sistemas</h3>
    <table class="exam-grid">
        <thead>
            <tr>
                <th style="width:30%;">Sistema</th>
                <th style="width:10%; text-align:center;">Normal</th>
                <th style="width:10%; text-align:center;">Anormal</th>
                <th>Descripci&oacute;n de Hallazgos</th>
            </tr>
        </thead>
        <tbody id="tbExamenFisico"><!-- JS generated --></tbody>
    </table>
</div>


            <!-- PASO 5 — GINECO-OBSTÉTRICOS / GENITOURINARIO MASCULINO -->
            <!-- Paso 5: GINECO-OBSTÉTRICOS / GENITOURINARIO MASCULINO -->
<div class="step-panel" id="panel5">
    <!-- Sección Femenina -->
    <div id="secGineco" style="display:none;">
        <h3 class="section-title">Gineco-Obst&eacute;tricos</h3>

        <div class="paper-row">
            <div class="form-group flex-1"><label>Menarca (Edad)</label><input type="number" id="txtMenarca" class="form-control val-num" /></div>
            <div class="form-group flex-1">
                <label>Ciclos</label>
                <select id="txtCiclos" class="form-control">
                    <option value="">-- Seleccione --</option>
                    <option value="Regular">Regular (21-35 d&iacute;as)</option>
                    <option value="Irregular">Irregular</option>
                    <option value="Oligomenorrea">Oligomenorrea (+35 d&iacute;as)</option>
                    <option value="Polimenorrea">Polimenorrea (-21 d&iacute;as)</option>
                    <option value="Amenorrea">Amenorrea</option>
                </select>
            </div>
            <div class="form-group flex-1"><label>FUM</label><input type="date" id="txtFum" class="form-control" /></div>
        </div>

        <!-- Row 2: Planificación | Cit. Vag. -->
        <div class="paper-row">
            <div class="form-group flex-2">
                <label>Planificaci&oacute;n</label>
                <select id="txtPlanificacion" class="form-control">
                    <option value="">-- Seleccione --</option>
                    <option value="Ninguno">Ninguno</option>
                    <option value="Hormonal oral">Hormonal oral</option>
                    <option value="Inyectable">Inyectable</option>
                    <option value="DIU">DIU</option>
                    <option value="Implante">Implante subd&eacute;rmico</option>
                    <option value="Cond&oacute;n">Cond&oacute;n</option>
                    <option value="Ligadura">Ligadura de trompas</option>
                    <option value="Vasectom&iacute;a">Vasectom&iacute;a</option>
                    <option value="Ritmo">M&eacute;todo del ritmo</option>
                    <option value="Otro">Otro</option>
                </select>
            </div>
            <div class="form-group flex-1"><label>Cit. Vag.</label><input type="date" id="txtPap" class="form-control" /></div>
        </div>

        <!-- Row 3: Gestas | Partos | Abortos | Cesáreas -->
        <div class="paper-row">
            <div class="form-group flex-1"><label>Gestas</label><input type="number" id="txtGestas" class="form-control val-num" /></div>
            <div class="form-group flex-1"><label>P (Partos)</label><input type="number" id="txtPartos" class="form-control val-num" /></div>
            <div class="form-group flex-1"><label>A (Abortos)</label><input type="number" id="txtAbortos" class="form-control val-num" /></div>
            <div class="form-group flex-1"><label>C (Ces&aacute;reas)</label><input type="number" id="txtCesareas" class="form-control val-num" /></div>
        </div>

        <div id="formFem"></div>
    </div>

    <!-- Sección Masculina -->
    <div id="secGenito" style="display:none;">
        <h3 class="section-title">Antecedentes de Aparato Genitourinario Masculino</h3>

        <div class="paper-row">
            <div class="form-group flex-1">
                <label>Examen Cl&iacute;nico</label>
                <div class="check-grid" style="display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 12px;">
                    <label class="check-item">
                        <input type="checkbox" id="chkPrepucio" />
                        Prepucio retr&aacute;ctil
                    </label>
                    <label class="check-item">
                        <input type="checkbox" id="chkHidrocele" />
                        Hidrocele
                    </label>
                    <label class="check-item">
                        <input type="checkbox" id="chkTesticulos" />
                        Test&iacute;culos
                    </label>
                    <label class="check-item">
                        <input type="checkbox" id="chkHernia" />
                        Hernia
                    </label>
                    <label class="check-item">
                        <input type="checkbox" id="chkFimosis" />
                        Fimosis
                    </label>
                    <label class="check-item">
                        <input type="checkbox" id="chkCriptorquidia" />
                        Criptorquidia
                    </label>
                    <label class="check-item">
                        <input type="checkbox" id="chkVaricocele" />
                        Varicocele
                    </label>
                </div>
            </div>
        </div>

        <div class="paper-row">
            <div class="form-group flex-1"><label>PSA (Ant&iacute;geno Prost&aacute;tico)</label><input type="text" id="txtPsa" class="form-control" /></div>
            <div class="form-group flex-1"><label>MPF (Planificaci&oacute;n)</label><input type="text" id="txtMpf" class="form-control" /></div>
        </div>

        <div id="formMasc"></div>
    </div>

    <p id="msgSexoPendiente" style="color:#999; font-style:italic; margin-top:20px; display:none;">
        <i class="fas fa-info-circle"></i> El sexo del paciente se determinar&aacute; autom&aacute;ticamente al cargar los datos.
    </p>
</div>


            <!-- PASO 6 — COLUMNA VERTEBRAL + DIAGNÓSTICO -->
            <!-- Paso 6: Columna Vertebral + Diagnóstico -->
<div class="step-panel" id="panel6">
    <!-- Historial de Columna y Sistemas (Se movió aquí por utilidad clínica) -->
    <div id="comparativaColumnaContainer" style="display:none; margin-bottom:15px;">
        <div style="background: #fdfdfd; border: 1px dashed #cbd5e0; border-radius: 8px; padding: 10px;">
            <div style="font-size: 0.72rem; font-weight: 700; color: #4a5568; text-transform: uppercase; margin-bottom: 5px; display: flex; align-items: center;">
                <i class="fas fa-history" style="margin-right: 5px; color: #e53e3e;"></i> Antecedentes de Columna y Sistemas
            </div>
            <div id="comparativaColumnaTable" style="overflow-x: auto;">
                <!-- JS populated -->
            </div>
        </div>
    </div>

    <h3 class="section-title">Columna Vertebral</h3>
    <table style="width:100%; border-collapse:collapse; margin-bottom:15px; font-size:0.9rem;">
        <thead>
            <tr style="background:#f0f4f8;">
                <th style="border:1px solid #ddd; padding:8px;">Curva</th>
                <th style="border:1px solid #ddd; padding:8px; text-align:center;">Cervical</th>
                <th style="border:1px solid #ddd; padding:8px; text-align:center;">Dorsal</th>
                <th style="border:1px solid #ddd; padding:8px; text-align:center;">Lumbar</th>
            </tr>
            <tr style="background:#e8eef4; font-size:0.78rem;">
                <th style="border:1px solid #ddd; padding:4px 8px;"></th>
                <th style="border:1px solid #ddd; padding:4px; text-align:center;">N: Normal &nbsp; A: Aumentada &nbsp; D: Disminuida</th>
                <th style="border:1px solid #ddd; padding:4px; text-align:center;">N / A / D</th>
                <th style="border:1px solid #ddd; padding:4px; text-align:center;">N / A / D</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td style="border:1px solid #ddd; padding:8px; font-weight:600;">LORDOSIS</td>
                <td style="border:1px solid #ddd; padding:6px;">
                    <select id="ddlLordosisCervical" class="form-control" style="padding:5px;">
                        <option value="0">Sin dato</option>
                        <option value="1">Normal</option>
                        <option value="2">Aumentada</option>
                        <option value="3">Disminuida</option>
                    </select>
                </td>
                <td style="border:1px solid #ddd; padding:6px;">
                    <select id="ddlLordosisDorsal" class="form-control" style="padding:5px;">
                        <option value="0">Sin dato</option>
                        <option value="1">Normal</option>
                        <option value="2">Aumentada</option>
                        <option value="3">Disminuida</option>
                    </select>
                </td>
                <td style="border:1px solid #ddd; padding:6px;">
                    <select id="ddlLordosisLumbar" class="form-control" style="padding:5px;">
                        <option value="0">Sin dato</option>
                        <option value="1">Normal</option>
                        <option value="2">Aumentada</option>
                        <option value="3">Disminuida</option>
                    </select>
                </td>
            </tr>
            <tr>
                <td style="border:1px solid #ddd; padding:8px; font-weight:600;">CIFOSIS</td>
                <td style="border:1px solid #ddd; padding:6px;">
                    <select id="ddlCifosisCervical" class="form-control" style="padding:5px;">
                        <option value="0">Sin dato</option>
                        <option value="1">Normal</option>
                        <option value="2">Aumentada</option>
                        <option value="3">Disminuida</option>
                    </select>
                </td>
                <td style="border:1px solid #ddd; padding:6px;">
                    <select id="ddlCifosisDorsal" class="form-control" style="padding:5px;">
                        <option value="0">Sin dato</option>
                        <option value="1">Normal</option>
                        <option value="2">Aumentada</option>
                        <option value="3">Disminuida</option>
                    </select>
                </td>
                <td style="border:1px solid #ddd; padding:6px;">
                    <select id="ddlCifosisLumbar" class="form-control" style="padding:5px;">
                        <option value="0">Sin dato</option>
                        <option value="1">Normal</option>
                        <option value="2">Aumentada</option>
                        <option value="3">Disminuida</option>
                    </select>
                </td>
            </tr>
        </tbody>
    </table>

    <table style="width:100%; border-collapse:collapse; margin-bottom:15px; font-size:0.9rem;">
        <thead>
            <tr style="background:#f0f4f8;">
                <th style="border:1px solid #ddd; padding:8px;">Escoliosis</th>
                <th style="border:1px solid #ddd; padding:8px; text-align:center;">Dorsal</th>
                <th style="border:1px solid #ddd; padding:8px; text-align:center;">Lumbar</th>
                <th style="border:1px solid #ddd; padding:8px; text-align:center;">Doble</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td style="border:1px solid #ddd; padding:8px; font-weight:600;">Derecha</td>
                <td style="border:1px solid #ddd; padding:8px; text-align:center;"><input type="checkbox" id="chkEscDorsalDer" /></td>
                <td style="border:1px solid #ddd; padding:8px; text-align:center;"><input type="checkbox" id="chkEscLumbarDer" /></td>
                <td style="border:1px solid #ddd; padding:8px; text-align:center;"><input type="checkbox" id="chkEscDoboDer" /></td>
            </tr>
            <tr>
                <td style="border:1px solid #ddd; padding:8px; font-weight:600;">Izquierda</td>
                <td style="border:1px solid #ddd; padding:8px; text-align:center;"><input type="checkbox" id="chkEscDorsalIzq" /></td>
                <td style="border:1px solid #ddd; padding:8px; text-align:center;"><input type="checkbox" id="chkEscLumbarIzq" /></td>
                <td style="border:1px solid #ddd; padding:8px; text-align:center;"><input type="checkbox" id="chkEscDoboIzq" /></td>
            </tr>
        </tbody>
    </table>
    <div class="paper-row">
        <div class="form-group flex-1" style="margin-bottom:20px;">
            <label>Observaciones Columna</label>
            <textarea id="txtObsColumna" class="form-control" rows="2"></textarea>
        </div>
    </div>

    <h3 class="section-title">Diagn&oacute;stico</h3>

    <!-- Resultado arriba, destacado -->
    <div style="margin-bottom: 20px;">
        <label style="display:block; font-size:0.82rem; color:#34495e; font-weight:700; text-transform:uppercase; letter-spacing:0.3px; margin-bottom:6px;">RESULTADO / APTITUD</label>
        <select id="ddlAptitud" class="form-control" style="max-width: 320px;">
            <option value="">-- Seleccione --</option>
            <option value="1">APTO</option>
            <option value="2">CON RESTRICCIONES</option>
            <option value="3">NO APTO</option>
            <option value="4">Pendiente de Evaluaci&oacute;n</option>
        </select>
    </div>

    <!-- Diagnóstico y Recomendaciones lado a lado -->
    <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px; width: 100%;">
        <div>
            <label style="display:block; font-size:0.82rem; color:#34495e; font-weight:700; text-transform:uppercase; letter-spacing:0.3px; margin-bottom:6px;">DIAGN&Oacute;STICO M&Eacute;DICO</label>
            <textarea id="txtDiagnostico" class="form-control" rows="4" placeholder="Escriba el diagnóstico clínico detallado..." style="width:100%; resize:vertical; min-height:90px;"></textarea>
        </div>
        <div>
            <label style="display:block; font-size:0.82rem; color:#34495e; font-weight:700; text-transform:uppercase; letter-spacing:0.3px; margin-bottom:6px;">RECOMENDACIONES AL TRABAJADOR</label>
            <textarea id="txtRecomendaciones" class="form-control" rows="4" placeholder="Indique las recomendaciones preventivas o de seguimiento..." style="width:100%; resize:vertical; min-height:90px;"></textarea>
        </div>
    </div>


<script>
    document.addEventListener('DOMContentLoaded', function () {
        var ids = ['ddlLordosisDorsal', 'ddlCifosisCervical', 'ddlCifosisLumbar'];
        ids.forEach(function (id) {
            var el = document.getElementById(id);
            if (!el) return;
            el.style.background = '#f5f5f5';
            el.disabled = true;
            el.value = '0';
            el.title = 'Esta curva no aplica para evaluación estándar';
        });
    });
</script>


        </div><!-- /wizard-content -->

        <div class="actions">
            <button type="button" class="btn-nav btn-prev" onclick="prevStep()" id="btnPrev" style="visibility:hidden;">Anterior</button>
            <button type="button" class="btn-nav btn-next" onclick="nextStep()" id="btnNext">Siguiente</button>
            <button type="button" class="btn-nav btn-finish" onclick="saveExam()" id="btnFinish" style="display:none;">Guardar Examen</button>
        </div>
    </div>

    <!-- Modal: Bifurcación Antidoping después del Examen Médico -->
    <div class="custom-modal-overlay" id="modalConfirmacionAD">
        <div class="custom-modal" style="width: 500px;">
            <div class="cm-header">
                <h4><i class="fas fa-check-circle" style="color:#27ae60;"></i> ¡Evaluación Guardada!</h4>
            </div>
            <div class="cm-body" style="text-align:center; padding: 24px 20px;">
                <p style="font-size: 1.05rem; font-weight: 500; color: #333; margin-bottom: 10px;">
                    La evaluación médica se ha guardado y la solicitud fue marcada como <strong>Completada</strong>.
                </p>
                <p style="font-size: 0.95rem; color: #555; margin-top: 14px;">
                    <i class="fas fa-flask" style="color:#c0392b;"></i>
                    ¿Desea realizar <strong>Examen Antidoping</strong> ahora para este mismo candidato?
                </p>
            </div>
            <div class="cm-footer" style="justify-content: center; gap: 16px;">
                <button type="button" class="btn-nav btn-prev"
                        style="background:#f0f0f0; color:#444; border:1px solid #ddd; min-width:130px;"
                        onclick="finalizarSinAntidoping()">
                    <i class="fas fa-times"></i> No, finalizar
                </button>
                <button type="button" class="btn-nav btn-next"
                        style="background:#c0392b; min-width:130px;"
                        onclick="irAntidopingAhora()">
                    <i class="fas fa-flask"></i> Sí, Antidoping
                </button>
            </div>
        </div>
    </div>

    <script>
        function finalizarSinAntidoping() {
            window.location.href = 'DashboardServicioMedico.aspx';
        }
        function irAntidopingAhora() {
            if (typeof continuarAntidoping === 'function') {
                continuarAntidoping();
            } else {
                console.error("Función continuar Antidoping no encontrada.");
            }
        }
    </script>

    <!-- Custom Confirm/Message Modals -->
    <div class="custom-modal-overlay" id="confirmOverlay">
        <div class="custom-modal">
            <div class="cm-header"><h4>¿Confirmar Acción?</h4></div>
            <div class="cm-body"><p id="confirmBody"></p></div>
            <div class="cm-footer">
                <button type="button" class="btn-nav btn-prev" onclick="handleConfirm(false)">Cancelar</button>
                <button type="button" class="btn-nav btn-next" onclick="handleConfirm(true)">Confirmar</button>
            </div>
        </div>
    </div>

    <div class="custom-modal-overlay" id="msgOverlay">
        <div class="custom-modal">
            <div class="cm-header"><h4 id="msgTitle"></h4></div>
            <div class="cm-body">
                <div id="msgIcon" style="text-align:center; font-size: 2.5rem; margin-bottom: 10px;"></div>
                <p id="msgBody"></p>
            </div>
            <div class="cm-footer">
                <button type="button" id="btnMsgOk" class="btn-nav btn-next" onclick="$('#msgOverlay').hide()">Entendido</button>
            </div>
        </div>
    </div>

    <!-- Scripts del Módulo Médico -->    
</div>


<style>
    /* ── Reset limpio para este módulo ───────────────────── */
    *, *::before, *::after { box-sizing: border-box; }

    body { background: #f0f4f8; font-family: 'Inter', 'Segoe UI', sans-serif; margin: 0; }

    /* ── Banner del Paciente ─────────────────────────────── */
    .paciente-banner {
        background: linear-gradient(135deg, #1a5276 0%, #2471a3 100%);
        color: #fff;
        padding: 14px 24px;
        display: flex;
        align-items: center;
        gap: 16px;
        box-shadow: 0 3px 10px rgba(0,0,0,0.15);
        position: sticky;
        top: 0;
        z-index: 200;
    }
    .pb-avatar { font-size: 2.8rem; opacity: 0.9; line-height: 1; }
    .pb-info { flex: 1; }
    .pb-nombre { font-size: 1.25rem; font-weight: 700; letter-spacing: 0.3px; }
    .pb-detalle { font-size: 0.85rem; opacity: 0.85; margin-top: 2px; }
    .pb-sep { margin: 0 8px; opacity: 0.5; }
    .pb-badges { display: flex; flex-direction: column; gap: 5px; align-items: flex-end; }
    .pb-badge {
        font-size: 0.72rem; font-weight: 700; padding: 3px 10px;
        border-radius: 20px; text-transform: uppercase; letter-spacing: 0.5px;
    }
    .pb-badge-servicio { background: rgba(255,255,255,0.2); color: #fff; }
    .pb-badge-tipo { background: #f39c12; color: #fff; }
    .pb-badge-candidato { background: #27ae60 !important; }

    /* ── Expediente Clínico (Paso 0) ────────────────────── */
    .expr-card {
        background: #fff;
        border-radius: 10px;
        border: 1px solid #e0e7ef;
        margin-bottom: 16px;
        overflow: hidden;
        box-shadow: 0 2px 6px rgba(0,0,0,0.05);
        transition: box-shadow 0.2s;
    }
    .expr-card:hover { box-shadow: 0 4px 14px rgba(0,0,0,0.10); }
    .expr-card-header {
        display: flex; align-items: center; gap: 14px;
        padding: 14px 20px;
        cursor: pointer;
        border-bottom: 1px solid transparent;
        transition: background 0.2s;
    }
    .expr-card-header:hover { background: #f6f9fc; }
    .expr-card-header.open { border-bottom-color: #e0e7ef; }
    .expr-fecha {
        font-size: 1rem; font-weight: 700; color: #1a5276;
        min-width: 100px;
    }
    .expr-lugar { font-size: 0.82rem; color: #888; flex: 1; }
    .expr-aptitud {
        padding: 4px 14px; border-radius: 20px;
        font-size: 0.78rem; font-weight: 800; text-transform: uppercase;
    }
    .expr-aptitud.APTO { background: #d5f5e3; color: #1e8449; }
    .expr-aptitud.APTO-CON-RESTRICCIONES { background: #fef9e7; color: #b7770d; }
    .expr-aptitud.NO-APTO { background: #fde8e8; color: #922b21; }
    .expr-aptitud.default { background: #f0f0f0; color: #555; }
    .expr-chevron { color: #aaa; font-size: 0.9rem; transition: transform 0.2s; }
    .expr-chevron.open { transform: rotate(180deg); }

    .expr-body { padding: 16px 20px 20px; display: none; }
    .expr-vitals-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
        gap: 10px;
        margin-bottom: 14px;
    }
    .expr-vital {
        background: #f4f8fc; border-radius: 8px; padding: 10px 12px;
        border-left: 3px solid #2980b9;
    }
    .expr-vital label { font-size: 0.7rem; color: #888; text-transform: uppercase; display: block; margin-bottom: 2px; }
    .expr-vital span { font-size: 1.1rem; font-weight: 700; color: #1a5276; }
    .expr-vital .expr-unit { font-size: 0.7rem; color: #999; font-weight: 400; }

    .expr-section-title { font-size: 0.78rem; font-weight: 700; color: #555; text-transform: uppercase; margin: 14px 0 8px; letter-spacing: 0.5px; }
    .expr-tags { display: flex; flex-wrap: wrap; gap: 6px; }
    .expr-tag {
        background: #fde8e8; color: #922b21;
        border-radius: 14px; padding: 3px 10px;
        font-size: 0.75rem; font-weight: 600;
    }
    .expr-tag-more { background: #edf2f7; color: #4a5568; }
    .expr-alert {
        display: flex; align-items: center; gap: 10px;
        border-radius: 8px; padding: 10px 12px;
        margin-bottom: 12px; font-size: 0.86rem;
    }
    .expr-alert i { font-size: 1rem; }
    .expr-alert-risk {
        background: #fff5f5;
        border: 1px solid #f5c2c7;
        color: #842029;
    }
    .expr-alert-ok {
        background: #eefaf0;
        border: 1px solid #c3e6cb;
        color: #1f6f35;
    }
    .expr-no-risk {
        background: #f8fafc;
        border: 1px dashed #d5dde5;
        border-radius: 8px;
        padding: 10px 12px;
        color: #6c7a89;
        font-size: 0.84rem;
    }
    .expr-muted-note {
        margin-top: 10px;
        color: #7a828a;
        font-size: 0.84rem;
    }
    .expr-diagnostico {
        background: #f8f9fa; border-radius: 8px; padding: 10px 14px;
        font-size: 0.88rem; color: #444; line-height: 1.5;
        border-left: 3px solid #1a5276; margin-top: 8px;
    }
    .exp-loading { text-align: center; padding: 40px; color: #999; font-size: 1rem; }
    .exp-empty { text-align: center; padding: 40px; color: #aaa; }
    .badge-expediente {
        background: #1a5276; color: #fff;
        border-radius: 20px; padding: 4px 14px;
        font-size: 0.8rem; font-weight: 700;
    }

    /* ── Header ──────────────────────────────────────────── */
    .ad-page-header {
        background: #fff;
        padding: 14px 30px;
        border-bottom: 3px solid #c0392b;
        box-shadow: 0 2px 8px rgba(0,0,0,0.07);
        display: flex;
        align-items: center;
        gap: 14px;
    }
    .ad-page-header h1 {
        margin: 0;
        font-size: 1.3rem;
        font-weight: 800;
        color: #c0392b;
        text-transform: uppercase;
        letter-spacing: 0.5px;
    }
    .ad-page-header .flask-icon { font-size: 1.5rem; color: #c0392b; }
    .ad-patient-badge {
        margin-left: auto;
        background: #fdf2f8;
        border: 1px solid #e8a0c0;
        border-radius: 20px;
        padding: 5px 16px;
        font-size: 0.82rem;
        font-weight: 600;
        color: #8e44ad;
    }

    /* ── Contenedor principal ────────────────────────────── */
    .ad-page-body { max-width: 860px; margin: 30px auto; padding: 0 20px 60px; }

    /* ── Card de paciente ────────────────────────────────── */
    .patient-card {
        background: #fff;
        border: 1px solid #d0d0d0;
        border-left: 5px solid #1a5276;
        border-radius: 8px;
        padding: 18px 24px;
        margin-bottom: 22px;
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 8px 20px;
        font-size: 0.88rem;
    }
    .patient-card strong { color: #555; }
    .patient-card span   { color: #111; font-weight: 600; }

    /* ── Secciones ───────────────────────────────────────── */
    .ad-card {
        background: #fff;
        border: 1px solid #d8d8d8;
        border-radius: 10px;
        padding: 24px 28px;
        margin-bottom: 22px;
        box-shadow: 0 2px 8px rgba(0,0,0,0.04);
    }
    .ad-card-title {
        font-size: 1rem;
        font-weight: 800;
        color: #1a5276;
        border-bottom: 2px solid #eaf2f8;
        padding-bottom: 10px;
        margin-bottom: 20px;
        display: flex;
        align-items: center;
        gap: 8px;
    }

    /* ── Grilla de drogas ────────────────────────────────── */
    .drug-grid {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 14px;
    }
    @media (max-width: 600px) { .drug-grid { grid-template-columns: 1fr; } }

    .drug-row {
        display: flex;
        align-items: center;
        gap: 10px;
        background: #f9fafb;
        border: 1px solid #e8e8e8;
        border-radius: 8px;
        padding: 10px 14px;
        transition: border-color 0.2s;
    }
    .drug-row:hover { border-color: #aac; }

    .drug-name {
        flex: 1;
        font-size: 0.84rem;
        font-weight: 600;
        color: #333;
    }

    /* Switch NEG / POS */
    .result-switch { display: flex; border-radius: 6px; overflow: hidden; border: 1px solid #ddd; }
    .result-switch .sw-btn {
        padding: 5px 10px;
        font-size: 0.75rem;
        font-weight: 700;
        cursor: pointer;
        border: none;
        background: #f0f0f0;
        color: #888;
        transition: all 0.15s;
    }
    .result-switch .sw-btn.neg.active { background: #27ae60; color: #fff; }
    .result-switch .sw-btn.pos.active { background: #e74c3c; color: #fff; }
    .result-switch .sw-btn.disabled-btn { opacity: 0.4; cursor: not-allowed; }

    /* Checkbox Aplica */
    .chk-aplica-wrap { display: flex; align-items: center; gap: 4px; font-size: 0.75rem; color: #666; }
    .chk-aplica-wrap input[type=checkbox] { accent-color: #1a5276; width: 14px; height: 14px; }

    /* ── Comentarios ─────────────────────────────────────── */
    .ad-textarea {
        width: 100%;
        border: 1px solid #d0d0d0;
        border-radius: 6px;
        padding: 10px 14px;
        font-size: 0.9rem;
        font-family: inherit;
        resize: vertical;
        min-height: 80px;
        outline: none;
        transition: border-color 0.2s;
    }
    .ad-textarea:focus { border-color: #1a5276; }

    /* ── Evidencia ───────────────────────────────────────── */
    .evidence-drop {
        border: 2px dashed #c0d0e0;
        border-radius: 8px;
        padding: 20px;
        text-align: center;
        color: #888;
        font-size: 0.85rem;
        cursor: pointer;
        transition: border-color 0.2s, background 0.2s;
    }
    .evidence-drop:hover { border-color: #1a5276; background: #f0f8ff; }
    .evidence-drop input[type=file] { display: none; }

    /* ── Veredicto ───────────────────────────────────────── */
    .verdict-bar {
        display: flex;
        align-items: center;
        gap: 14px;
        padding: 16px 20px;
        border-radius: 8px;
        border: 2px solid #27ae60;
        background: #eafaf1;
        transition: all 0.3s;
    }
    .verdict-bar.positivo { border-color: #e74c3c; background: #fdf0ef; }
    .verdict-icon { font-size: 1.8rem; }
    .verdict-text { font-size: 1rem; font-weight: 800; }

    /* ── Acciones ────────────────────────────────────────── */
    .ad-actions {
        display: flex;
        gap: 12px;
        justify-content: flex-end;
        margin-top: 10px;
    }
    .btn-ad {
        padding: 10px 28px;
        border-radius: 6px;
        border: none;
        font-size: 0.9rem;
        font-weight: 700;
        cursor: pointer;
        transition: opacity 0.2s, transform 0.1s;
    }
    .btn-ad:hover   { opacity: 0.88; transform: translateY(-1px); }
    .btn-ad:active  { transform: translateY(0); }
    .btn-ad-cancel  { background: #f0f0f0; color: #555; }
    .btn-ad-save    { background: #1a5276; color: #fff; }
    .btn-ad-save:disabled { opacity: 0.5; cursor: not-allowed; transform: none; }
    .btn-ad-print   { background: #27ae60; color: #fff; }

    /* ── Antecedentes Genitourinario Masculino ─────────────────────────── */
    .antecedentes-grid {
        border: 1px solid #e0e0e0;
        border-radius: 6px;
        padding: 12px;
        background-color: #fafafa;
    }
    
    .antecedentes-grid > div {
        padding: 4px 0;
    }
    
    .antecedentes-grid > div:nth-child(even) {
        background-color: #f5f5f5;
        border-radius: 4px;
        padding: 4px 8px;
    }
    
    .antecedentes-grid input[type="text"] {
        font-size: 0.9rem;
        min-height: 32px;
    }
    
    .antecedentes-grid input[type="checkbox"] {
        transform: scale(1.1);
    }

    /* ── Modal de consentimiento ─────────────────────────── */
    .consent-overlay {
        position: fixed; inset: 0;
        background: rgba(10, 20, 40, 0.70);
        display: flex; align-items: center; justify-content: center;
        z-index: 5000;
        backdrop-filter: blur(4px);
    }
    .consent-box {
        background: #fff;
        border-radius: 14px;
        width: 560px;
        max-width: 95vw;
        box-shadow: 0 20px 60px rgba(0,0,0,0.3);
        overflow: hidden;
    }
    .consent-head {
        background: linear-gradient(135deg, #1a5276, #2980b9);
        color: #fff;
        padding: 16px 24px;
        font-weight: 800;
        font-size: 1rem;
        display: flex;
        align-items: center;
        gap: 10px;
    }
    .consent-body { padding: 24px; }
    .consent-text-box {
        background: #f8f9fa;
        border: 1px solid #dee2e6;
        border-radius: 6px;
        padding: 14px;
        font-size: 0.85rem;
        line-height: 1.6;
        color: #444;
        max-height: 180px;
        overflow-y: auto;
        margin-bottom: 18px;
    }
    .consent-check {
        display: flex;
        align-items: center;
        gap: 10px;
        font-size: 0.9rem;
        font-weight: 600;
        color: #333;
        cursor: pointer;
        margin-bottom: 18px;
    }
    .consent-check input { accent-color: #1a5276; width: 18px; height: 18px; cursor: pointer; }
    .consent-foot {
        display: flex;
        gap: 10px;
        justify-content: flex-end;
        padding: 16px 24px;
        border-top: 1px solid #eee;
    }
    .btn-consent-cancel { background: #f0f0f0; color: #555; border: none; padding: 9px 22px; border-radius: 6px; font-weight: 600; cursor: pointer; }
    .btn-consent-ok     { background: #1a5276; color: #fff;  border: none; padding: 9px 22px; border-radius: 6px; font-weight: 600; cursor: pointer; }
    .btn-consent-ok:disabled { opacity: 0.45; cursor: not-allowed; }

    /* ── Modal de mensaje ────────────────────────────────── */
    .msg-overlay {
        position: fixed; inset: 0;
        background: rgba(10,20,40,0.65);
        display: none; align-items: center; justify-content: center;
        z-index: 6000;
    }
    .msg-box {
        background: #fff;
        border-radius: 12px;
        width: 380px;
        max-width: 94vw;
        padding: 32px 28px;
        text-align: center;
        box-shadow: 0 16px 40px rgba(0,0,0,0.25);
    }
    .msg-icon { font-size: 3rem; margin-bottom: 14px; }
    .msg-title { font-size: 1.1rem; font-weight: 800; color: #222; margin-bottom: 8px; }
    .msg-body  { font-size: 0.9rem; color: #666; margin-bottom: 22px; line-height: 1.5; }
    .btn-msg-ok { background: #1a5276; color: #fff; border: none; padding: 10px 32px; border-radius: 6px; font-weight: 700; cursor: pointer; font-size: 0.95rem; }
</style>

<!-- ══════════════════════════════════════════════════════
     MODAL DE CONSENTIMIENTO (Unificado)
     ════════════════════════════════════════════════════ -->
<div class="consent-overlay" id="consentOverlay" style="display:none;">
    <div class="consent-box">
        <div class="consent-head">
            <i class="fas fa-file-signature"></i>
            <span id="consentTitle">Consentimiento Informado</span>
        </div>
        <div class="consent-body">
            <div class="consent-text-box" id="consentBodyText">
                <!-- Se llena por JS -->
            </div>
            <label class="consent-check" style="margin-top:20px;">
                <input type="checkbox" id="chkAceptoConsentimiento" onchange="toggleConsentOk()" />
                Acepto los términos y manifiesto mi consentimiento.
            </label>
        </div>

        <div class="consent-foot">
            <button type="button" class="btn-consent-cancel" onclick="rechazarConsiento()">
                <i class="fas fa-times"></i> Cancelar
            </button>
            <button type="button" class="btn-consent-ok" id="btnAceptoCon" disabled onclick="aceptarConsiento()">
                <i class="fas fa-check"></i> Aceptar y Continuar
            </button>
        </div>
    </div>
</div>

<!-- ══════════════════════════════════════════════════════
     MODAL DE MENSAJE
     ════════════════════════════════════════════════════ -->
<div class="msg-overlay" id="adMsgOverlay" style="display:none;">
    <div class="msg-box">
        <div class="msg-icon" id="adMsgIcon"></div>
        <div class="msg-title" id="adMsgTitle"></div>
        <div class="msg-body"  id="adMsgBody"></div>
        <button type="button" class="btn-msg-ok" id="adMsgOkBtn" onclick="cerrarMsg()">Entendido</button>
    </div>
</div>

<!-- ══════════════════════════════════════════════════════
     ENCABEZADO
     ════════════════════════════════════════════════════ -->
<div id="secAntidoping" style="display:none;">

    <div class="ad-page-header">
        <i class="fas fa-flask flask-icon"></i>
        <h1>Examen Antidoping</h1>
    </div>

    <!-- ══════════════════════════════════════════════════════
         CUERPO PRINCIPAL
         ════════════════════════════════════════════════════ -->
    <div class="ad-page-body">

    <!-- Datos del paciente -->
    <div class="patient-card" id="patientCard">
        <div><strong>Paciente:</strong> <span id="lblAdNombre">Cargando...</span></div>
        <div><strong>No. Empleado:</strong> <span id="lblAdNumEmpleado">—</span></div>
        <div><strong>Empresa / Proyecto:</strong> <span id="lblAdEmpresa">—</span></div>
        <div><strong>Puesto:</strong> <span id="lblAdPuesto">—</span></div>
    </div>

    <!-- Evidencia fotográfica -->
    <div class="ad-card">
        <div class="ad-card-title">
            <i class="fas fa-camera"></i> Evidencia Fotográfica
        </div>
        <div class="evidence-drop" onclick="document.getElementById('fileEvidencia').click()">
            <i class="fas fa-cloud-upload-alt" style="font-size:1.6rem; color:#aac; margin-bottom:6px;"></i>
            <div>Haz clic para seleccionar foto de evidencia <span style="color:#aaa;">(opcional)</span></div>
            <input type="file" id="fileEvidencia" accept="image/*" onchange="onEvidenceSelected(this)" />
        </div>
        <div id="evidencePreview" style="margin-top:10px; display:none;">
            <img id="evidenceImg" src="" style="max-height:120px; border-radius:6px; border:1px solid #ddd;" />
            <span id="evidenceName" style="font-size:0.8rem; color:#555; margin-left:8px;"></span>
        </div>
    </div>

    <!-- Resultados de drogas -->
    <div class="ad-card">
        <div class="ad-card-title">
            <i class="fas fa-vials"></i> Resultados Preliminares
        </div>
        <div class="drug-grid" id="drugGrid">
            <!-- generado por JS -->
        </div>
    </div>

    <!-- Comentarios -->
    <div class="ad-card">
        <div class="ad-card-title"><i class="fas fa-comment-medical"></i> Comentarios / Observaciones</div>
        <textarea class="ad-textarea" id="txtComentariosAd" placeholder="Escriba aquí cualquier observación relevante..."></textarea>
    </div>

    <!-- Acciones -->
    <div class="ad-actions">
        <button type="button" class="btn-ad btn-ad-cancel" onclick="cancelarAd()">
            <i class="fas fa-arrow-left"></i> Cancelar
        </button>
        <button type="button" class="btn-ad btn-ad-print" id="btnImprimirAD" onclick="imprimirAntidoping()" style="display:none;">
            <i class="fas fa-print"></i> Imprimir Formato
        </button>
        <button type="button" class="btn-ad btn-ad-save" id="btnGuardarAd" onclick="guardarAd()">
            <i class="fas fa-save"></i> Guardar Antidoping
        </button>
    </div>

</div>

</div>

<script>
    // ── Variables inyectadas desde servidor ────────────────
    var idOrden = <%= IdOrden %>;

    // ── Catálogo de drogas ─────────────────────────────────
    var drugs = [
        { code: 'coc', name: 'Coca\u00edna (COC)' },
        { code: 'opi', name: 'Opi\u00e1ceos (OPI)' },
        { code: 'thc', name: 'Marihuana (THC)' },
        { code: 'alc', name: 'Alcohol' },
        { code: 'anf', name: 'Anfetaminas (AMP)' },
        { code: 'met', name: 'Metanfetaminas (MET)' },
        { code: 'mfn', name: 'Metilfenidato (MTD)' },
        { code: 'fen', name: 'Fentanilo (FEN)' },
        { code: 'bzd', name: 'Benzodiacepinas (BZO)' }
    ];

    // Modal redundant removed

    // ── Inicialización ─────────────────────────────────────
    $(document).ready(function() {
        renderDrugGrid();
        loadPatient();
    });

    // ── Renderizar grilla de drogas ────────────────────────
    function renderDrugGrid() {
        var html = '';
        drugs.forEach(function(d) {
            html += '<div class="drug-row" data-drug="' + d.code + '">' +
                '<span class="drug-name">' + d.name + '</span>' +
                '<label class="chk-aplica-wrap">' +
                    '<input type="checkbox" class="chk-aplica" checked onchange="toggleDrug(this)" />' +
                    '<span>Aplica</span>' +
                '</label>' +
                '<div class="result-switch">' +
                    '<button class="sw-btn neg active" onclick="setResult(this,\'neg\')">Neg</button>' +
                    '<button class="sw-btn pos"         onclick="setResult(this,\'pos\')">Pos</button>' +
                '</div>' +
            '</div>';
        });
        $('#drugGrid').html(html);
    }

    // ── Cargar datos del paciente ──────────────────────────
    function loadPatient() {
        $.ajax({
            url: 'EvaluacionMedica.aspx/ObtenerDatosPaciente',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ idOrden: idOrden }),
            success: function(r) {
                var resp = r.d;
                if (resp && resp.success && resp.paciente) {
                    var p = resp.paciente;
                    $('#lblAdNombre').text(p.NombreCompleto || '—');
                    $('#lblAdNumEmpleado').text(p.NumeroEmpleado || '—');
                    var em = p.Empresa || 'la empresa';
                    $('#lblAdEmpresa').text(em);
                    $('#lblAdPuesto').text(p.Puesto || '—');
                    $('#lblConsentEmpresa').text(em);
                }
            }
        });
    }

    // (Consentimiento inicializado en loadPatientData callback en EvaluacionMedica.js)

    function toggleConsentOk() {
        $('#btnAceptoCon').prop('disabled', !$('#chkAceptoConsentimiento').is(':checked'));
    }

    function aceptarConsiento() {
        $('#consentOverlay').fadeOut(200, function() {
            // Si estamos en flujo de Antidoping (ya se guardó el médico)
            // o si el servicio es puramente Antidoping desde el inicio
            if (currentTipoServicio == 3 || window.__antidopingFlow === true || $('#mainWizard').css('display') === 'none') {
                showSuccess("Consentimiento aceptado. Iniciando prueba Toxicológica...", function() {
                    if($('#mainWizard').length) $('#mainWizard').hide();
                    $('#secAntidoping').fadeIn(400);
                    $('.ad-container').show().css('visibility', 'visible');
                    window.scrollTo(0,0);
                });
            } else {
                // Flujo médico normal, solo cerramos y dejamos que el usuario capture
                showToast("Consentimiento registrado correctamente.", "success");
            }
        });
    }

    function rechazarConsiento() {
        if (confirm('Sin el consentimiento no es posible realizar la evaluación. ¿Desea salir del sistema?')) {
            window.location.href = '../RecursosHumanos/DashboardServicioMedico.aspx';
        }
    }

    // ── Resultados de drogas ──────────────────────────────
    function toggleDrug(chk) {
        var $row = $(chk).closest('.drug-row');
        var $btns = $row.find('.sw-btn');
        if (!chk.checked) {
            $btns.addClass('disabled-btn').prop('disabled', true);
        } else {
            $btns.removeClass('disabled-btn').prop('disabled', false);
            if (!$row.find('.sw-btn.active').length) {
                $row.find('.sw-btn.neg').addClass('active');
            }
        }
    }

    function setResult(btn, tipo) {
        var $btn = $(btn);
        if ($btn.hasClass('disabled-btn')) return;
        $btn.closest('.result-switch').find('.sw-btn').removeClass('active neg pos');
        $btn.addClass('active').addClass(tipo);
    }

    // ── Evidencia ─────────────────────────────────────────
    function onEvidenceSelected(input) {
        if (input.files && input.files[0]) {
            var reader = new FileReader();
            reader.onload = function(e) {
                $('#evidenceImg').attr('src', e.target.result);
                $('#evidenceName').text(input.files[0].name);
                $('#evidencePreview').show();
            };
            reader.readAsDataURL(input.files[0]);
        }
    }

    // ── Guardar ───────────────────────────────────────────
    function guardarAd() {
        $('#btnGuardarAd').prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Guardando...');

        var fd = new FormData();
        fd.append('PkOrdenMedico', idOrden);
        fd.append('ConsentimientoFirmado', true);
        fd.append('Comentarios', $('#txtComentariosAd').val());

        // Veredicto calculado internamente (sin mostrarlo en pantalla)
        var hasPos = $('#drugGrid .sw-btn.pos.active').length > 0;
        fd.append('VeredictoFinal', hasPos ? 'POSITIVO' : 'NEGATIVO');

        // Evidencia
        var $f = $('#fileEvidencia');
        if ($f[0].files.length > 0) fd.append('FileEvidencia', $f[0].files[0]);

        // Drogas
        var map = { 'coc':'Cocaina','opi':'Opiaceos','thc':'THC','alc':'Alcohol',
                    'anf':'Anfetaminas','met':'Metanfetaminas','mfn':'Metilfenidato',
                    'fen':'Fentanilo','bzd':'Benzodiacepinas' };
        drugs.forEach(function(d) {
            var $row = $('[data-drug="' + d.code + '"]');
            var aplica  = $row.find('.chk-aplica').is(':checked');
            var positivo = $row.find('.sw-btn.pos.active').length > 0;
            fd.append('Aplica'    + map[d.code], aplica);
            fd.append('Resultado' + map[d.code], positivo);
        });

        $.ajax({
            url: 'EvaluacionMedica.aspx?action=GuardarAntidoping',
            type: 'POST',
            data: fd,
            contentType: false,
            processData: false,
            success: function(resp) {
                if (resp && resp.success) {
                    $('#btnImprimirAD').fadeIn(300); // Mostrar botón de imprimir
                    $('#btnGuardarAd').hide(); // Ocultar botón de guardar ya que se completó
                    
                    showAdMsg('success', '¡Antidoping Guardado!',
                        'El examen fue registrado correctamente. Puede imprimir el formato ahora o regresar a la bandeja.',
                        function() { window.location.href = 'DashboardServicioMedico.aspx'; });
                } else {
                    $('#btnGuardarAd').prop('disabled', false).html('<i class="fas fa-save"></i> Guardar Antidoping');
                    showAdMsg('error', 'Error al Guardar', resp.message || 'Ocurrió un error inesperado.');
                }
            },
            error: function(xhr) {
                $('#btnGuardarAd').prop('disabled', false).html('<i class="fas fa-save"></i> Guardar Antidoping');
                showAdMsg('error', 'Error de Conexión',
                    'No se pudo conectar con el servidor. (HTTP ' + xhr.status + ')');
            }
        });
    }

    function imprimirAntidoping() {
        var url = 'ImpresionFormatos.aspx?id=' + idOrden + '&tipo=ANTIDOPING';
        window.open(url, '_blank');
    }

    // ── Cancelar / sin antidoping ─────────────────────────
    function cancelarAd() {
        if (!confirm('¿Desea salir sin guardar el antidoping?')) return;
        completarSinAd();
    }

    function completarSinAd() {
        $.ajax({
            url: 'EvaluacionMedica.aspx/CompletarSinAntidoping',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ pkOrdenMedico: idOrden }),
            success: function() { window.location.href = 'DashboardServicioMedico.aspx'; },
            error: function() { window.location.href = 'DashboardServicioMedico.aspx'; }
        });
    }

    // ── Modales de mensaje ────────────────────────────────
    var _msgCallback = null;

    function showAdMsg(tipo, title, body, callback) {
        var icons = { success: '✅', error: '❌', info: 'ℹ️' };
        $('#adMsgIcon').text(icons[tipo] || 'ℹ️');
        $('#adMsgTitle').text(title);
        $('#adMsgBody').text(body);
        _msgCallback = callback || null;
        $('#adMsgOverlay').css('display', 'flex');
    }

    function cerrarMsg() {
        $('#adMsgOverlay').hide();
        if (_msgCallback) { var cb = _msgCallback; _msgCallback = null; cb(); }
    }
</script>
<%-- ═══ Loading Overlay ═══ --%>
<div id="loadingOverlay" style="display:none; position:fixed; top:0; left:0; width:100%; height:100%; background:rgba(255,255,255,0.7); backdrop-filter:blur(3px); z-index:10000; flex-direction:column; align-items:center; justify-content:center;">
    <div class="spinner-container" style="position:relative;">
        <div class="spinner-border text-primary" style="width: 4rem; height: 4rem;" role="status">
            <span class="visually-hidden">Loading...</span>
        </div>
        <i class="fas fa-hand-holding-medical" style="position:absolute; top:50%; left:50%; transform:translate(-50%, -50%); color:#2980b9; font-size:1.5rem;"></i>
    </div>
    <div style="margin-top:15px; font-family:'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; font-weight:600; color:#2c3e50; text-transform:uppercase; letter-spacing:1px;">Procesando informaci&oacute;n...</div>
</div>

</form>
<script src="./js/EvaluacionMedica.js"></script>
</body>
</html>




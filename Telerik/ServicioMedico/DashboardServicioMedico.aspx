<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DashboardServicioMedico.aspx.cs" Inherits="Telerik.ServicioMedico.DashboardServicioMedico" %>
<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Servicio Médico - Dashboard del Médico</title>
    
    <!-- Estilos Independientes -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.2/css/bootstrap.min.css" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.2/css/all.min.css" />
    <link rel="stylesheet" href="../RecursosHumanos/styles/DashboardRecursosHumanosSM.css" />
    <link rel="stylesheet" href="/ServicioMedico/styles/DashboardServicioMedico.css" />
</head>
<body>
    <form id="form1" runat="server">
        <div id="gci-modulo-medico-container">
            <div class="gci-container">
                <div class="gci-header">
                    <div>
                        <h2><i class="fas fa-user-md"></i> Panel de Servicio Médico</h2>
                        <span class="gci-header-subtitle">Gestión de Solicitudes y Evaluaciones Clínicas</span>
                    </div>
                    <div class="gci-header-actions">
                        <button type="button" class="btn-gci btn-gci-primary" onclick="abrirModalEvaluacionEmpleado()">
                            <i class="fas fa-plus-circle"></i> Nueva Evaluación Empleado
                        </button>
                    </div>
                </div>
                <!-- Tabla de resultados -->
                <main class="gci-panel">
                    <div class="sp-title-row">
                        <div class="sp-title-text"><i class="fas fa-list-ul"></i> Bandeja Médica</div>
                        <div class="sp-filters-inline" style="flex-wrap: wrap; justify-content: flex-end;">
                            <div>
                                Emp: <input type="number" id="filtroNumEmp" class="filter-input-mini" onkeyup="if(event.keyCode==13) aplicarFiltros(true)" placeholder="ID..." style="width:70px;" />
                            </div>
                            <div>
                                Mod:
                                <select id="filtroModalidad" class="filter-select-mini" onchange="aplicarFiltros(true)">
                                    <option value="">Todas</option>
                                    <option value="INGRESO">Ingreso</option>
                                    <option value="PERIODICO">Periódico</option>
                                </select>
                            </div>
                            <div>
                                Estatus:
                                <select id="filtroEstatus" class="filter-select-mini" onchange="aplicarFiltros(true)">
                                    <option value="-1">PENDIENTES</option>
                                    <option value="3">COMPLETADAS</option>
                                    <option value="">TODAS</option>
                                </select>
                            </div>
                            <div style="border-left: 1px solid #ddd; padding-left: 15px; margin-left: 5px;">
                                Mostrar:
                                <select id="selectTamanoPagina" class="filter-select-mini" onchange="onCambioTamanoPagina()">
                                    <option value="10">10</option>
                                    <option value="25" selected>25</option>
                                    <option value="50">50</option>
                                </select>
                            </div>
                            <button type="button" class="btn-gci btn-gci-secondary" onclick="aplicarFiltros(true)" style="padding:4px 8px;"><i class="fas fa-sync-alt"></i></button>
                        </div>
                    </div>
                    <div class="resultados-info" id="resultsInfo" style="margin-top: -10px; margin-bottom: 10px;"></div>
                    <div id="connectionStatus" style="display:none; padding:8px 15px; background:#fff3cd; border-left:4px solid #ffc107; margin-bottom:10px; font-size:13px; color:#856404;">
                        <i class="fas fa-exclamation-triangle"></i> <span id="connectionStatusText">Verificando conexión...</span>
                    </div>
                    <div class="table-responsive">
                        <table class="gci-table">
                            <thead>
                                <tr>
                                    <th>Folio</th>
                                    <th>Fecha</th>
                                    <th>Modalidad</th>
                                    <th>Candidato / Empleado</th>
                                    <th>Empresa</th>
                                    <th>Proyecto</th>
                                    <th>Servicio</th>
                                    <th>Estatus</th>
                                    <th>Aptitud</th>
                                    <th style="text-align:center;">Formato(s)</th>
                                </tr>
                            </thead>
                            <tbody id="tbodySolicitudes">
                                <tr><td colspan="10" class="no-data">Cargando bandeja...</td></tr>
                            </tbody>
                        </table>
                    </div>
                    <div id="paginacionControles" class="paginacion"></div>
                </main>
            </div>

            <!-- Modal de Detalle  -->
            <div class="modal-overlay" id="modalDetalle">
                <div class="modal-box" style="width: 780px;">
                    <div class="modal-header">
                        <h3><i class="fas fa-file-medical-alt"></i> Detalle de Evaluación</h3>
                        <button type="button" class="modal-close" onclick="cerrarModal()"><i class="fas fa-times"></i></button>
                    </div>
                    <div class="modal-body">
                        <!-- Card Superior -->
                        <div class="detail-section" style="background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%); padding: 2rem; border-radius: 20px; border-left: 8px solid var(--gci-accent);">
                            <div class="detail-grid">
                                <div class="detail-item">
                                    <label><i class="fas fa-building"></i> Empresa</label>
                                    <span id="detGeneralEmpresa">&mdash;</span>
                                </div>
                                <div class="detail-item">
                                    <label><i class="fas fa-map-marked-alt"></i> Proyecto / Obra</label>
                                    <span id="detGeneralProyecto">&mdash;</span>
                                </div>
                            </div>
                        </div>

                        <!-- Info Solicitud -->
                        <div class="detail-section">
                            <div class="ds-title">Información de la Solicitud</div>
                            <div class="detail-grid" style="grid-template-columns: repeat(3, 1fr);">
                                <div class="detail-item"><label>Folio</label><span id="modalFolio" style="color:var(--gci-accent)">&mdash;</span></div>
                                <div class="detail-item"><label>Modalidad</label><span id="detModalidad">&mdash;</span></div>
                                <div class="detail-item"><label>Estatus</label><span id="detEstatus">&mdash;</span></div>
                            </div>
                        </div>

                        <!-- Paciente -->
                        <div class="detail-section" style="border-top: 2px dashed #f1f5f9; padding-top: 1.5rem;">
                            <div class="ds-title">Datos del Paciente</div>
                            <div id="seccionIngresoNotice" class="status-badge" style="background: #eff6ff; color: #1d4ed8; display:none; margin-bottom:1rem; border:1px solid #dbeafe;">
                                <i class="fas fa-info-circle"></i> Nuevo Ingreso detectado
                            </div>
                            <div class="detail-grid" style="grid-template-columns: 2fr 1fr;">
                                <div class="detail-item">
                                    <label id="lblNomPersona">Nombre Completo</label>
                                    <span id="detPacienteNombre" style="font-size: 1.4rem; font-family: 'Outfit';">&mdash;</span>
                                </div>
                                <div class="detail-item">
                                    <label>No. Empleado</label>
                                    <span id="detPacienteEmpNum">&mdash;</span>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="modal-actions">
                        <a href="#" id="btnIrEvaluar" class="btn-gci btn-gci-primary"><i class="fas fa-stethoscope"></i> Iniciar Evaluación</a>
                        <a href="#" id="btnVerResultados" class="btn-gci btn-gci-secondary"><i class="fas fa-file-medical"></i> Ver Resultados</a>
                        <div style="flex:1;"></div>
                        <button type="button" class="btn-gci btn-gci-secondary" onclick="cerrarModal()">Cerrar</button>
                    </div>
                </div>
            </div>

            <!-- Modal Mensajes -->
            <div class="modal-overlay" id="msgOverlay">
                <div class="modal-box" style="width: 440px; text-align: center;">
                    <div class="modal-body" style="padding: 3rem;">
                        <div id="msgIcon" style="font-size: 4.5rem; margin-bottom: 1.5rem; color: var(--gci-accent);"></div>
                        <h3 id="msgTitle" style="margin-bottom: 1rem;"></h3>
                        <p id="msgBody" style="color: var(--gci-secondary); line-height: 1.6;"></p>
                    </div>
                    <div class="modal-actions" style="justify-content: center;">
                        <button type="button" class="btn-gci btn-gci-primary" style="width: 100%;" onclick="cerrarMsg()">Entendido</button>
                    </div>
                </div>
            </div>

            <!-- Modal Pase Médico -->
            <div class="modal-overlay" id="modalPase" style="z-index: 10000;">
                <div class="modal-box" style="width: 900px; max-height: 90vh; display: flex; flex-direction: column;">
                    <div class="modal-header">
                        <h3><i class="fas fa-file-signature"></i> Pase Médico</h3>
                        <button type="button" class="modal-close" onclick="cerrarModalPase()"><i class="fas fa-times"></i></button>
                    </div>
                    <div class="modal-body" style="padding:0; background:#f5f5f5; overflow:auto; flex:1;">
                        <div id="paseContent" style="padding:20px;"></div>
                    </div>
                    <div class="modal-actions">
                        <button type="button" class="btn-gci btn-gci-info" onclick="imprimirPaseModal()">
                            <i class="fas fa-print"></i> Imprimir Pase
                        </button>
                        <div style="flex:1;"></div>
                        <button type="button" class="btn-gci btn-gci-secondary" onclick="cerrarModalPase()">Cerrar</button>
                    </div>
                </div>
            </div>

            <!-- Modal: Previsualización de Evaluación Médica (Formato GCI-FOR-SYM-45) -->
            <div class="modal-overlay" id="modalPreviewEvaluacion">
                <div class="modal-box" style="width: 96vw; max-width: 980px;">
                    <div class="modal-header">
                        <h3><i class="fas fa-file-medical"></i> Previsualización — Examen Médico</h3>
                        <button type="button" class="modal-close" onclick="cerrarPreviewEvaluacion()"><i class="fas fa-times"></i></button>
                    </div>
                    <div class="modal-body" style="padding: 0;">
                        <div class="preview-toolbar">
                            <div class="pt-left">
                                <strong>Folio:</strong> <span id="prevFolio">—</span>
                                <span class="pt-sep">|</span>
                                <strong>Orden:</strong> <span id="prevOrden">—</span>
                            </div>
                            <div class="pt-right">
                                <button type="button" class="btn-gci btn-gci-secondary" onclick="cerrarPreviewEvaluacion()">Cerrar</button>
                            </div>
                        </div>

                        <div class="exam-preview-scroll">
                            <!-- ===================== PAGE 1 ===================== -->
                            <div class="exam-page" id="examPage1">
                                <div class="form-wrapper">
                                    <!-- HEADER -->
                                    <table class="header-table">
                                        <tr>
                                            <td class="header-logo" style="width:90px; height:50px;">
                                                <div style="border:2px solid #4a7c3f; padding:4px; display:inline-block;">
                                                    <span style="color:#4a7c3f; font-size:20px; font-weight:900; letter-spacing:-1px;">GCI</span>
                                                </div>
                                                <div style="font-size:6px; color:#4a7c3f; margin-top:2px;">GRUPO CONSTRUCTOR INDUSTRIAL</div>
                                            </td>
                                            <td class="header-center">
                                                <div class="title-main">SEGURIDAD, SALUD Y MEDIO AMBIENTE</div>
                                                <div class="title-sub">EXAMEN MÉDICO</div>
                                                <div class="title-company">GRUPO CONSTRUCTOR INDUSTRIAL OIL &amp; GAS S.A. DE C.V.</div>
                                            </td>
                                            <td class="header-hse" style="width:80px; height:50px;">
                                                <div style="border:2px solid #2255aa; padding:4px 6px; display:inline-block;">
                                                    <span style="color:#2255aa; font-size:16px; font-weight:900;">HSE</span>
                                                </div>
                                            </td>
                                        </tr>
                                    </table>

                                    <!-- HOJA -->
                                    <div class="hoja-row">
                                        <strong>HOJA:</strong>&nbsp;&nbsp;1&nbsp;&nbsp;<strong>DE</strong>&nbsp;&nbsp;2
                                    </div>

                                    <!-- INFO ROWS -->
                                    <table class="info-table">
                                        <tr>
                                            <td style="width:60%;">
                                                <strong>Lugar y fecha del Examen:</strong>&nbsp;<span class="val" id="prevLugarFecha"></span>
                                            </td>
                                            <td>
                                                <strong>Cargo:</strong>&nbsp;<span class="val" id="prevCargo"></span>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <strong>Nombre:</strong>&nbsp;<span class="val" id="prevNombre" style="width:280px;"></span>
                                            </td>
                                            <td>
                                                <strong>No. IMSS</strong>&nbsp;<span class="val" id="prevNss"></span>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colspan="2">
                                                <strong>Fecha de Nacimiento:</strong>&nbsp;<span class="val" id="prevNacimiento"></span>
                                                &nbsp;&nbsp;<strong>Edad:</strong>&nbsp;<span class="val" id="prevEdad"></span>&nbsp;años
                                                &nbsp;&nbsp;<strong>Lugar de nacimiento:</strong>&nbsp;<span class="val" id="prevLugarNac" style="width:120px;"></span>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colspan="2">
                                                <strong>Estado Civil:</strong>&nbsp;
                                                <span class="cb" id="prevEcSoltero"></span>soltero&nbsp;&nbsp;
                                                <span class="cb" id="prevEcCasado"></span>casado&nbsp;&nbsp;
                                                <span class="cb" id="prevEcUnion"></span>union libre&nbsp;&nbsp;
                                                <span class="cb" id="prevEcSeparado"></span>separado&nbsp;&nbsp;
                                                <strong>Mano dominante</strong><span class="val" id="prevMano"></span>&nbsp;&nbsp;
                                                <strong>Teléfono:</strong><span class="val" id="prevTelefono"></span>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colspan="2">
                                                <strong>Domicilio:</strong>&nbsp;<span class="val" id="prevDomicilio" style="width:350px;"></span>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colspan="2">
                                                <strong>Nivel Académico:</strong>&nbsp;&nbsp;
                                                <span class="cb" id="prevNaPrimaria"></span>Primaria&nbsp;&nbsp;
                                                <span class="cb" id="prevNaSecundaria"></span>Secundaria&nbsp;&nbsp;
                                                <span class="cb" id="prevNaMedia"></span>Media Sup.&nbsp;&nbsp;
                                                <span class="cb" id="prevNaUniversidad"></span>Universidad&nbsp;&nbsp;
                                                <strong>Profesión:</strong><span class="val" id="prevProfesion"></span>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colspan="2">
                                                <strong>Examen de:</strong>&nbsp;&nbsp;
                                                <span class="cb" id="prevExIngreso"></span>Ingreso&nbsp;&nbsp;
                                                <span class="cb" id="prevExPeriodico"></span>Periódico&nbsp;&nbsp;
                                                <strong>Sexo:</strong>&nbsp;
                                                <span class="cb" id="prevSexoM"></span>Masc&nbsp;
                                                <span class="cb" id="prevSexoF"></span>Fem&nbsp;&nbsp;
                                                <strong>Tipo de Sangre:</strong><span class="val" id="prevTipoSangre"></span>
                                            </td>
                                        </tr>
                                    </table>

                                    <!-- ANTECEDENTE HEREDO FAMILIARES -->
                                    <div class="section-header">ANTECEDENTE HEREDO FAMILIARES</div>
                                    <div id="prevAhfContainer"></div>

                                    <!-- ANTECEDENTES PERSONALES Patológicos -->
                                    <div class="section-header">ANTECEDENTES PERSONALES Patológicos</div>
                                    <div id="prevAppContainer"></div>

                                    <!-- ANTECEDENTES LABORALES -->
                                    <div class="section-header">ANTECEDENTES LABORALES</div>
                                    <table class="laboral-table">
                                        <tr>
                                            <th style="width:30%;">EMPRESA</th>
                                            <th style="width:15%;">TIEMPO</th>
                                            <th style="width:20%;">PUESTO</th>
                                            <th style="width:20%;">AGENTES EXPUESTOS</th>
                                            <th style="width:15%;">ACCIDENTES</th>
                                        </tr>
                                        <tr>
                                            <td id="prevLabEmpresa"></td>
                                            <td id="prevLabTiempo"></td>
                                            <td id="prevLabPuesto"></td>
                                            <td id="prevLabAgentes"></td>
                                            <td id="prevLabAccidentes"></td>
                                        </tr>
                                        <tr>
                                            <td></td><td></td><td></td><td></td><td></td>
                                        </tr>
                                    </table>

                                    <!-- HÁBITOS -->
                                    <div class="section-header">HÁBITOS</div>
                                    <div class="habitos-block" id="prevHabitosContainer"></div>

                                    <!-- VACUNAS -->
                                    <div class="vacunas-block" id="prevVacunasContainer"></div>

                                    <!-- EXPLORACIÓN FISICA -->
                                    <div class="section-header">EXPLORACIÓN FISICA</div>
                                    <div class="ef-block">
                                        <div class="ef-row">
                                            <div class="ef-field"><strong>TA:</strong>&nbsp;<span class="ef-val" id="prevTa"></span>&nbsp;mmHg</div>
                                            <div class="ef-field"><strong>FC:</strong>&nbsp;<span class="ef-val" id="prevFc"></span>&nbsp;x min</div>
                                            <div class="ef-field"><strong>FR:</strong>&nbsp;<span class="ef-val" id="prevFr"></span>&nbsp;x min</div>
                                            <div class="ef-field"><strong>Peso:</strong>&nbsp;<span class="ef-val" id="prevPeso"></span>&nbsp;kgs</div>
                                            <div class="ef-field"><strong>Estatura:</strong>&nbsp;<span class="ef-val" id="prevEstatura"></span>&nbsp;m</div>
                                        </div>
                                        <div class="ef-row">
                                            <div class="ef-field"><strong>IMC:</strong>&nbsp;<span class="ef-val" id="prevImc"></span></div>
                                            <div class="ef-field"><strong>Temp:</strong>&nbsp;<span class="ef-val" id="prevTemp"></span></div>
                                            <div class="ef-field"><strong>Aparatos y sistemas:</strong>&nbsp;<span class="ef-val" id="prevAparatos" style="min-width:200px;"></span></div>
                                        </div>
                                        <div style="margin-top:4px;"><strong>Síntomas:</strong>&nbsp;<span id="prevSintomas"></span></div>
                                    </div>

                                    <!-- EXPLORATION TABLE -->
                                    <table class="expl-table" id="prevExploracion">
                                        <tr>
                                            <th class="col-item"></th>
                                            <th class="col-norm">Normal</th>
                                            <th class="col-anorm">Anormal</th>
                                            <th class="col-desc">Descripción de Hallazgos</th>
                                        </tr>
                                    </table>

                                    <!-- FOOTER -->
                                    <div class="form-footer">GCI-FOR-SYM-45 REV. 0</div>
                                </div>
                            </div>

                            <!-- ===================== PAGE 2 ===================== -->
                            <div class="exam-page" id="examPage2">
                                <div class="form-wrapper">
                                    <!-- HEADER page 2 -->
                                    <table class="header-table">
                                        <tr>
                                            <td class="header-logo" style="width:90px; height:50px;">
                                                <div style="border:2px solid #4a7c3f; padding:4px; display:inline-block;">
                                                    <span style="color:#4a7c3f; font-size:20px; font-weight:900; letter-spacing:-1px;">GCI</span>
                                                </div>
                                                <div style="font-size:6px; color:#4a7c3f; margin-top:2px;">GRUPO CONSTRUCTOR INDUSTRIAL</div>
                                            </td>
                                            <td class="header-center">
                                                <div class="title-main">SEGURIDAD, SALUD Y MEDIO AMBIENTE</div>
                                                <div class="title-sub">EXAMEN MEDICO</div>
                                                <div class="title-company">GRUPO CONSTRUCTOR INDUSTRIAL OIL &amp; GAS S.A DE C.V.</div>
                                            </td>
                                            <td class="header-hse" style="width:80px; height:50px;">
                                                <div style="border:2px solid #2255aa; padding:4px 6px; display:inline-block;">
                                                    <span style="color:#2255aa; font-size:16px; font-weight:900;">HSE</span>
                                                </div>
                                            </td>
                                        </tr>
                                    </table>

                                    <!-- HOJA 2 -->
                                    <div class="hoja-row">
                                        <strong>HOJA:</strong>&nbsp;&nbsp;2&nbsp;&nbsp;<strong>DE</strong>&nbsp;&nbsp;2
                                    </div>

                                    <!-- GINECO-OBSTÉTRICOS -->
                                    <div class="p2-block" id="prevGinecoBlock">
                                        <div style="font-weight:bold; margin-bottom:3px; font-size:10px;">GINECO-OBSTÉTRICOS:</div>
                                        <div class="p2-row">
                                            <strong>Menarca:</strong><span class="p2-val" id="prevMenarca"></span>
                                            <strong>Ciclos:</strong><span class="p2-val" id="prevCiclos"></span>
                                            <strong>FUM:</strong><span class="p2-val" id="prevFum"></span>
                                            <strong>No. Hijos/Edades:</strong><span class="p2-val" id="prevNumHijos" style="min-width:100px;"></span>
                                        </div>
                                        <div class="p2-row">
                                            <strong>Planificación:</strong><span class="p2-val" id="prevPlanificacion" style="min-width:100px;"></span>
                                            <strong>IVSA:</strong><span class="p2-val" id="prevIvsaFem"></span>
                                            <strong>Cit. Vag:</strong><span class="p2-val" id="prevCitVag"></span>
                                            <strong>ETS:</strong><span class="p2-val" id="prevEts"></span>
                                        </div>
                                        <div class="p2-row">
                                            <strong>Gestas:</strong><span class="p2-val" id="prevGestas"></span>
                                            <strong>P:</strong><span class="p2-val" id="prevPartos"></span>
                                            <strong>A:</strong><span class="p2-val" id="prevAbortos"></span>
                                            <strong>C:</strong><span class="p2-val" id="prevCesareas"></span>
                                        </div>
                                    </div>

                                    <!-- ANTECEDENTES APARATO GENITOURINARIO MASCULINO -->
                                    <div class="p2-block" id="prevMasculinoBlock">
                                        <div style="font-weight:bold; margin-bottom:5px; font-size:10px;">ANTECEDENTES DE APARATO GENITOURINARIO MASCULINO:</div>
                                        <div class="p2-row">
                                            <strong>Examen Clínico:</strong>&nbsp;Prepicio retráctil&nbsp;<span class="cb" id="prevPrepucio"></span>
                                            &nbsp;&nbsp;<strong>Testículos: Descendidos:</strong>&nbsp;<span class="cb" id="prevTesticulos"></span>
                                            &nbsp;&nbsp;<strong>Fimosis:</strong>&nbsp;<span class="cb" id="prevFimosis"></span>
                                            &nbsp;&nbsp;<strong>Criptorquidia:</strong>&nbsp;<span class="cb" id="prevCriptorquidia"></span>
                                            &nbsp;&nbsp;<strong>Varicocele:</strong>&nbsp;<span class="cb" id="prevVaricocele"></span>
                                        </div>
                                        <div class="p2-row" style="margin-left:40px;">
                                            Hidrocele:&nbsp;<span class="cb" id="prevHidrocele"></span>
                                            &nbsp;&nbsp;Hernia:&nbsp;<span class="cb" id="prevHernia"></span>
                                            &nbsp;&nbsp;<strong>IVSA:</strong>&nbsp;<span class="cb" id="prevIvsaMasc"></span>
                                            &nbsp;&nbsp;<strong>PSA:</strong>&nbsp;<span class="cb" id="prevPsa"></span>
                                            &nbsp;&nbsp;<strong>MPF:</strong>&nbsp;<span class="cb" id="prevMpf"></span>
                                        </div>
                                    </div>

                                    <!-- COLUMNA VERTEBRAL -->
                                    <div class="cv-header">COLUMNA VERTEBRAL:</div>
                                    <div class="cv-legend">
                                        <span>N: Normal</span>
                                        <span>A: Aumentada</span>
                                        <span>D: Disminuida</span>
                                    </div>
                                    <table class="cv-table">
                                        <tr>
                                            <th style="width:20%;">CURVA</th>
                                            <th style="width:20%;">CERVICAL</th>
                                            <th style="width:30%;" class="dorsal">DORSAL</th>
                                            <th style="width:30%;">LUMBAR</th>
                                        </tr>
                                        <tr>
                                            <td class="row-label">LORDOSIS</td>
                                            <td id="prevLordC"></td>
                                            <td id="prevLordD" style="background:#f0f0f0;"></td>
                                            <td id="prevLordL"></td>
                                        </tr>
                                        <tr>
                                            <td class="row-label">CIFOSIS</td>
                                            <td id="prevCifoC"></td>
                                            <td id="prevCifoD" style="background:#f0f0f0;"></td>
                                            <td id="prevCifoL"></td>
                                        </tr>
                                    </table>
                                    <table class="cv-table" style="margin-top:2px;">
                                        <tr>
                                            <th style="width:20%;">ESCOLIOSIS</th>
                                            <th style="width:27%;">DORSAL</th>
                                            <th style="width:27%;">LUMBAR</th>
                                            <th style="width:26%;">DOBLE</th>
                                        </tr>
                                        <tr>
                                            <td class="row-label">DERECHA</td>
                                            <td id="prevEscDd"></td>
                                            <td id="prevEscLd"></td>
                                            <td id="prevEscDobD"></td>
                                        </tr>
                                        <tr>
                                            <td class="row-label">IZQUIERDA</td>
                                            <td id="prevEscDi"></td>
                                            <td id="prevEscLi"></td>
                                            <td id="prevEscDobI"></td>
                                        </tr>
                                    </table>

                                    <!-- DIAGNÓSTICO -->
                                    <div class="diag-block">
                                        <div><strong>DIAGNÓSTICO</strong></div>
                                        <div style="margin-top:4px;"><span class="diag-val" id="prevDiagnostico"></span></div>
                                        <div style="margin-top:4px;"><span class="diag-val" id="prevDiagnostico2"></span></div>
                                    </div>

                                    <!-- RESULTADO -->
                                    <div class="diag-block">
                                        <div class="p2-row">
                                            <strong>RESULTADO:</strong>&nbsp;&nbsp;
                                            <strong>APTO:</strong>&nbsp;<span class="cb" id="prevResApto"></span>&nbsp;&nbsp;
                                            <strong>NO APTO:</strong>&nbsp;<span class="cb" id="prevResNoApto"></span>&nbsp;&nbsp;
                                            <strong>CON RESTRICCIONES:</strong>&nbsp;<span class="cb" id="prevResRestr"></span>
                                        </div>
                                    </div>

                                    <!-- RECOMENDACIONES -->
                                    <div class="diag-block">
                                        <div><strong>RECOMENDACIONES:</strong></div>
                                        <div style="margin-top:4px;"><span class="diag-val" id="prevRecomendaciones"></span></div>
                                        <div style="margin-top:4px;"><span class="diag-val" id="prevRecomendaciones2"></span></div>
                                    </div>

                                    <!-- REALIZÓ -->
                                    <div class="diag-block">
                                        <div class="p2-row">
                                            <strong>REALIZÓ:</strong>&nbsp;<span class="p2-val" style="width:300px;"></span>
                                        </div>
                                        <div style="text-align:center; font-size:9px; margin-top:2px;">NOMBRE Y FIRMA</div>
                                    </div>

                                    <!-- DECLARATION -->
                                    <div class="declaration">
                                        Declaro que toda la información suministrada es verídica y que no he ocultado ningún dato sobre mis antecedentes y/o estado de salud y estoy consciente que cualquier omisión o falsificación la empresa tendrá la facultad de anular cualquier tramite relacionado conmigo y autorizo al servicio médico (salud ocupacional) de la empresa para que realice los exámenes necesarios con motivo de mi trabajo y sean utilizados con fines estadísticos y epidemiológicos. Así mismo autorizo al medico referente para poner en conocimiento de la empresa todo lo referente a los resultados del examen físico y de las pruebas auxiliares de diagnóstico.
                                    </div>

                                    <!-- FIRMA DEL TRABAJADOR -->
                                    <div class="firma-block">
                                        <div><strong>NOMBRE Y FIRMA DEL TRABAJADOR (A):</strong>&nbsp;<span id="prevNombreTrabajador"></span></div>
                                        <div class="firma-line"></div>
                                    </div>

                                    <!-- FOOTER -->
                                    <div class="form-footer">GCI-FOR-SYM-45 REV. 0</div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal Evaluación Empleado -->
        <div id="modalEvaluacionEmpleado" style="display:none;">
            <div class="modal-emp-overlay" onclick="if(event.target===this)cerrarModalEvaluacionEmpleado()">
                <div class="modal-emp-content">
                    <div class="modal-emp-header">
                        <h3><i class="fas fa-user-plus"></i> Nueva Evaluación - Empleado</h3>
                        <button type="button" class="modal-emp-close" onclick="cerrarModalEvaluacionEmpleado()">&times;</button>
                    </div>
                    <div class="modal-emp-body">
                        <label style="font-weight:600; margin-bottom:8px; display:block;">Buscar por Número de Empleado:</label>
                        <div class="emp-search-row">
                            <input type="number" id="txtNumEmpleado" class="emp-search-input" placeholder="Ingrese número de empleado..." />
                            <button type="button" class="btn-gci btn-gci-primary" onclick="buscarEmpleado()">
                                <i class="fas fa-search"></i> Buscar
                            </button>
                        </div>
                        <div id="empResultado"></div>
                    </div>
                    <div class="modal-emp-footer">
                        <button type="button" class="btn-gci btn-gci-secondary" onclick="cerrarModalEvaluacionEmpleado()">Cancelar</button>
                        <button type="button" id="btnCrearEvaluacion" class="btn-gci btn-gci-primary" style="display:none;" onclick="crearEvaluacionEmpleado()">
                            <i class="fas fa-file-medical"></i> Crear Evaluación
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </form>

    <script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>
    <script src="./js/DashboardServicioMedicoInbox.js"></script>
    <script>
        function cerrarMsg() { $('#msgOverlay').removeClass('active'); }
        function cerrarPreviewEvaluacion() { $('#modalPreviewEvaluacion').removeClass('active'); }
        // Extend default behavior to use classes
        function showMsg(title, body, icon) {
            $('#msgTitle').text(title);
            $('#msgBody').text(body);
            $('#msgIcon').html('<i class="fas ' + (icon || 'fa-info-circle') + '"></i>');
            $('#msgOverlay').addClass('active');
        }
    </script>
</body>
</html>

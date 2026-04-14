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

            </div>
        </div>

        <!-- ── Modal Vista Previa de Formato (iframe) ── -->
        <div class="modal-overlay" id="modalVistaPrevia" style="z-index:10001;">
            <div class="modal-box" style="width:96vw; max-width:1050px; height:92vh; display:flex; flex-direction:column;">
                <div class="modal-header">
                    <h3 id="modalVistaPreviaTitle"><i class="fas fa-file-medical"></i> Vista Previa</h3>
                    <button type="button" class="modal-close" onclick="cerrarVistaPrevia()"><i class="fas fa-times"></i></button>
                </div>
                <div style="flex:1; background:#888; overflow:hidden; display:flex; align-items:stretch;">
                    <iframe id="iframeVistaPrevia"
                            src="about:blank"
                            style="flex:1; border:none; width:100%; height:100%; background:white;"
                            title="Vista previa del formato"></iframe>
                </div>
                <div class="modal-actions">
                    <button type="button" class="btn-gci btn-gci-info" id="btnImprimirFormato" onclick="imprimirVistaPrevia()">
                        <i class="fas fa-print"></i> Imprimir
                    </button>
                    <div style="flex:1;"></div>
                    <button type="button" class="btn-gci btn-gci-secondary" onclick="cerrarVistaPrevia()">Cerrar</button>
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
        function showMsg(title, body, icon) {
            $('#msgTitle').text(title);
            $('#msgBody').text(body);
            $('#msgIcon').html('<i class="fas ' + (icon || 'fa-info-circle') + '"></i>');
            $('#msgOverlay').addClass('active');
        }
    </script>
</body>
</html>

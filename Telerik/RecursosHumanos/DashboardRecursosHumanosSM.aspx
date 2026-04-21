<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DashboardRecursosHumanosSM.aspx.cs"
    Inherits="Telerik.DashboardRecursosHumanosSM" %>
    <!DOCTYPE html>
    <html lang="es">

    <head runat="server">
        <meta charset="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <title>Servicio Médico - Bandeja Principal</title>

        <!-- Estilos Independientes -->
        <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.2/css/bootstrap.min.css" />
        <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.2/css/all.min.css" />
        <link rel="preconnect" href="https://fonts.googleapis.com">
        <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
        <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&display=swap"
            rel="stylesheet">
        <link rel="stylesheet" href="./styles/DashboardRecursosHumanosSM.css" />

        <style>
            /* Ajustes básicos en caso de que falten en el CSS */
            body {
                background-color: #f4f7f6;
                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            }

            /* Paginación */
            .paginacion-wrapper {
                display: flex;
                justify-content: center;
                align-items: center;
                gap: 8px;
                margin: 20px 0;
            }

            .pag-btn {
                padding: 5px 12px;
                border: 1px solid #ccc;
                border-radius: 4px;
                background: #fff;
                cursor: pointer;
                transition: all 0.2s;
            }

            .pag-btn:hover {
                background: #f0f0f0;
                border-color: #999;
            }

            .pag-btn.active {
                background: #007bff;
                color: #fff;
                border-color: #007bff;
                font-weight: bold;
            }

            .pag-btn:disabled {
                opacity: 0.5;
                cursor: default;
            }
        </style>
    </head>

    <body>
        <form id="form1" runat="server">
            <div id="gci-modulo-medico-container">
                <div class="gci-container">
                    <div class="gci-header">
                        <div>
                            <h2><i class="fas fa-users-cog"></i> Recursos Humanos</h2>
                            <span class="gci-header-subtitle">Módulo de Solicitudes Médicas</span>
                        </div>
                        <button type="button" class="btn-nueva-solicitud" onclick="abrirNuevaSolicitud()">
                            <i class="fas fa-plus-circle"></i> Nueva Solicitud
                        </button>
                    </div>

                    <!-- Tabla de solicitudes -->
                    <div class="gci-panel">
                        <div class="sp-title-row">
                            <div class="sp-title-text"><i class="fas fa-history"></i> Solicitudes Recientes</div>
                            <div class="sp-filters-inline">
                                <div>
                                    Nombre: <input type="text" id="filtroNombreMini" class="filter-input-mini"
                                        onkeyup="aplicarFiltros(true)" placeholder="Buscar por nombre..." style="width:160px;" />
                                </div>
                                <div>
                                    Folio / ID: <input type="text" id="filtroIdMini" class="filter-input-mini"
                                        onkeyup="aplicarFiltros(true)" placeholder="Folio..." style="width:80px;" />
                                </div>
                                <div style="margin-right:15px;">
                                    Modalidad:
                                    <select id="filtroModalidad" class="filter-select-mini" onchange="aplicarFiltros(true)">
                                        <option value="">TODAS</option>
                                        <option value="INGRESO" selected>INGRESO</option>
                                        <option value="PERIODICO">PERIÓDICO</option>
                                    </select>
                                </div>
                                <div style="margin-right:15px;">
                                    Estatus:
                                    <select id="filtroEstatus" class="filter-select-mini" onchange="aplicarFiltros(true)">
                                        <option value="">TODOS</option>
                                        <option value="-1" selected>PENDIENTES</option>
                                        <option value="3">COMPLETADAS</option>
                                    </select>
                                </div>
                                <div style="margin-left:5px;">
                                    Mostrar:
                                    <select id="selectTamanoPagina" class="filter-select-mini"
                                        onchange="onCambioTamanoPagina()">
                                        <option value="10">10</option>
                                        <option value="25" selected>25</option>
                                        <option value="50">50</option>
                                    </select> por página
                                </div>
                                <button type="button" class="btn-gci btn-gci-secondary" onclick="cargarSolicitudes()"
                                    style="padding:4px 10px; font-size: 0.8rem;"><i class="fas fa-sync-alt"></i>
                                    Actualizar</button>
                            </div>
                        </div>
                        <div class="resultados-info" id="resultsInfo"></div>
                        <div id="connectionStatus"
                            style="display:none; padding:8px 15px; background:#fff3cd; border-left:4px solid #ffc107; margin-bottom:10px; font-size:13px; color:#856404;">
                            <i class="fas fa-exclamation-triangle"></i> <span id="connectionStatusText">Verificando
                                conexión...</span>
                        </div>
                        <table class="gci-table">
                            <thead>
                                <tr>
                                    <th>Folio</th>
                                    <th>Fecha</th>
                                    <th>Modalidad</th>
                                    <th>Persona</th>
                                    <th>No. Emp.</th>
                                    <th>Empresa</th>
                                    <th>Proyecto</th>
                                    <th>Servicio</th>
                                    <th>Estatus</th>
                                    <th>Aptitud</th>
                                    <th style="text-align:center;">Formato(s)</th>
                                </tr>
                            </thead>
                            <tbody id="tbodySolicitudes">
                                <tr>
                                    <td colspan="11" class="no-data">Cargando solicitudes...</td>
                                </tr>
                            </tbody>
                        </table>
                        <div id="paginacionControles" class="paginacion"></div>
                    </div>
                </div>

                <!-- Modal Nueva Solicitud (Ingreso y Periódico) -->
                <div class="modal-overlay" id="modalNuevaSolicitud">
                    <div class="modal-box" style="width: 700px;">
                        <div class="modal-header">
                            <h3><i class="fas fa-file-medical"></i> Solicitud Médica</h3>
                            <button type="button" class="modal-close" onclick="cerrarNuevaSolicitud()"><i
                                    class="fas fa-times"></i></button>
                        </div>
                        <div class="modal-body" style="padding: 25px 30px; background: #fff;">
                            <div id="modalAlert" class="modal-alert error"></div>

                            <!-- Selector de Modalidad -->
                            <div
                                style="display: flex; justify-content: center; margin-bottom: 25px; background: #f1f5f9; padding: 5px; border-radius: 12px;">
                                <button type="button" id="btnTabIngreso" class="btn-gci active"
                                    style="flex:1; border-radius: 10px;" onclick="switchModalidad('INGRESO')">
                                    <i class="fas fa-user-plus"></i> Nuevo Ingreso
                                </button>
                                <button type="button" id="btnTabPeriodico" class="btn-gci"
                                    style="flex:1; border-radius: 10px;" onclick="switchModalidad('PERIODICO')">
                                    <i class="fas fa-user-clock"></i> Periódico / Empleado
                                </button>
                            </div>

                            <!-- Formulario de Ingreso (Candidatos) -->
                            <div id="formIngreso">
                                <div
                                    style="display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin-bottom: 15px;">
                                    <div class="form-group">
                                        <label>Nombre(s) *</label>
                                        <input type="text" id="txtSoloNombre" class="form-control"
                                            placeholder="Ej: Juan Antonio" onkeypress="return soloLetras(event)" />
                                    </div>
                                    <div class="form-group">
                                        <label>Apellido Paterno *</label>
                                        <input type="text" id="txtApePat" class="form-control" placeholder="Ej: Pérez"
                                            onkeypress="return soloLetras(event)" />
                                    </div>
                                </div>

                                <div
                                    style="display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin-bottom: 15px;">
                                    <div class="form-group">
                                        <label>Apellido Materno</label>
                                        <input type="text" id="txtApeMat" class="form-control" placeholder="Ej: García"
                                            onkeypress="return soloLetras(event)" />
                                    </div>
                                </div>
                                
                                <div
                                    style="display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin-bottom: 15px;">
                                    <div class="form-group">
                                        <label>Empresa *</label>
                                        <select id="ddlEmpresaIngreso" class="form-control"
                                            onchange="onEmpresaIngresoChange()">
                                            <option value="">-- Seleccione Empresa --</option>
                                        </select>
                                    </div>
                                    <div class="form-group">
                                        <label>Proyecto *</label>
                                        <select id="ddlProyectoIngreso" class="form-control">
                                            <option value="">-- Primero Empresa --</option>
                                        </select>
                                    </div>
                                </div>

                                <div
                                    style="display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin-bottom: 15px;">
                                    <div class="form-group">
                                        <label>Departamento *</label>
                                        <select id="ddlDepartamentoIngreso" class="form-control" onchange="onDepartamentoIngresoChange()">
                                            <option value="">-- Primero Empresa --</option>
                                        </select>
                                    </div>
                                    <div class="form-group">
                                        <label>Área *</label>
                                        <select id="ddlAreaIngreso" class="form-control" onchange="onAreaIngresoChange()">
                                            <option value="">-- Primero Departamento --</option>
                                        </select>
                                    </div>
                                </div>

                                <div
                                    style="display: grid; grid-template-columns: 1fr; gap: 15px; margin-bottom: 20px;">
                                    <div class="form-group">
                                        <label>Puesto / Vacante *</label>
                                        <select id="ddlPuestoIngreso" class="form-control">
                                            <option value="">-- Primero Área --</option>
                                        </select>
                                    </div>
                                </div>

                            </div>

                            <!-- Formulario de Periódico (Empleados Existentes) -->
                            <div id="formPeriodico" style="display:none;">
                                <div
                                    style="display: flex; gap: 10px; margin-bottom: 20px; align-items: flex-end; background: #f8fafc; padding: 15px; border-radius: 12px; border: 1px solid #e2e8f0;">
                                    <div class="form-group" style="flex: 1;">
                                        <label>Número de Empleado *</label>
                                        <input type="number" id="txtNumEmpleadoBusqueda" class="form-control"
                                            placeholder="Ej: 1234" />
                                    </div>
                                    <button type="button" class="btn-gci btn-gci-primary" onclick="buscarEmpleado()"
                                        style="height: 45px;">
                                        <i class="fas fa-search"></i> Buscar
                                    </button>
                                </div>

                                <div id="infoEmpleadoEncontrado"
                                    style="display:none; margin-bottom:20px; border:1px solid #dcfce7; background:#f0fdf4; padding:15px; border-radius:12px;">
                                    <div style="font-weight:700; color:#166534; margin-bottom:10px;"><i
                                            class="fas fa-check-circle"></i> Empleado Encontrado</div>
                                    <div style="display:grid; grid-template-columns: 2fr 1fr; gap:10px;">
                                        <div>
                                            <label
                                                style="font-size:0.7rem; color:#666; text-transform:uppercase;">Nombre</label>
                                            <div id="lblNombreEmpleado" style="font-weight:600; color:#333;">-</div>
                                        </div>
                                        <div>
                                            <label
                                                style="font-size:0.7rem; color:#666; text-transform:uppercase;">Empresa</label>
                                            <div id="lblEmpresaEmpleado" style="font-weight:600; color:#333;">-</div>
                                        </div>
                                    </div>
                                </div>

                                <div style="display: grid; grid-template-columns: 1fr; gap: 15px; margin-bottom: 12px;">
                                    <div class="form-group">
                                        <label>Tipo de Examen (Empleado) *</label>
                                        <select id="ddlTipoServicioPeriodico" class="form-control"
                                            onchange="actualizarTipoPersonaEmpleado()">
                                            <option value="">-- Seleccione Tipo de Examen --</option>
                                        </select>
                                    </div>
                                </div>

                                <div id="lblTipoPersonaEmpleado" class="status-badge"
                                    style="background:#eff6ff; color:#1d4ed8; border:1px solid #dbeafe; margin-bottom:12px; display:none;">
                                    <i class="fas fa-user-tag"></i> Tipo seleccionado: -
                                </div>

                            </div>

                            <!-- Acciones Unificadas en el Card -->
                            <div class="modal-actions-unified">
                                <button type="button" class="btn-gci btn-gci-secondary"
                                    onclick="cerrarNuevaSolicitud()">Cerrar</button>
                                <button type="button" class="btn-gci btn-gci-primary" id="btnCrearSol"
                                    onclick="crearSolicitud()">Guardar Solicitud</button>
                            </div>
                        </div>
                    </div>
                </div>


                <!-- ══════════ Modal de Detalle ══════════ -->
                <div class="modal-overlay" id="modalDetalle">
                    <div class="modal-box" style="width: 720px;">
                        <div class="modal-header">
                            <h3><i class="fas fa-file-medical-alt"></i> Detalle de Solicitud &mdash; <span
                                    id="modalFolio"></span></h3>
                            <button type="button" class="modal-close" onclick="cerrarModal()"><i
                                    class="fas fa-times"></i></button>
                        </div>
                        <div class="modal-body">
                            <!-- SECCIÓN 1: Organización (Destaque Superior - FIEL A LA ESTRUCTURA INICIAL) -->
                            <div class="detail-section"
                                style="background: #f4f7f9; padding: 15px; border-radius: 8px; margin-bottom: 20px; border-left: 5px solid #1a5276;">
                                <div class="detail-grid"
                                    style="display:grid; grid-template-columns: 1fr 1fr; gap:20px;">
                                    <div class="detail-item">
                                        <label
                                            style="font-size:0.75rem; color:#1a5276; text-transform:uppercase; font-weight:bold; margin-bottom:5px; display:block;"><i
                                                class="fas fa-building"></i> Empresa</label>
                                        <span id="detGeneralEmpresa"
                                            style="font-size: 1.2rem; color: #1a5276; font-weight:500;">&mdash;</span>
                                    </div>
                                    <div class="detail-item">
                                        <label
                                            style="font-size:0.75rem; color:#1a5276; text-transform:uppercase; font-weight:bold; margin-bottom:5px; display:block;"><i
                                                class="fas fa-map-marked-alt"></i> Proyecto / Obra</label>
                                        <span id="detGeneralProyecto"
                                            style="font-size: 1.2rem; color: #1a5276; font-weight:500;">&mdash;</span>
                                    </div>
                                </div>
                            </div>

                            <!-- SECCIÓN 2: Info de la Solicitud -->
                            <div class="detail-section"
                                style="margin-bottom:20px; border-top:1px solid #eee; padding-top:15px;">
                                <div class="ds-title"
                                    style="font-size:0.85rem; color:#1a5276; font-weight:bold; margin-bottom:15px;"><i
                                        class="fas fa-info-circle"></i> Datos de la Solicitud</div>
                                <div class="detail-grid"
                                    style="display:grid; grid-template-columns: repeat(3, 1fr); gap:15px;">
                                    <div class="detail-item"><label
                                            style="font-size:0.7rem; color:#999;">FECHA</label><span id="detFecha"
                                            style="font-size:0.95rem; border-bottom:1px solid #f0f0f0; display:block; padding:3px 0;">&mdash;</span>
                                    </div>
                                    <div class="detail-item"><label
                                            style="font-size:0.7rem; color:#999;">MODALIDAD</label><span
                                            id="detModalidad"
                                            style="font-size:0.95rem; border-bottom:1px solid #f0f0f0; display:block; padding:3px 0;">&mdash;</span>
                                    </div>
                                    <div class="detail-item"><label style="font-size:0.7rem; color:#999;">TIPO DE
                                            SERVICIO</label><span id="detTipoServicio"
                                            style="font-size:0.95rem; border-bottom:1px solid #f0f0f0; display:block; padding:3px 0;">&mdash;</span>
                                    </div>
                                    <div class="detail-item"><label
                                            style="font-size:0.7rem; color:#999;">ESTATUS</label><span id="detEstatus"
                                            style="font-size:0.95rem; border-bottom:1px solid #f0f0f0; display:block; padding:3px 0;">&mdash;</span>
                                    </div>
                                    <div class="detail-item"><label style="font-size:0.7rem; color:#999;">FOLIO
                                            INTERNO</label><span id="detFolio"
                                            style="font-size:0.95rem; border-bottom:1px solid #f0f0f0; display:block; padding:3px 0;">&mdash;</span>
                                    </div>
                                </div>
                            </div>

                            <!-- SECCIÓN 3: Datos de la Persona (Abajo) -->
                            <div class="detail-section" style="border-top:1px solid #eee; padding-top:15px;">
                                <div class="ds-title"
                                    style="font-size:0.85rem; color:#1a5276; font-weight:bold; margin-bottom:15px;"><i
                                        class="fas fa-user"></i> Datos de la Persona</div>

                                <div id="seccionIngresoAviso" class="ingreso-notice"
                                    style="margin-bottom:15px; background: #fff9db; border: 1px solid #ffd43b; color: #856404; padding: 10px; border-radius: 6px; font-size: 0.9rem; display:none;">
                                    <i class="fas fa-user-plus"></i> NUEVO INGRESO: Los datos clínicos se capturarán
                                    durante el examen.
                                </div>

                                <div class="emp-detail-box"
                                    style="background:#fff; border:1px solid #eee; padding:15px; border-radius:8px;">
                                    <div class="detail-grid">
                                        <div class="detail-item" style="grid-column: span 3;">
                                            <label id="lblNomPersona">NOMBRE COMPLETO</label>
                                            <span id="detEmpNombre"
                                                style="font-size: 1.4rem; border:none; color:#333; font-weight:bold; display:block; margin-top:5px;">&mdash;</span>
                                        </div>
                                        <div class="detail-item"><label>NO. EMPLEADO</label><span id="detEmpNum"
                                                style="display:block; padding:3px 0; border-bottom:1px solid #f0f0f0;">&mdash;</span>
                                        </div>
                                        <div class="detail-item"><label>PUESTO</label><span id="detEmpPuesto"
                                                style="display:block; padding:3px 0; border-bottom:1px solid #f0f0f0;">&mdash;</span>
                                        </div>
                                        <!-- NSS Purificado -->
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="modal-actions" id="modalActions">
                            <button type="button" class="btn-gci btn-gci-danger" id="btnEliminar"
                                onclick="eliminarSolicitud()"><i class="fas fa-trash-alt"></i> Eliminar</button>
                            <div style="flex:1;"></div>
                            <button type="button" id="btnImprimirPase" class="btn-gci btn-gci-info"
                                onclick="imprimirPase()"><i class="fas fa-print"></i> Imprimir Pase Médico</button>
                            <button type="button" class="btn-gci btn-gci-secondary"
                                onclick="cerrarModal()">Cerrar</button>
                        </div>
                    </div>
                </div>
                <div class="modal-overlay" id="msgOverlay"
                    style="z-index: 9999; display: none; align-items: center; justify-content: center;">
                    <div class="modal-box"
                        style="width: 450px; text-align: center; padding: 40px; border-radius: 20px; box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);">
                        <div id="msgIcon"
                            style="font-size: 4.5rem; margin-bottom: 20px; transition: transform 0.3s ease;"></div>
                        <h3 id="msgTitle"
                            style="color: #1e293b; margin-bottom: 12px; font-weight: 800; font-size: 1.5rem;"></h3>
                        <p id="msgBody"
                            style="font-size: 1.1rem; color: #64748b; margin-bottom: 30px; line-height: 1.6;"></p>

                        <div id="msgActions"
                            style="display: flex; gap: 12px; justify-content: center; flex-wrap: wrap;">
                            <button type="button" class="btn-gci btn-gci-secondary" onclick="cerrarMsg()"
                                id="btnMsgCancel"
                                style="display: none; min-width: 140px; padding: 12px 24px; font-weight: 600; border-radius: 12px; transition: all 0.2s;">
                                Cancelar
                            </button>
                            <button type="button" class="btn-gci btn-gci-primary" id="btnMsgOk"
                                style="min-width: 140px; padding: 12px 24px; font-weight: 600; border-radius: 12px; transition: all 0.2s;">
                                Aceptar
                            </button>
                            <button type="button" class="btn-gci btn-gci-info" id="btnMsgPrint"
                                style="display: none; min-width: 140px; padding: 12px 24px; font-weight: 600; border-radius: 12px; background: #0ea5e9; border: none; transition: all 0.2s;">
                                <i class="fas fa-print"></i> Imprimir Pase
                            </button>
                        </div>
                    </div>
                </div>

                <!-- ══════════ Modal de Pase Médico (Directo) ══════════ -->
                <div class="modal-overlay" id="modalPase" style="z-index: 10000;">
                    <div class="modal-box"
                        style="width: 850px; max-height: 90vh; display: flex; flex-direction: column;">
                        <div class="modal-header">
                            <h3><i class="fas fa-file-signature"></i> Pase Médico</h3>
                            <button type="button" class="modal-close" onclick="cerrarModalPase()"><i
                                    class="fas fa-times"></i></button>
                        </div>
                        <div class="modal-body" style="flex: 1; overflow: auto; padding: 0;">
                            <div id="paseContent" style="padding: 20px; background: #f5f5f5;">
                                <!-- El contenido del pase se cargará aquí -->
                            </div>
                        </div>
                        <div class="modal-actions">
                            <button type="button" class="btn-gci btn-gci-info" onclick="imprimirPaseModal()">
                                <i class="fas fa-print"></i> Imprimir Pase
                            </button>
                            <div style="flex:1;"></div>
                            <button type="button" class="btn-gci btn-gci-secondary"
                                onclick="cerrarModalPase()">Cerrar</button>
                        </div>
                    </div>
                </div>

            </div>
        </form>

        <script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>
        <script src="https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.2/js/bootstrap.bundle.min.js"></script>

        <!-- Incluir el script separado de esta pantalla -->
        <script src="./js/DashboardRecursosHumanosSM.js"></script>
    </body>

    </html>
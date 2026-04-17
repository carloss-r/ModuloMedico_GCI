var currentOrdenId = null;
var paginaActual = 1;
var registrosPorPagina = 25;
var totalRegistrosGlobal = 0;
var DEFAULT_TIMEOUT = 30000;
var DEFAULT_RETRIES = 2;

$.ajaxSetup({ timeout: DEFAULT_TIMEOUT, cache: false });

function getFriendlyErrorMessage(xhr, status) {
    if (status === 'timeout') return 'La solicitud tardó demasiado. Intente nuevamente.';
    if (xhr && xhr.status === 401) return 'Sesión expirada. Recargue la página.';
    if (xhr && xhr.status === 404) return 'Servicio no disponible (404).';
    if (xhr && xhr.status === 500) return 'Error interno del servidor (500).';
    return 'Error de conexión. Verifique su internet e intente nuevamente.';
}

function renderTableError(errorMsg, retryHandlerName) {
    $('#tbodySolicitudes').html(
        '<tr><td colspan="11" class="no-data"><i class="fas fa-exclamation-circle"></i> ' +
        errorMsg +
        ' <button type="button" class="btn-gci btn-gci-sm btn-gci-primary" onclick="' + retryHandlerName + '()" style="margin-left:10px;">' +
        '<i class="fas fa-sync"></i> Reintentar</button></td></tr>'
    );
}

function apiCall(opts) {
    var retriesLeft = typeof opts.retries === 'number' ? opts.retries : DEFAULT_RETRIES;
    var requestConfig = {
        url: opts.url,
        type: opts.type || 'POST',
        data: opts.data ? JSON.stringify(opts.data) : undefined,
        contentType: opts.contentType || 'application/json; charset=utf-8',
        dataType: opts.dataType || 'json',
        timeout: opts.timeout || DEFAULT_TIMEOUT,
        success: function (response) {
            if (typeof opts.onSuccess === 'function') opts.onSuccess(response);
        },
        error: function (xhr, status, error) {
            if ((status === 'timeout' || (xhr && xhr.status === 0)) && retriesLeft > 0) {
                retriesLeft--;
                setTimeout(function () { $.ajax(requestConfig); }, 700);
                return;
            }

            var msg = getFriendlyErrorMessage(xhr, status);
            console.error('AJAX Error:', opts.url, status, error);
            if (typeof opts.onError === 'function') {
                opts.onError(xhr, status, error, msg);
            } else {
                showNotification('error', 'Error de Sistema', msg);
            }
        }
    };

    $.ajax(requestConfig);
}

// Función para recargar datos
window.recargarDatos = function() {
    $('#tbodySolicitudes').html('<tr class="loading-spinner-row"><td colspan="11"><div class="gci-spinner"></div><span class="gci-loading-text">Cargando solicitudes...</span></td></tr>');
    setTimeout(function() {
        cargarInicial();
    }, 500);
};

$(document).ready(function () {
    cargarInicial();

    // Cerrar menú contextual al hacer clic fuera
    $(document).on('click', function() { $('#ctxMenu').hide(); });

    // Delegación de eventos para cerrar modales al hacer clic en el overlay
    $(document).on('click', '.modal-overlay', function (e) {
        if ($(e.target).hasClass('modal-overlay')) {
            if (e.target.id === 'modalNuevaSolicitud') cerrarNuevaSolicitud();
            else if (e.target.id === 'modalPase') cerrarModalPase();
            else cerrarModal();
        }
    });

    // Monitoreo de conexión
    var lastOnline = navigator.onLine;
    function updateConnectionStatus() {
        var $status = $('#connectionStatus');
        var $text = $('#connectionStatusText');
        var isOnline = navigator.onLine;
        if (isOnline === lastOnline) return;
        lastOnline = isOnline;

        if (!isOnline) {
            $status.css({ display: 'block', background: '#f8d7da', borderLeftColor: '#dc3545', color: '#721c24' });
            $text.text('Sin conexión a Internet. Algunas funciones no estarán disponibles.');
        } else {
            $status.css({ display: 'block', background: '#d4edda', borderLeftColor: '#28a745', color: '#155724' });
            $text.text('Conexión restaurada. Recargando datos...');
            setTimeout(function () {
                $status.hide();
                recargarDatos();
            }, 1200);
        }
    }
    window.addEventListener('online', updateConnectionStatus);
    window.addEventListener('offline', updateConnectionStatus);
    if (!navigator.onLine) updateConnectionStatus();
});

// --- Helper de Notificaciones Modernas ---
window.showNotification = function(type, title, body, callbackOrPkPrint) {
    var $msgOverlay = $('#msgOverlay');
    var $msgIcon = $('#msgIcon');
    var $msgTitle = $('#msgTitle');
    var $msgBody = $('#msgBody');
    var $btnOk = $('#btnMsgOk');
    var $btnPrint = $('#btnMsgPrint');
    var $btnCancel = $('#btnMsgCancel');

    // Reset buttons
    $btnCancel.hide();
    $btnPrint.hide();
    $btnOk.text('Aceptar').off('click').on('click', cerrarMsg);

    // Configurar icono y color según tipo
    if (type === 'success') {
        $msgIcon.html('<i class="fas fa-check-circle" style="color: #10b981;"></i>');
        $msgTitle.css('color', '#059669');
    } else if (type === 'error') {
        $msgIcon.html('<i class="fas fa-times-circle" style="color: #ef4444;"></i>');
        $msgTitle.css('color', '#dc2626');
    } else if (type === 'warning' || type === 'confirm') {
        $msgIcon.html('<i class="fas fa-exclamation-triangle" style="color: #f59e0b;"></i>');
        $msgTitle.css('color', '#d97706');
    } else {
        $msgIcon.html('<i class="fas fa-info-circle" style="color: #3b82f6;"></i>');
        $msgTitle.css('color', '#2563eb');
    }

    $msgTitle.text(title);
    $msgBody.text(body);

    // Lógica especial para confirmación
    if (type === 'confirm' && typeof callbackOrPkPrint === 'function') {
        $btnCancel.show();
        $btnOk.text('Confirmar').off('click').on('click', function() {
            cerrarMsg();
            callbackOrPkPrint();
        });
    } 
    // Lógica para impresión en éxito
    else if (typeof callbackOrPkPrint === 'number' || (typeof callbackOrPkPrint === 'string' && !isNaN(callbackOrPkPrint))) {
        $btnPrint.show().off('click').on('click', function() {
            cerrarMsg();
            mostrarModalPase(callbackOrPkPrint);
        });
        $btnOk.text('Cerrar');
    }

    $msgOverlay.css('display', 'flex').hide().fadeIn(300);
};

window.cerrarMsg = function() {
    $('#msgOverlay').fadeOut(200, function() {
        $(this).hide();
    });
};

// --- Funciones Globales (Disponibles para onclick) ---

window.cargarInicial = function() {
    apiCall({
        url: 'DashboardRecursosHumanosSM.aspx/CargarInicial',
        contentType: 'application/json',
        onSuccess: function (r) {
            var resp = r.d;
            if (resp.success) {
                var $ddlPer = $('#ddlTipoServicioPeriodico');
                var $ddlEmp = $('#ddlEmpresaIngreso');

                $ddlPer.empty().append('<option value="">-- Seleccione Tipo de Examen --</option>');
                $ddlEmp.find('option:gt(0)').remove();

                $.each(resp.tiposServicio || [], function(_, s) {
                    var txt = (s.Descripcion || '').toUpperCase();
                    var esAntidoping = txt.indexOf('ANTIDOP') >= 0;
                    var esPeriodico = txt.indexOf('PERIOD') >= 0;
                    if (esAntidoping || esPeriodico) {
                        $ddlPer.append('<option value="' + s.Id + '">' + s.Descripcion + '</option>');
                    }
                });

                $.each(resp.empresas || [], function(_, e) {
                    $ddlEmp.append('<option value="' + e.Id + '">' + e.Descripcion + '</option>');
                });
            }
        },
        onError: function (xhr, status, error, msg) {
            renderTableError(msg, 'recargarDatos');
        }
    });

    aplicarFiltros(true);
};

window.cargarSolicitudes = function() { aplicarFiltros(); };

window.aplicarFiltros = function(resetPage) {
    if (resetPage) paginaActual = 1;

    var req = {
        pagina: paginaActual,
        tamanoPagina: registrosPorPagina,
        filtroNumEmpleado: $('#filtroNumEmp').length ? ($('#filtroNumEmp').val() ? parseInt($('#filtroNumEmp').val()) : null) : null,
        filtroModalidad: null,
        filtroEstatus: ($('#filtroEstatus').val() === '-1') ? -1 : ($('#filtroEstatus').val() ? parseInt($('#filtroEstatus').val()) : null),
        fechaDesde: $('#filtroFechaDesde').val() || null,
        fechaHasta: $('#filtroFechaHasta').val() || null,
        filtroEmpresa: null,
        filtroArea: null,
        filtroAnio: null,
        filtroSemana: null,
        filtroNombre: $('#filtroEmpleadoMini').val() || null
    };

    $('#tbodySolicitudes').html('<tr class="loading-spinner-row"><td colspan="11"><div class="gci-spinner"></div><span class="gci-loading-text">Cargando solicitudes...</span></td></tr>');

    apiCall({
        url: 'DashboardRecursosHumanosSM.aspx/CargarPagina',
        data: req,
        onSuccess: function(r) {
            var resp = r.d;
            if (resp.success) {
                totalRegistrosGlobal = resp.total;
                renderPaginaMed(resp.data);
            } else {
                renderTableError(resp.message || 'Error al cargar datos.', 'aplicarFiltros');
            }
        },
        onError: function(xhr, status, error, msg) {
            renderTableError(msg, 'aplicarFiltros');
        }
    });
};

function renderPaginaMed(paginaDatos) {
    var $tbody = $('#tbodySolicitudes');
    var totalPaginas = Math.max(1, Math.ceil(totalRegistrosGlobal / registrosPorPagina));
    var inicio = (paginaActual - 1) * registrosPorPagina;
    var fin = Math.min(inicio + registrosPorPagina, totalRegistrosGlobal);

    $tbody.empty();
    if (paginaDatos.length > 0) {
        $('#resultsInfo').text('Mostrando ' + (inicio + 1) + '-' + fin + ' de ' + totalRegistrosGlobal + ' solicitudes.');
        
        paginaDatos.forEach(function(s) {
            var estLow = (s.EstatusDesc || '').toLowerCase();
            var badgeEst = 'badge-pendiente';
            if (estLow.indexOf('proceso') >= 0) badgeEst = 'badge-proceso';
            if (estLow.indexOf('complet') >= 0) badgeEst = 'badge-completado';

            // Modalidad badge (Text only)
            var badgeMod = s.Modalidad === 'INGRESO' ? 'badge-ingreso' : 'badge-periodico';
            
            // Indicadores de exámenes completados - Removidos para limpiar la UI
            var examIndicators = '';

            // Aptitud badge (solo si está completada)
            var aptitudHtml = '';
            if (estLow.indexOf('complet') >= 0) {
                aptitudHtml = '<span class="status-badge badge-apto"><i class="fas fa-check-double"></i> APTO</span>';
            } else {
                aptitudHtml = '<span style="color:#ccc;">-</span>';
            }

            var row = '<tr data-pk="' + s.PkOrdenMedico + '" onclick="verDetalle(' + s.PkOrdenMedico + ')">' +
                '<td><strong>' + s.FolioDisplay + '</strong></td>' +
                '<td>' + (s.FechaOrdenFormateada || '-') + '</td>' +
                '<td><span class="status-badge ' + badgeMod + '">' + (s.Modalidad || 'INGRESO') + '</span></td>' +
                '<td>' + (s.NombrePersona || 'N/A') + '</td>' +
                '<td>' + (s.FkEmpleado || 'N/A') + '</td>' +
                '<td>' + (s.EmpresaNombre || s.EmpresaCandidato || '-') + '</td>' +
                '<td>' + (s.ProyectoDesc || '-') + '</td>' +
                '<td>' + (s.TipoServicioDesc || '-') + '</td>' +
                '<td>' +
                    '<span class="status-badge ' + badgeEst + '">' + (s.EstatusDesc || 'Pendiente') + '</span>' +
                    '<div style="margin-top:4px; display:flex; flex-direction:column; gap:2px;">' + examIndicators + '</div>' +
                '</td>' +
                '<td style="text-align:center;">' + aptitudHtml + '</td>' +
                '<td style="text-align:center;">' +
                   '<div style="display:flex; justify-content:center; gap:5px;">' +
                      '<button type="button" class="btn-action" title="Ver Detalle" onclick="event.stopPropagation(); verDetalle(' + s.PkOrdenMedico + ')"><i class="fas fa-eye"></i></button>' +
                      '<button type="button" class="btn-action" title="Ver Pase" onclick="event.stopPropagation(); mostrarModalPase(' + s.PkOrdenMedico + ')"><i class="fas fa-file-signature"></i></button>' +
                   '</div>' +
                '</td>' +
                '</tr>';
            $tbody.append(row);
        });
    } else {
        $tbody.html('<tr><td colspan="11" class="no-data">No se encontraron solicitudes.</td></tr>');
    }

    $('#paginacionControles').html(buildPaginacion(paginaActual, totalPaginas));
}

window.verDetalle = function(pkOrden) {
    currentOrdenId = pkOrden;
    
    // Mostrar modal con estado de carga
    $('#modalFolio').text('Cargando...');
    $('#modalDetalle').addClass('active');
    
    apiCall({
        url: 'DashboardRecursosHumanosSM.aspx/VerDetalle',
        contentType: 'application/json',
        data: { id: pkOrden },
        timeout: 15000,
        onSuccess: function(r) {
            var resp = r.d;
            if (!resp.success) { 
                cerrarModal();
                showNotification('error', 'No se pudo cargar', resp.message || 'Error al cargar el detalle'); 
                return; 
            }
            var o = resp.orden;
            
            $('#modalFolio').text(o.FolioDisplay);
            $('#detFolio').text(o.FolioDisplay);
            $('#detFecha').text(o.FechaOrdenFormateada);
            $('#detGeneralEmpresa').text(o.EmpresaNombre || '-');
            $('#detGeneralProyecto').text(o.ProyectoDesc || '-');
            $('#detModalidad').text(o.Modalidad || '-');
            $('#detTipoServicio').text(o.TipoServicioDesc || '-');
            
            // Si Modalidad y Tipo de Servicio son iguales, ocultamos uno para no repetir
            if ((o.Modalidad || '').toLowerCase() === (o.TipoServicioDesc || '').toLowerCase()) {
                $('#detModalidad').closest('.field').hide();
            } else {
                $('#detModalidad').closest('.field').show();
            }

            $('#detEstatus').text(o.EstatusDesc || 'Pendiente');

            // Poblar Datos de la Persona (Unificado con aviso dinámico)
            $('#detEmpNombre').text(o.NombrePersona || '-');
            
            if (o.Modalidad === 'INGRESO') {
                $('#seccionIngresoAviso').show();
                $('#lblNomPersona').text('NOMBRE DEL CANDIDATO');
                $('#detEmpNum').text('-'); 
                $('#detEmpPuesto').text(o.PuestoCandidato || '-');
                $('#detEmpNss').text(o.NssCandidato || '-');
            } else {
                $('#seccionIngresoAviso').hide();
                $('#lblNomPersona').text('NOMBRE COMPLETO');
                var emp = resp.empleado || {};
                $('#detEmpNum').text(emp.PkEmpleado || o.FkEmpleado || '-');
                $('#detEmpPuesto').text(emp.PuestoDesc || o.PuestoCandidato || '-');
                $('#detEmpNss').text(emp.Nss || o.NssCandidato || '-');
            }

            // Configurar botón de impresión dentro del detalle
            $('#btnImprimirPase').off('click').on('click', function() {
                mostrarModalPase(o.PkOrdenMedico);
            });

            $('#btnEliminar').toggle(o.EstatusDesc.toLowerCase().indexOf('complet') < 0);
        },
        onError: function(xhr, status, error, msg) {
            cerrarModal();
            showNotification('error', 'Error de Respuesta', msg + '\n\nSi el problema persiste, recargue la página (F5).');
        }
    });
};

window.imprimirPase = function() {
    if (!currentOrdenId) {
        showNotification('warning', 'Atención', 'Seleccione una solicitud primero.');
        return;
    }
    mostrarModalPase(currentOrdenId);
};

window.mostrarModalPase = function(pkOrdenMedico) {
    // Cerrar modal de detalle si está abierto
    $('#modalDetalle').removeClass('active');
    
    // Mostrar indicador de carga
    $('#paseContent').html('<div style="text-align:center; padding:40px;"><div class="gci-spinner"></div><p>Cargando pase...</p></div>');
    $('#modalPase').addClass('active');
    
    apiCall({
        url: 'DashboardRecursosHumanosSM.aspx/ObtenerPaseHtml',
        data: { pkOrdenMedico: pkOrdenMedico },
        timeout: 15000,
        onSuccess: function(r) {
            if (r.d && r.d.success) {
                $('#paseContent').html(r.d.html);
            } else {
                var errorMsg = (r.d && r.d.message) ? r.d.message : 'Error al cargar el pase';
                $('#paseContent').html('<div style="text-align:center; padding:40px; color:#d32f2f;"><i class="fas fa-exclamation-circle" style="font-size:48px; margin-bottom:15px;"></i><p>' + errorMsg + '</p><button type="button" class="btn-gci btn-gci-primary" onclick="mostrarModalPase(' + pkOrdenMedico + ')" style="margin-top:15px;"><i class="fas fa-sync"></i> Reintentar</button></div>');
            }
        },
        onError: function(xhr, status, error, msg) {
            $('#paseContent').html('<div style="text-align:center; padding:40px; color:#d32f2f;"><i class="fas fa-exclamation-circle" style="font-size:48px; margin-bottom:15px;"></i><p>' + msg + '</p><button type="button" class="btn-gci btn-gci-primary" onclick="mostrarModalPase(' + pkOrdenMedico + ')" style="margin-top:15px;"><i class="fas fa-sync"></i> Reintentar</button></div>');
        }
    });
};

window.cerrarModalPase = function() {
    $('#modalPase').removeClass('active');
    $('#paseContent').html('');
};

window.imprimirPaseModal = function() {
    var contenido = document.getElementById('paseContent').innerHTML;
    var ventana = window.open('', '_blank', 'width=800,height=600,scrollbars=yes');
    if (ventana) {
        ventana.document.write('<!DOCTYPE html><html><head><title>Pase Médico</title></head><body>' + contenido + '</body></html>');
        ventana.document.close();
        ventana.focus();
        setTimeout(function() {
            ventana.print();
        }, 500);
    } else {
        showNotification('warning', 'Bloqueador de Elementos Emergentes', 'Por favor, permita las ventanas emergentes en su navegador para poder imprimir el formato.');
    }
};

window.cerrarModal = function() { $('.modal-overlay').removeClass('active'); };

window.eliminarSolicitud = function() {
    if (!currentOrdenId) {
        showNotification('warning', 'Atención', 'Seleccione una solicitud primero.');
        return;
    }
    
    // Cambio de confirm de navegador por modal personalizado
    showNotification('confirm', 'Confirmar Eliminación', '¿Está seguro de que desea eliminar permanentemente esta solicitud? Esta acción no se puede deshacer.', function() {
        apiCall({
            url: 'DashboardRecursosHumanosSM.aspx/Eliminar',
            data: { pkOrdenMedico: currentOrdenId },
            onSuccess: function (r) {
                var resp = r.d;
                if (resp && resp.success) {
                    cerrarModal();
                    aplicarFiltros(true);
                    showNotification('success', '¡Eliminado!', resp.message || 'Solicitud eliminada correctamente.');
                } else {
                    showNotification('error', 'Error', (resp && resp.message) ? resp.message : 'No fue posible eliminar la solicitud.');
                }
            },
            onError: function(xhr, status, error, msg) {
                showNotification('error', 'Error de Conexión', msg);
            }
        });
    });
};

window.abrirNuevaSolicitud = function() {
    $('#modalNuevaSolicitud').addClass('active');
    $('#modalAlert').hide();
    switchModalidad('INGRESO'); // Default
    $('#txtSoloNombre, #txtApePat, #txtApeMat').val('');
    $('#txtNumEmpleadoBusqueda').val('');
    $('#ddlTipoServicioPeriodico').val('');
    $('#lblTipoPersonaEmpleado').hide();
    $('#infoEmpleadoEncontrado').hide();
    $('#ddlEmpresaIngreso').val('');
    $('#ddlProyectoIngreso, #ddlPuestoIngreso').html('<option value="">-- Primero Empresa --</option>');
};

var currentModalidad = 'INGRESO';
window.switchModalidad = function(tipo) {
    currentModalidad = (tipo === 'PERIODICO') ? 'PERIODICO' : 'INGRESO';
    if (currentModalidad === 'PERIODICO') {
        $('#btnTabPeriodico').addClass('active').siblings().removeClass('active');
        $('#formIngreso').hide();
        $('#formPeriodico').show();
        actualizarTipoPersonaEmpleado();
    } else {
        $('#btnTabIngreso').addClass('active').siblings().removeClass('active');
        $('#formIngreso').show();
        $('#formPeriodico').hide();
    }
    $('#modalAlert').hide();
};

window.actualizarTipoPersonaEmpleado = function() {
    var txt = ($('#ddlTipoServicioPeriodico option:selected').text() || '').trim();
    if (!txt || txt.indexOf('Seleccione') >= 0) {
        $('#lblTipoPersonaEmpleado').hide();
        return;
    }

    var upper = txt.toUpperCase();
    var tipoExamen = upper.indexOf('ANTIDOP') >= 0 ? 'ANTIDOPING' : 'PERIÓDICO';
    $('#lblTipoPersonaEmpleado').html('<i class="fas fa-user-tag"></i> Tipo de persona: EMPLEADO — Examen: ' + tipoExamen).show();
};

window.buscarEmpleado = function() {
    var num = $('#txtNumEmpleadoBusqueda').val();
    if (!num) return;

    apiCall({
        url: 'DashboardRecursosHumanosSM.aspx/BuscarEmpleado',
        contentType: 'application/json',
        data: { numeroEmpleado: parseInt(num) },
        onSuccess: function(r) {
            if (r.d.success) {
                var e = r.d.empleado;
                $('#lblNombreEmpleado').text(e.NombreCompleto);
                $('#lblEmpresaEmpleado').text(e.EmpresaDesc);
                $('#infoEmpleadoEncontrado').show();
                $('#modalAlert').hide();
            } else {
                $('#infoEmpleadoEncontrado').hide();
                $('#modalAlert').text(r.d.message).show();
            }
        }
    });
};

window.cerrarNuevaSolicitud = function() { $('#modalNuevaSolicitud').removeClass('active'); };

window.onEmpresaIngresoChange = function() {
    var idEmp = $('#ddlEmpresaIngreso').val();
    if (!idEmp) return;

    apiCall({
        url: 'DashboardRecursosHumanosSM.aspx/ObtenerProyectosPorEmpresa',
        contentType: 'application/json',
        data: { fkEmpresa: parseInt(idEmp) },
        onSuccess: function(r) {
            var $ddl = $('#ddlProyectoIngreso');
            $ddl.empty().append('<option value="">-- Seleccione Proyecto --</option>');
            if (r.d.success) {
                $.each(r.d.data, function(_, p) { $ddl.append('<option value="' + p.Id + '">' + p.Descripcion + '</option>'); });
            }
        }
    });

    apiCall({
        url: 'DashboardRecursosHumanosSM.aspx/ObtenerPuestosPorEmpresa',
        contentType: 'application/json',
        data: { fkEmpresa: parseInt(idEmp) },
        onSuccess: function(r) {
            var $ddl = $('#ddlPuestoIngreso');
            $ddl.empty().append('<option value="">-- Seleccione Puesto --</option>');
            if (r.d.success) {
                $.each(r.d.data, function(_, p) { $ddl.append('<option value="' + p.Id + '">' + p.Descripcion + '</option>'); });
            }
        }
    });
};

window.crearSolicitud = function() {
    var data = {
        Modalidad: currentModalidad,
        NumeroEmpleado: currentModalidad === 'PERIODICO' ? (parseInt($('#txtNumEmpleadoBusqueda').val()) || null) : null,
        FkTipoServicio: currentModalidad === 'PERIODICO' ? (parseInt($('#ddlTipoServicioPeriodico').val()) || 0) : 0,
        NombreCandidato: $('#txtSoloNombre').val(),
        ApellidoPaterno: $('#txtApePat').val(),
        ApellidoMaterno: $('#txtApeMat').val(),
        PuestoDesc: $('#ddlPuestoIngreso option:selected').text(),
        PuestoDeseado: $('#ddlPuestoIngreso option:selected').text(),
        EmpresaDesc: $('#ddlEmpresaIngreso option:selected').text(),
        ProyectoDesc: $('#ddlProyectoIngreso option:selected').text(),
        FkEmpresa: parseInt($('#ddlEmpresaIngreso').val()) || null,
        FkProyecto: parseInt($('#ddlProyectoIngreso').val()) || null,
        Sexo: ''
    };

    if (currentModalidad === 'INGRESO') {
        if (!data.NombreCandidato || !data.ApellidoPaterno || !data.FkEmpresa) {
            showNotification('warning', 'Campos Incompletos', 'Por favor complete los campos obligatorios del candidato (Nombres, Apellido Paterno y Empresa).');
            return;
        }
    } else {
        if (!data.NumeroEmpleado || !$('#infoEmpleadoEncontrado').is(':visible')) {
            showNotification('warning', 'Empleado No Seleccionado', 'Por favor busque y seleccione un empleado de la lista antes de continuar.');
            return;
        }
        if (!data.FkTipoServicio) {
            showNotification('warning', 'Tipo de Examen', 'Debe seleccionar el tipo de examen que se realizará al empleado.');
            return;
        }
    }

    apiCall({
        url: 'DashboardRecursosHumanosSM.aspx/CrearSolicitud',
        contentType: 'application/json',
        data: data,
        onSuccess: function(r) {
            if(r.d.success) {
                cerrarNuevaSolicitud();
                aplicarFiltros(true);
                
                // NOTIFICACIÓN DE ÉXITO PREMIUM CON OPCIÓN DE IMPRESIÓN
                showNotification(
                    'success', 
                    '¡Éxito!', 
                    'La solicitud ha sido creada correctamente en el sistema. ¿Desea imprimir el pase médico en este momento?', 
                    r.d.pkOrdenMedico
                );
            } else {
                showNotification('error', 'Error al Crear', r.d.message);
            }
        }
    });
};

window.onCambioTamanoPagina = function() {
    var size = parseInt($('#selectTamanoPagina').val(), 10);
    registrosPorPagina = isNaN(size) ? 25 : size;
    aplicarFiltros(true);
};

function buildPaginacion(actual, total) {
    if (total <= 1) return '';
    var html = [];
    
    // Ant
    html.push('<button class="pag-btn" ' + (actual <= 1 ? 'disabled' : '') + ' onclick="irAPagina(' + (actual - 1) + ')" title="Anterior"><i class="fas fa-chevron-left"></i></button>');

    // Páginas
    var start = Math.max(1, actual - 2);
    var end = Math.min(total, start + 4);
    if (end - start < 4) start = Math.max(1, end - 4);

    for (var i = start; i <= end; i++) {
        html.push('<button class="pag-btn ' + (i === actual ? 'active' : '') + '" onclick="irAPagina(' + i + ')">' + i + '</button>');
    }

    // Sig
    html.push('<button class="pag-btn" ' + (actual >= total ? 'disabled' : '') + ' onclick="irAPagina(' + (actual + 1) + ')" title="Siguiente"><i class="fas fa-chevron-right"></i></button>');
    
    return '<div class="paginacion-wrapper">' + html.join('') + '</div>';
}

window.irAPagina = function(num) {
    paginaActual = num;
    aplicarFiltros(false);
};

window.soloLetras = function(e) {
    var key = e.keyCode || e.which;
    var tecla = String.fromCharCode(key).toLowerCase();
    var letras = " áéíóúabcdefghijklmnñopqrstuvwxyz";
    return letras.indexOf(tecla) !== -1 || key == 8;
};

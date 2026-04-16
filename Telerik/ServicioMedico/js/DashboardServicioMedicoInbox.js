/* Lógica de MédicaDashboardServicioMedico.aspx */
$(document).ready(function () {
    paginaActual = 1;
    registrosPorPagina = 25;
    totalRegistrosGlobal = 0;

    cargarInicial();

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
                aplicarFiltros(true);
            }, 1200);
        }
    }

    window.addEventListener('online', updateConnectionStatus);
    window.addEventListener('offline', updateConnectionStatus);
    if (!navigator.onLine) updateConnectionStatus();
});

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
                alert(msg);
            }
        }
    };

    $.ajax(requestConfig);
}

function renderTableError(msg) {
    $('#tbodySolicitudes').html(
        '<tr><td colspan="10" class="no-data"><i class="fas fa-exclamation-circle"></i> ' +
        msg +
        ' <button type="button" class="btn-gci btn-gci-sm btn-gci-primary" onclick="aplicarFiltros(true)" style="margin-left:10px;">' +
        '<i class="fas fa-sync"></i> Reintentar</button></td></tr>'
    );
}

function cargarInicial() {
    aplicarFiltros(true);
}

// La previsualización del examen se muestra en modal con iframe
function verEvaluacionPreview(idOrden) {
    abrirVistaPrevia('ImpresionFormatos.aspx?id=' + idOrden + '&tipo=EXAMEN', 'Examen Médico');
}

function getTipoSangre(fk){
    var map = {1:'O+',2:'O-',3:'A+',4:'A-',5:'B+',6:'B-',7:'AB+',8:'AB-'};
    return map[fk] || '';
}


function setBox(id, on){
    var $b = $('#' + id);
    if(!$b.length) return;
    $b.toggleClass('on', !!on);
}

function setOptLine(id, on){
    var $e = $('#' + id);
    if(!$e.length) return;
    $e.text(on ? 'X' : '');
}

function mkMale(lbl, val){
    return '<div class="m-item"><span class="box' + (val ? ' on' : '') + '"></span><span>' + lbl + '</span></div>';
}

function mapCurva(v){
    // N: Normal / A: Aumentada / D: Disminuida (el VM usa int?)
    if(v === null || v === undefined) return '';
    if(v === 'N' || v === 'A' || v === 'D') return v;
    var n = parseInt(v,10);
    if(n === 1) return 'N';
    if(n === 2) return 'A';
    if(n === 3) return 'D';
    return '';
}

function safeVal(v){
    if(v === null || v === undefined) return '';
    return (''+v).toUpperCase();
}

function formatDate(d){
    if(!d) return '';
    // Soporta /Date( )/ o ISO
    if(typeof d === 'string'){
        var m = d.match(/\/Date\((\d+)\)\//);
        if(m) {
            var dt = new Date(parseInt(m[1],10));
            return pad2(dt.getDate()) + '/' + pad2(dt.getMonth()+1) + '/' + dt.getFullYear();
        }
        if(/^\d{4}-\d{2}-\d{2}/.test(d)){
            return d.substring(8,10) + '/' + d.substring(5,7) + '/' + d.substring(0,4);
        }
        return d;
    }
    try{
        var dt2 = new Date(d);
        if(isNaN(dt2.getTime())) return '';
        return pad2(dt2.getDate()) + '/' + pad2(dt2.getMonth()+1) + '/' + dt2.getFullYear();
    }catch(e){ return ''; }
}

function pad2(n){ return (n < 10 ? '0' : '') + n; }

function escapeHtml(s){
    if(s === null || s === undefined) return '';
    return (''+s)
        .replace(/&/g,'&amp;')
        .replace(/</g,'&lt;')
        .replace(/>/g,'&gt;')
        .replace(/"/g,'&quot;')
        .replace(/\'/g,'&#39;');
}

function cargarSolicitudes() { aplicarFiltros(); }

function aplicarFiltros(resetPage) {
    if (resetPage) paginaActual = 1;
    
    var estatusVal = $('#filtroEstatus').val();
    var filtroEstatus = (estatusVal && estatusVal !== "-1") ? parseInt(estatusVal) : null;
    var soloPendientes = (estatusVal === "-1");

    var req = {
        pagina: paginaActual,
        tamanoPagina: registrosPorPagina,
        filtroNumEmpleado: $('#filtroNumEmp').val() ? parseInt($('#filtroNumEmp').val()) : null,
        filtroModalidad: $('#filtroModalidad').val() || null,
        filtroEstatus: soloPendientes ? -1 : filtroEstatus,
        fechaDesde: $('#filtroFechaDesde').val() || null,
        fechaHasta: null,
        filtroEmpresa: null,
        filtroArea: null,
        filtroAnio: null,
        filtroSemana: null
    };

    $('#tbodySolicitudes').html('<tr><td colspan="10" class="no-data">Cargando bandeja médica...</td></tr>');

    apiCall({
        url: 'DashboardServicioMedico.aspx/CargarPagina',
        data: req,
        onSuccess: function (r) {
            var resp = r.d;
            if (resp.success) {
                totalRegistrosGlobal = resp.total;
                renderPagina(resp.data);
            } else {
                renderTableError(resp.message || 'No fue posible cargar la bandeja.');
            }
        },
        onError: function (xhr, status, err, msg) {
            renderTableError(msg);
        }
    });
}

function renderPagina(datos) {
    var $tbody = $('#tbodySolicitudes');
    $tbody.empty();

    if (!datos || datos.length === 0) {
        $tbody.append('<tr><td colspan="10" class="no-data">No hay solicitudes pendientes en este momento.</td></tr>');
        renderPaginacion();
        return;
    }

    datos.forEach(function (s) {
        var estLow = (s.EstatusDesc || '').toLowerCase();
        var badgeEst = 'badge-pendiente';
        if (estLow.indexOf('proceso') >= 0) badgeEst = 'badge-proceso';
        if (estLow.indexOf('complet') >= 0) badgeEst = 'badge-completado';

        // Modalidad badge (Text only)
        var modClass = (s.Modalidad === 'INGRESO' ? 'badge-ingreso' : 'badge-periodico');

        // Indicadores de exámenes completados (Solo texto coloreado)
        var examIndicators = '';
        if (s.TieneEvaluacion) {
            examIndicators += '<div class="status-badge badge-med" title="Evaluación Médica Completada"><i class="fas fa-heartbeat"></i> MÉDICO OK</div>';
        }
        if (s.TieneAntidoping) {
            examIndicators += '<div class="status-badge badge-tox" title="Antidoping Completado"><i class="fas fa-vial"></i> TOX OK</div>';
        }

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
            '<td><span class="status-badge ' + modClass + '">' + (s.Modalidad || 'INGRESO') + '</span></td>' +
            '<td><strong>' + (s.NombrePersona || 'SIN NOMBRE') + '</strong></td>' +
            '<td>' + (s.EmpresaNombre || '-') + '</td>' +
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
                   '<button type="button" class="btn-action" title="Ver Evaluación Médica" onclick="event.stopPropagation(); verEvaluacionPreview(' + s.PkOrdenMedico + ')"><i class="fas fa-file-medical"></i></button>' +
                   '<button type="button" class="btn-action" title="Ver Pase Médico" onclick="event.stopPropagation(); verPaseEmpleado(' + s.PkOrdenMedico + ')"><i class="fas fa-file-signature"></i></button>' +
                   (s.TieneAntidoping ? '<button type="button" class="btn-action" title="Ver Antidoping" onclick="event.stopPropagation(); verAntidoping(' + s.PkOrdenMedico + ')"><i class="fas fa-vial"></i></button>' : '') +
               '</div>' +
            '</td>' +
            '</tr>';
        $tbody.append(row);
    });

    renderPaginacion();
}

function renderPaginacion() {
    var totalPaginas = Math.ceil(totalRegistrosGlobal / registrosPorPagina) || 1;
    var $cont = $('#paginacionControles');
    $cont.empty();

    if (totalPaginas <= 1) return;

    // Lógica simplificada de paginación
    $cont.append('<button onclick="cambiarPagina(' + (paginaActual - 1) + ')" ' + (paginaActual === 1 ? 'disabled' : '') + '>Ant.</button>');
    $cont.append('<span>Página ' + paginaActual + ' de ' + totalPaginas + '</span>');
    $cont.append('<button onclick="cambiarPagina(' + (paginaActual + 1) + ')" ' + (paginaActual === totalPaginas ? 'disabled' : '') + '>Sig.</button>');
}

function cambiarPagina(p) {
    if (p < 1) return;
    paginaActual = p;
    aplicarFiltros();
}

function onCambioTamanoPagina() {
    registrosPorPagina = parseInt($('#selectTamanoPagina').val()) || 25;
    aplicarFiltros(true);
}

function verDetalle(id) {
    // Construir endpoint sobre el path actual para evitar problemas de rutas/base href/virtual dirs.
    var endpoint = window.location.pathname + '/VerDetalle';

    apiCall({
        url: endpoint,
        data: { id: id },
        onSuccess: function (r) {
            var resp = r.d;
            if (resp && resp.success) {
                var o = resp.orden;
                $('#modalFolio').text(o.FolioDisplay);
                $('#detGeneralEmpresa').text(o.EmpresaNombre);
                $('#detGeneralProyecto').text(o.ProyectoDesc);
                $('#detModalidad').text(o.Modalidad);
                $('#detTipoServicio').text(o.TipoServicioDesc);
                $('#detEstatus').text(o.EstatusDesc);

                $('#detPacienteNombre').text(o.NombrePersona);
                
                if (o.Modalidad === 'INGRESO') {
                    $('#seccionIngresoNotice').show();
                    $('#lblNomPersona').text('NOMBRE DEL CANDIDATO');
                    $('#detPacienteEmpNum').text('-');
                    $('#detPacientePuesto').text(o.PuestoCandidato || '-');
                    $('#detPacienteNss').text(o.NssCandidato || '-');
                } else {
                    $('#seccionIngresoNotice').hide();
                    $('#lblNomPersona').text('NOMBRE DEL EMPLEADO');
                    var emp = resp.empleado || {};
                    $('#detPacienteEmpNum').text(emp.PkEmpleado || o.FkEmpleado || '-');
                    $('#detPacientePuesto').text(emp.PuestoDesc || o.PuestoCandidato || '-');
                    $('#detPacienteNss').text(emp.Nss || o.NssCandidato || '-');
                }

                // Acciones específicas para el Médico
                var wizardUrl = 'EvaluacionMedica.aspx?id=' + o.PkOrdenMedico;
                $('#btnIrEvaluar').attr('href', wizardUrl).toggle(o.FkEstatus === 1); // Solo si está pendiente
                
                // Botón Resultados si ya está completada
                if (o.FkEstatus === 3) {
                     $('#btnVerResultados').attr('href', '#').off('click').on('click', function(e){
                         e.preventDefault();
                         verEvaluacionPreview(o.PkOrdenMedico);
                     }).show();
                } else {
                     $('#btnVerResultados').hide();
                }

                $('#modalDetalle').addClass('active');
            } else {
                alert((resp && resp.message) ? resp.message : 'No fue posible cargar el detalle.');
            }
        },
        onError: function (xhr, status, err, msg) {
            alert(msg);
        }
    });
}

function cerrarModal() { $('.modal-overlay').removeClass('active'); }

function limpiarFiltros() {
    $('#filtroNumEmp').val('');
    $('#filtroModalidad').val('');
    $('#filtroEstatus').val('-1');
    $('#filtroFechaDesde').val('');
    aplicarFiltros(true);
}

// ==================== MODAL EVALUACION EMPLEADO ====================
var empleadoSeleccionado = null;

function abrirModalEvaluacionEmpleado() {
    $('#modalEvaluacionEmpleado').show();
    $('#txtNumEmpleado').val('').focus();
    $('#empResultado').html('');
    $('#btnCrearEvaluacion').hide();
    empleadoSeleccionado = null;
}

function cerrarModalEvaluacionEmpleado() {
    $('#modalEvaluacionEmpleado').hide();
    $('#txtNumEmpleado').val('');
    $('#empResultado').html('');
    $('#btnCrearEvaluacion').hide();
    empleadoSeleccionado = null;
}

function buscarEmpleado() {
    var numEmp = $('#txtNumEmpleado').val();
    if (!numEmp || numEmp <= 0) {
        $('#empResultado').html('<div class="emp-not-found"><i class="fas fa-exclamation-circle"></i> Ingrese un número de empleado válido</div>');
        $('#btnCrearEvaluacion').hide();
        return;
    }

    $('#empResultado').html('<div style="text-align:center; padding:20px;"><i class="fas fa-spinner fa-spin"></i> Buscando...</div>');

    apiCall({
        url: 'DashboardServicioMedico.aspx/BuscarEmpleado',
        data: { numeroEmpleado: parseInt(numEmp) },
        onSuccess: function (r) {
            var resp = r.d;
            if (resp.success && resp.empleado) {
                empleadoSeleccionado = resp.empleado;
                mostrarDatosEmpleado(resp.empleado);
                $('#btnCrearEvaluacion').show();
            } else {
                $('#empResultado').html('<div class="emp-not-found"><i class="fas fa-user-slash"></i> ' + (resp.message || 'Empleado no encontrado') + '</div>');
                $('#btnCrearEvaluacion').hide();
                empleadoSeleccionado = null;
            }
        },
        onError: function (xhr, status, err, msg) {
            $('#empResultado').html('<div class="emp-not-found"><i class="fas fa-exclamation-triangle"></i> ' + msg + '</div>');
            $('#btnCrearEvaluacion').hide();
            empleadoSeleccionado = null;
        }
    });
}

function mostrarDatosEmpleado(emp) {
    var html = '<div class="emp-data-card">';
    html += '<div class="emp-data-row"><div class="emp-data-label">Número:</div><div class="emp-data-value">' + escapeHtml(emp.NumeroEmpleado || '') + '</div></div>';
    html += '<div class="emp-data-row"><div class="emp-data-label">Nombre:</div><div class="emp-data-value">' + escapeHtml(emp.NombreCompleto || '') + '</div></div>';
    html += '<div class="emp-data-row"><div class="emp-data-label">CURP:</div><div class="emp-data-value">' + escapeHtml(emp.Curp || '') + '</div></div>';
    html += '<div class="emp-data-row"><div class="emp-data-label">NSS:</div><div class="emp-data-value">' + escapeHtml(emp.Nss || '') + '</div></div>';
    html += '<div class="emp-data-row"><div class="emp-data-label">Puesto:</div><div class="emp-data-value">' + escapeHtml(emp.Puesto || '') + '</div></div>';
    html += '<div class="emp-data-row"><div class="emp-data-label">Empresa:</div><div class="emp-data-value">' + escapeHtml(emp.Empresa || '') + '</div></div>';
    
    // Mostrar si tiene evaluación previa
    if (emp.DatosPrevios) {
        html += '<div style="margin-top:12px; padding-top:10px; border-top:1px dashed #aaa;">';
        html += '<strong style="color:#28a745;"><i class="fas fa-history"></i> Se encontró evaluación previa - los datos se heredarán</strong>';
        html += '</div>';
    }
    
    html += '</div>';
    $('#empResultado').html(html);
}

function crearEvaluacionEmpleado() {
    if (!empleadoSeleccionado) {
        alert('Primero busque y seleccione un empleado');
        return;
    }

    if (!empleadoSeleccionado.PkOrdenMedico) {
        alert('No se pudo crear la orden de evaluación');
        return;
    }

    // Cerrar modal y mostrar el pase generado
    cerrarModalEvaluacionEmpleado();
    verPaseEmpleado(empleadoSeleccionado.PkOrdenMedico);
}

// Abre el Pase Médico en modal con iframe para vista previa e impresión
window.verPaseEmpleado = function(pkOrdenMedico) {
    abrirVistaPrevia('ImpresionFormatos.aspx?id=' + pkOrdenMedico + '&tipo=PASE', 'Pase Médico');
};

window.mostrarModalPase = window.verPaseEmpleado;

// ── Control del modal de Vista Previa con iframe ──────────────────────
function abrirVistaPrevia(url, titulo) {
    $('#modalVistaPreviaTitle').html('<i class="fas fa-file-medical"></i> Vista Previa — ' + titulo);
    var $frame = $('#iframeVistaPrevia');
    $frame.attr('src', 'about:blank');
    $('#modalVistaPrevia').addClass('active');
    // Cargar url en el iframe (ligero delay para que el modal sea visible primero)
    setTimeout(function() { $frame.attr('src', url); }, 80);
}

function cerrarVistaPrevia() {
    $('#modalVistaPrevia').removeClass('active');
    // Limpiar el iframe para liberar memoria
    setTimeout(function() { $('#iframeVistaPrevia').attr('src', 'about:blank'); }, 300);
}

function imprimirVistaPrevia() {
    var frame = document.getElementById('iframeVistaPrevia');
    if (frame && frame.contentWindow) {
        frame.contentWindow.focus();
        frame.contentWindow.print();
    }
}



// Permitir buscar con Enter en el campo de número de empleado
$(document).on('keypress', '#txtNumEmpleado', function(e) {
    if (e.which === 13) {
        buscarEmpleado();
    }
});

// Abre el Antidoping en modal con iframe para vista previa e impresión
function verAntidoping(pkOrdenMedico) {
    abrirVistaPrevia('Formatos/Antidoping.html?id=' + pkOrdenMedico, 'Antidoping');
}


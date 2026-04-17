/* Lógica de DashboardServicioMedico.aspx */
var paginaActual = 1;
var registrosPorPagina = 25;
var totalRegistrosGlobal = 0;

$(document).ready(function () {
    // Inicialización de estado por sesión de navegador
    paginaActual = 1;
    registrosPorPagina = parseInt($('#selectTamanoPagina').val()) || 25;
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

    // Delegación de eventos para cerrar modales
    $(document).on('click', '.modal-overlay', function (e) {
        if ($(e.target).hasClass('modal-overlay')) {
            var targetId = e.target.id;
            if (targetId === 'modalPase') cerrarModalPase();
            else if (targetId === 'modalVistaPrevia') cerrarVistaPrevia();
            else cerrarModal();
        }
    });
});

function cargarInicial() {
    aplicarFiltros(true);
}

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

    $('#tbodySolicitudes').html('<tr><td colspan="9" class="no-data">Cargando bandeja médica...</td></tr>');

    apiCall({
        url: 'DashboardServicioMedico.aspx/CargarPagina',
        data: req,
        onSuccess: function (r) {
            var resp = r.d;
            if (resp && resp.success) {
                totalRegistrosGlobal = resp.total;
                renderPagina(resp.data);
            } else {
                renderTableError(resp ? resp.message : 'No fue posible cargar la bandeja.');
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
        $tbody.append('<tr><td colspan="9" class="no-data">No hay solicitudes en este momento.</td></tr>');
        renderPaginacion();
        return;
    }

    datos.forEach(function (s) {
        var estLow = (s.EstatusDesc || '').toLowerCase();
        var badgeEst = 'badge-pendiente';
        if (estLow.indexOf('proceso') >= 0) badgeEst = 'badge-proceso';
        if (estLow.indexOf('complet') >= 0) badgeEst = 'badge-completado';

        var aptitudHtml = '';
        if (estLow.indexOf('complet') >= 0) {
            aptitudHtml = '<span class="status-badge badge-apto"><i class="fas fa-check-double"></i> APTO</span>';
        } else {
            aptitudHtml = '<span style="color:#ccc;">-</span>';
        }

        var row = '<tr data-pk="' + s.PkOrdenMedico + '" onclick="verDetalle(' + s.PkOrdenMedico + ')">' +
            '<td><strong>' + s.FolioDisplay + '</strong></td>' +
            '<td>' + (s.FechaOrdenFormateada || '-') + '</td>' +
            '<td><strong>' + (s.NombrePersona || 'SIN NOMBRE') + '</strong></td>' +
            '<td>' + (s.EmpresaNombre || s.EmpresaCandidato || '-') + '</td>' +
            '<td>' + (s.ProyectoDesc || '-') + '</td>' +
            '<td>' + (s.TipoServicioDesc || '-') + '</td>' +
            '<td><span class="status-badge ' + badgeEst + '">' + (s.EstatusDesc || 'Pendiente') + '</span></td>' +
            '<td>' + aptitudHtml + '</td>' +
            '<td style="text-align:center;">' +
               '<div style="display:flex; justify-content:center; gap:5px;">' +
                   '<button type="button" class="btn-action" title="Ver Detalle" onclick="event.stopPropagation(); verDetalle(' + s.PkOrdenMedico + ')"><i class="fas fa-eye"></i></button>' +
                   (s.TieneEvaluacion ? '<button type="button" class="btn-action" title="Ver Evaluación Médica" onclick="event.stopPropagation(); verEvaluacionPreview(' + s.PkOrdenMedico + ')"><i class="fas fa-file-medical"></i></button>' : '') +
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

    if (totalPaginas <= 1 && totalRegistrosGlobal <= registrosPorPagina) {
        $cont.hide();
        return;
    }
    $cont.show();

    var html = '<div class="paginacion-wrapper" style="display:flex; justify-content:center; align-items:center; gap:8px; margin: 20px 0;">';
    html += '<button type="button" class="pag-btn" onclick="cambiarPagina(' + (paginaActual - 1) + '); return false;" ' + (paginaActual === 1 ? 'disabled style="opacity:0.5;"' : '') + '><i class="fas fa-chevron-left"></i></button>';

    var start = Math.max(1, paginaActual - 2);
    var end = Math.min(totalPaginas, start + 4);
    if (end - start < 4) start = Math.max(1, end - 4);

    for (var i = start; i <= end; i++) {
        var activeStyle = (i === paginaActual) ? 'background:#007bff; color:#fff;' : '';
        html += '<button type="button" class="pag-btn" style="' + activeStyle + '" onclick="cambiarPagina(' + i + '); return false;">' + i + '</button>';
    }

    html += '<button type="button" class="pag-btn" onclick="cambiarPagina(' + (paginaActual + 1) + '); return false;" ' + (paginaActual === totalPaginas ? 'disabled style="opacity:0.5;"' : '') + '><i class="fas fa-chevron-right"></i></button>';
    html += '</div>';
    html += '<div class="pag-info text-center" style="font-size:12px; color:#666; margin-bottom:20px;">' + totalRegistrosGlobal + ' registros | Página ' + paginaActual + ' de ' + totalPaginas + '</div>';

    $cont.html(html);
}

function cambiarPagina(p) {
    if (p < 1) return;
    paginaActual = p;
    aplicarFiltros(false);
}

function onCambioTamanoPagina() {
    registrosPorPagina = parseInt($('#selectTamanoPagina').val()) || 25;
    aplicarFiltros(true);
}

function verDetalle(id) {
    apiCall({
        url: 'DashboardServicioMedico.aspx/VerDetalle',
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
                
                // Configurar botones de acción
                var estLow = (o.EstatusDesc || '').toLowerCase();
                if (estLow.indexOf('complet') >= 0) {
                    $('#btnIrEvaluar').hide();
                } else {
                    $('#btnIrEvaluar').show().attr('href', 'EvaluacionMedica.aspx?id=' + id);
                }
                
                // Botón Ver Examen
                if (o.TieneEvaluacion) {
                    $('#btnVerExamen').show().off('click').on('click', function(e) {
                        e.preventDefault();
                        verEvaluacionPreview(id);
                    });
                } else {
                    $('#btnVerExamen').hide();
                }

                // Botón Ver Antidoping
                if (o.TieneAntidoping) {
                    $('#btnVerAntidoping').show().off('click').on('click', function(e) {
                        e.preventDefault();
                        verAntidoping(id);
                    });
                } else {
                    $('#btnVerAntidoping').hide();
                }

                // Botón Ver Pase
                $('#btnVerPase').off('click').on('click', function(e) {
                    e.preventDefault();
                    verPaseEmpleado(id);
                });

                if (o.Modalidad === 'INGRESO') {
                    $('#seccionIngresoNotice').show();
                    $('#lblNomPersona').text('NOMBRE DEL CANDIDATO');
                } else {
                    $('#seccionIngresoNotice').hide();
                    $('#lblNomPersona').text('NOMBRE DEL EMPLEADO');
                }
                $('#modalDetalle').addClass('active');
            }
        }
    });
}

function cerrarModal() { $('.modal-overlay').removeClass('active'); }

function verPaseEmpleado(id) {
    abrirVistaPrevia('ImpresionFormatos.aspx?id=' + id + '&tipo=PASE', 'Pase Médico');
}

function cerrarModalPase() {
    $('#modalPase').removeClass('active');
    $('#iframePase').attr('src', 'about:blank');
}

function abrirVistaPrevia(url, titulo) {
    $('#vpTitulo').text(titulo);
    $('#iframeVistaPrevia').attr('src', url);
    $('#modalVistaPrevia').addClass('active');
}

function cerrarVistaPrevia() {
    $('#modalVistaPrevia').removeClass('active');
    $('#iframeVistaPrevia').attr('src', 'about:blank');
}

function verEvaluacionPreview(id) {
    abrirVistaPrevia('ImpresionFormatos.aspx?id=' + id + '&tipo=EXAMEN', 'Evaluación Médica');
}

function verAntidoping(id) {
    abrirVistaPrevia('ImpresionFormatos.aspx?id=' + id + '&tipo=ANTIDOPING', 'Resultado Antidoping');
}

function imprimirVistaPrevia() {
    var iframe = document.getElementById('iframeVistaPrevia');
    if (iframe && iframe.contentWindow) {
        iframe.contentWindow.focus();
        iframe.contentWindow.print();
    } else {
        alert('No se pudo encontrar el contenido para imprimir.');
    }
}

function imprimirPaseModal() {
    // Si el pase médico se carga en un div (paseContent) como en RRHH
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
        alert('Por favor, permita las ventanas emergentes para imprimir.');
    }
}

function renderTableError(msg) {
    $('#tbodySolicitudes').html('<tr><td colspan="9" class="no-data" style="color:#dc3545;"><i class="fas fa-exclamation-circle"></i> ' + escapeHtml(msg) + '</td></tr>');
}

function apiCall(opts) {
    $.ajax({
        url: opts.url,
        type: 'POST',
        data: JSON.stringify(opts.data || {}),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (r) { if (opts.onSuccess) opts.onSuccess(r); },
        error: function (x, s, e) { if (opts.onError) opts.onError(x, s, e, 'Error de red'); }
    });
}

function escapeHtml(s) {
    return s ? String(s).replace(/[&<>"']/g, function(m) { return {'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[m]; }) : '';
}

/* --- Funciones para Nueva Evaluación de Empleado --- */

function abrirModalEvaluacionEmpleado() {
    $('#txtNumEmpleado').val('');
    $('#empResultado').empty();
    $('#btnCrearEvaluacion').hide();
    $('#modalEvaluacionEmpleado').fadeIn(200);
}

function cerrarModalEvaluacionEmpleado() {
    $('#modalEvaluacionEmpleado').fadeOut(200);
}

var empleadoSeleccionado = null;

function buscarEmpleado() {
    var num = $('#txtNumEmpleado').val();
    if (!num) {
        $('#empResultado').html('<div class="alert alert-warning">Ingrese un número de empleado.</div>');
        return;
    }

    $('#empResultado').html('<div class="text-center"><i class="fas fa-spinner fa-spin"></i> Buscando...</div>');

    apiCall({
        url: 'DashboardServicioMedico.aspx/BuscarEmpleado',
        data: { numeroEmpleado: parseInt(num) },
        onSuccess: function (r) {
            var resp = r.d;
            if (resp && resp.success) {
                var e = resp.empleado;
                empleadoSeleccionado = e;
                var html = '<div class="emp-card-result">' +
                    '<h5><i class="fas fa-user-check"></i> ' + e.NombreCompleto + '</h5>' +
                    '<p><strong>Puesto:</strong> ' + (e.Puesto || '-') + '<br>' +
                    '<strong>Empresa:</strong> ' + (e.Empresa || '-') + '</p>';
                
                if (e.PkOrdenMedico) {
                    html += '<div class="alert alert-info" style="font-size:13px;"><i class="fas fa-info-circle"></i> Este empleado ya tiene una orden técnica generada.</div>';
                }
                
                html += '</div>';
                $('#empResultado').html(html);
                $('#btnCrearEvaluacion').show();
            } else {
                $('#empResultado').html('<div class="alert alert-danger">' + (resp ? resp.message : 'No se encontró el empleado.') + '</div>');
                $('#btnCrearEvaluacion').hide();
                empleadoSeleccionado = null;
            }
        },
        onError: function () {
            $('#empResultado').html('<div class="alert alert-danger">Error de comunicación con el servidor.</div>');
        }
    });
}

function iniciarCreacionEvaluacion() {
    if (!empleadoSeleccionado) return;

    if (empleadoSeleccionado.PkOrdenMedico) {
        window.location.href = 'EvaluacionMedica.aspx?id=' + empleadoSeleccionado.PkOrdenMedico;
    } else {
        apiCall({
            url: 'DashboardServicioMedico.aspx/CrearOrdenEvaluacion',
            data: { pkEmpleado: empleadoSeleccionado.PkEmpleado },
            onSuccess: function (r) {
                if (r.d && r.d.success) {
                    window.location.href = 'EvaluacionMedica.aspx?id=' + r.d.PkOrdenMedico;
                } else {
                    alert('Error al crear la evaluación: ' + (r.d ? r.d.message : 'Desconocido'));
                }
            }
        });
    }
}
